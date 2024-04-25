using CommentTranslator22.Translate.Format;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Translate.Server
{
    internal class BingFanyi
    {
        public async Task<ApiRecvFormat> FanyiAsync(ApiSendFormat format)
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
                Success = true,
                Code = (int)response.StatusCode,
                Message = response.StatusCode.ToString(),
                ResultText = r
            };
        }
    }
}
