using CommentTranslator22.Translate.Format;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace CommentTranslator22.Translate
{
    internal static class TranslateServer
    {
        static string GetLanguageCode(ServerEnum server, LanguageEnum language)
        {
            return LanguageCode.Code[server.GetHashCode()][language.GetHashCode()];
        }

        public static async Task<ApiRecvFormat> BingAsync(ApiSendFormat format)
        {
            var client = new HttpClient();
            string r = "";
            string url = "https://cn.bing.com/translator";
            var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
            HttpResponseMessage response = await client.SendAsync(request);
            string html = await response.Content.ReadAsStringAsync();
            Regex regex = new Regex("params_AbusePreventionHelper = \\[(.+?),\"(.+?)\",.+?");
            var match = regex.Match(html);
            string token = match.Groups[2].Value;
            string key = match.Groups[1].Value;
            regex = new Regex("\"ig\":\"(.+?)\",");
            match = regex.Match(html);
            string ig = match.Groups[1].Value;
            string from = GetLanguageCode(ServerEnum.Bing, format.SourceLanguage);
            string to = GetLanguageCode(ServerEnum.Bing, format.TargetLanguage);

            url = $"https://cn.bing.com/ttranslatev3?IG={ig}&IID=translator.5028";
            request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            IDictionary<string, string> dic = new Dictionary<string, string>
            {
                { "fromLang", from },
                { "text", format.SourceText },
                { "to", to },
                { "token", token },
                { "key", key }
            };
            var data = new FormUrlEncodedContent(dic);
            request.Content = data;
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
            response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                html = System.Text.Encoding.UTF8.GetString(bytes);
                var doc = Newtonsoft.Json.Linq.JArray.Parse(html);

                r = doc[0]["translations"][0]["text"].ToString();
            }

            return new ApiRecvFormat()
            {
                IsSuccess = true,
                Code = response.StatusCode,
                SourceText = format.SourceText,
                TargetText = r
            };
        }

        public static async Task<ApiRecvFormat> GoogleAsync(ApiSendFormat format)
        {
            var client = new HttpClient();
            string from = GetLanguageCode(ServerEnum.Google, format.SourceLanguage);
            string to = GetLanguageCode(ServerEnum.Google, format.TargetLanguage);
            string r = "";
            string url = "https://translate.google.com/_/TranslateWebserverUi/data/batchexecute";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            IDictionary<string, string> dic = new Dictionary<string, string>
            {
                { "f.req", $"[[[\"MkEWBc\",\"[[\\\"{format.SourceText}\\\",\\\"{from}\\\",\\\"{to}\\\",true],[null]]\", null, \"generic\"]]]" }
            };
            var data = new FormUrlEncodedContent(dic);
            request.Content = data;
            HttpResponseMessage response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var bytes = await response.Content.ReadAsByteArrayAsync();
                string html = System.Text.Encoding.UTF8.GetString(bytes);
                html = html.Replace("\\n", "").Replace(")]}'", "");
                var jo = Newtonsoft.Json.Linq.JArray.Parse(html);
                jo = Newtonsoft.Json.Linq.JArray.Parse(jo[0][2].ToString());

                r = jo[1][0][0][5][0][0].ToString();
            }

            return new ApiRecvFormat()
            {
                IsSuccess = true,
                Code = response.StatusCode,
                SourceText = format.SourceText,
                TargetText = r
            };
        }


        #region 百度翻译
        public class BaiduTranslationResult
        {
            public string src;
            public string dst;
        }

        public class BaiduTranslationResponse
        {
            public string from;
            public string to;
            public List<BaiduTranslationResult> trans_result;
        }

        /// <summary>
        /// 计算MD5值
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static string EncryptString(string str)
        {
            using (MD5 md5 = MD5.Create())
            {
                // 将字符串转换成字节数组
                byte[] byteOld = Encoding.UTF8.GetBytes(str);
                // 调用加密方法
                byte[] byteNew = md5.ComputeHash(byteOld);
                // 将加密结果转换为字符串
                StringBuilder sb = new StringBuilder();
                foreach (byte b in byteNew)
                {
                    // 将字节转换成16进制表示的字符串
                    sb.Append(b.ToString("x2"));
                }
                // 返回加密的字符串
                return sb.ToString();
            }
        }

        public static async Task<ApiRecvFormat> BaiduAsync(ApiSendFormat format, string appid, string key)
        {
            var from = GetLanguageCode(ServerEnum.Baidu, format.SourceLanguage);
            var to = GetLanguageCode(ServerEnum.Baidu, format.TargetLanguage);
            var salt = new Random().Next(100000).ToString();
            var sign = EncryptString(appid + format.SourceText + salt + key);
            var url = $"http://api.fanyi.baidu.com/api/trans/vip/translate?q={HttpUtility.UrlEncode(format.SourceText)}&from={from}&to={to}&appid={appid}&salt={salt}&sign={sign}";

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                try
                {
                    var response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode(); // 检查HTTP响应状态码
                    var retString = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(retString) == false)
                    {
                        var json = JsonConvert.DeserializeObject<BaiduTranslationResponse>(retString);
                        if (json != null && json.trans_result.Count > 0 && json.trans_result[0].dst != null)
                        {
                            return new ApiRecvFormat()
                            {
                                IsSuccess = true,
                                Code = response.StatusCode,
                                SourceText = format.SourceText,
                                TargetText = json.trans_result[0].dst,
                            };
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    return new ApiRecvFormat();
                }
            }
            return new ApiRecvFormat();
        }

        #endregion
    }
}
