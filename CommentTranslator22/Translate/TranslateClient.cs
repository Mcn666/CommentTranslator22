using CommentTranslator22.Translate.Format;
using CommentTranslator22.Translate.Server;
using CommentTranslator22.Translate.TranslateData;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Translate
{
    public class TranslateClient
    {
        public int MaxTranslateLength { get; private set; } = 300;

        public int MinTranslateLength { get; private set; } = 15;

        public async Task<ApiRecvFormat> TranslateAsync(string str)
        {
            var res = LocalTranslateData.SeekTranslateResult(str);
            if (res != null)
            {
                return new ApiRecvFormat
                {
                    Message = "buf",
                    ResultText = res
                };
            }

            res = Preprocessing(str);
            if (res != null)
            {
                return new ApiRecvFormat
                {
                    Message = res
                };
            }

            var request = new ApiSendFormat()
            {
                SourceLanguage = CommentTranslator22Package.Config.SourceLanguage,
                TargetLanguage = CommentTranslator22Package.Config.TargetLanguage,
                SourceText = str
            };

            return await ExecuteAsync(request);
        }

        public async Task<ApiRecvFormat> ExecuteAsync(ApiSendFormat apiRequest)
        {
            apiRequest.SourceText = apiRequest.SourceText.Replace("\r\n", "\n");
            apiRequest.SourceText = HumpUnfold(apiRequest.SourceText);

            switch (CommentTranslator22Package.Config.TranslationServer)
            {
                case ServerEnum.Bing:
                    BingFanyi bingFanyi = new BingFanyi();
                    return await bingFanyi.FanyiAsync(apiRequest);
                case ServerEnum.Google:
                    GoogleFanyi googleFanyi = new GoogleFanyi();
                    return await googleFanyi.FanyiAsync(apiRequest);
                default:
                    return new ApiRecvFormat();
            }
        }

        private string Preprocessing(string text)
        {
            if (CommentTranslator22Package.Config.SourceLanguage == CommentTranslator22Package.Config.TargetLanguage ||
                CommentTranslator22Package.Config.TargetLanguage == LanguageEnum.Auto)
            {
                return "?>?";
            }

            if (text.Length < MinTranslateLength || text.Length > MaxTranslateLength)
            {
                return "Len";
            }

            switch (CommentTranslator22Package.Config.TargetLanguage)
            {
                case LanguageEnum.English:
                    if (LanguageProportion.English(text) > 0.6f)
                        return "EN?";
                    break;
                case LanguageEnum.简体中文:
                    if (LanguageProportion.Chinese(text) > 0.6f)
                        return "CN?";
                    break;
                case LanguageEnum.日本語:
                    if (LanguageProportion.Japanese(text) > 0.6f)
                        return "JA?";
                    break;
            }
            return null;
        }

        private string HumpUnfold(string humpString)
        {
            string[] ss = humpString.Split(' ');
            string res = "";
            foreach (var s in ss)
            {
                Regex regex = new Regex("([A-Z]|^)[a-z]+");
                var matcher = regex.Matches(s);
                if (matcher.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Match match in matcher)
                    {
                        string g = match.Groups[0].Value;
                        sb.Append(g + " ");
                    }

                    res += sb.ToString().TrimEnd() + " ";
                }
                else
                {
                    res += s + " ";
                }
            }
            return res;
        }
    }
}
