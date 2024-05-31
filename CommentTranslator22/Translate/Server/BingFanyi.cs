using CommentTranslator22.Translate.Format;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Translate.Server
{
    internal class BingFanyi
    {
        public class DetectedLanguage
        {
            [JsonProperty("language")]
            public string Language { get; set; }

            [JsonProperty("score")]
            public double Score { get; set; }
        }

        public class Translation
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("transliteration")]
            public Transliteration Transliteration { get; set; }

            [JsonProperty("to")]
            public string To { get; set; }

            [JsonProperty("sentLen")]
            public SentLen SentLen { get; set; }
        }

        public class Transliteration
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("script")]
            public string Script { get; set; }
        }

        public class SentLen
        {
            [JsonProperty("srcSentLen")]
            public int[] SrcSentLen { get; set; }

            [JsonProperty("transSentLen")]
            public int[] TransSentLen { get; set; }
        }

        public class TranslationResponse
        {
            [JsonProperty("detectedLanguage")]
            public DetectedLanguage DetectedLanguage { get; set; }

            [JsonProperty("translations")]
            public Translation[] Translations { get; set; }
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
            string from = TranslateServer.GetLanguageCode(ServerEnum.Bing, format.SourceLanguage);
            string to = TranslateServer.GetLanguageCode(ServerEnum.Bing, format.TargetLanguage);

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
                html = Encoding.UTF8.GetString(bytes);
                var doc = Newtonsoft.Json.Linq.JArray.Parse(html);

                r = doc[0]["translations"][0]["text"].ToString();
                //var bingRecv = JsonConvert.DeserializeObject<TranslationResponse[]>(html);
            }

            return new ApiRecvFormat()
            {
                IsSuccess = true,
                Code = response.StatusCode,
                SourceText = format.SourceText,
                TargetText = r
            };
        }
    }
}
