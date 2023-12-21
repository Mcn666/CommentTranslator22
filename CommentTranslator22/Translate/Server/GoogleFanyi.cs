using CommentTranslator22.Translate.Enum;
using CommentTranslator22.Translate.Format;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CommentTranslator22.Translate.Server
{
    internal class GoogleFanyi
    {
        public async Task<ApiRecvFormat> FanyiAsync(ApiSendFormat format)
        {
            var client = new HttpClient();
            string from = LanguageCode.Code[ServerEnum.Google.GetHashCode()][format.SourceLanguage.GetHashCode()];
            string to = LanguageCode.Code[ServerEnum.Google.GetHashCode()][format.TargetLanguage.GetHashCode()];
            string r = "";
            string url = "https://translate.google.com/_/TranslateWebserverUi/data/batchexecute";
            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(url));
            IDictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("f.req", $"[[[\"MkEWBc\",\"[[\\\"{format.SourceText}\\\",\\\"{from}\\\",\\\"{to}\\\",true],[null]]\", null, \"generic\"]]]");
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
                Success = true,
                Code = (int)response.StatusCode,
                Message = response.StatusCode.ToString(),
                SourceText = format.SourceText,
                ResultText = r
            };
        }
    }
}
