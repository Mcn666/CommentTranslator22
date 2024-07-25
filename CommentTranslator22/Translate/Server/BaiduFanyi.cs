using CommentTranslator22.Translate.Format;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CommentTranslator22.Translate.Server
{
    internal class BaiduFanyi
    {
        public class BaiduTranslationResult
        {
            public string src = null;
            public string dst = null;
        }

        public class BaiduTranslationResponse
        {
            public string from = null;
            public string to = null;
            public List<BaiduTranslationResult> trans_result = null;
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
            var from = TranslateServer.GetLanguageCode(ServerEnum.Baidu, format.SourceLanguage);
            var to = TranslateServer.GetLanguageCode(ServerEnum.Baidu, format.TargetLanguage);
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
                catch (HttpRequestException)
                {
                    return new ApiRecvFormat();
                }
            }
            return new ApiRecvFormat();
        }
    }
}
