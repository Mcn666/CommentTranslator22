using CommentTranslator22.Translate.Format;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Translate
{
    internal static class TranslateServer
    {
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
            string from = LanguageCode.Code[ServerEnum.Bing.GetHashCode()][format.SourceLanguage.GetHashCode()];
            string to = LanguageCode.Code[ServerEnum.Bing.GetHashCode()][format.TargetLanguage.GetHashCode()];

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
                Code = (int)response.StatusCode,
                Message = response.StatusCode.ToString(),
                SourceText = format.SourceText,
                TargetText = r
            };
        }

        public static async Task<ApiRecvFormat> GoogleAsync(ApiSendFormat format)
        {
            var client = new HttpClient();
            string from = LanguageCode.Code[ServerEnum.Google.GetHashCode()][format.SourceLanguage.GetHashCode()];
            string to = LanguageCode.Code[ServerEnum.Google.GetHashCode()][format.TargetLanguage.GetHashCode()];
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
                Code = (int)response.StatusCode,
                Message = response.StatusCode.ToString(),
                SourceText = format.SourceText,
                TargetText = r
            };
        }

        //public static async Task<ApiRecvFormat> BaiduAsync(ApiSendFormat format)
        //{
        //    return null;
        //}
    }
}
