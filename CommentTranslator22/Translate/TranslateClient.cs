using CommentTranslator22.Translate.Format;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Translate
{
    public class TranslateClient
    {
        public static TranslateClient Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            internal static TranslateClient instance = new TranslateClient();

            static Nested() { }
        }

        TranslateClient()
        {

        }

        public int MaxTranslateLength { get; private set; } = 300;

        public int MinTranslateLength { get; private set; } = 15;

        public async Task<ApiRecvFormat> TranslateAsync(string str)
        {
            var res = Preprocessing(str);
            if (res != null)
            {
                return new ApiRecvFormat
                {
                    Message = res,
                    SourceText = str
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
            switch (CommentTranslator22Package.Config.TranslationServer)
            {
                case ServerEnum.Bing:
                    return await TranslateServer.BingAsync(apiRequest);
                case ServerEnum.Google:
                    return await TranslateServer.GoogleAsync(apiRequest);
                case ServerEnum.Baidu:
                    {
                        var i = CommentTranslator22Package.Config.AppId;
                        var k = CommentTranslator22Package.Config.SecretKey;
                        if (string.IsNullOrEmpty(k) || string.IsNullOrEmpty(i))
                        {
                            return new ApiRecvFormat();
                        }
                        return await TranslateServer.BaiduAsync(apiRequest, i, k);
                    }
                default:
                    return new ApiRecvFormat();
            }
        }

        public string Preprocessing(string text)
        {
            switch (CommentTranslator22Package.Config.TargetLanguage)
            {
                case LanguageEnum.English:
                    if (LanguageProportion.English(text) > 0.5f)
                        return "EN?";
                    break;
                case LanguageEnum.简体中文:
                    if (LanguageProportion.Chinese(text) > 0.5f)
                        return "CN?";
                    break;
                case LanguageEnum.日本語:
                    if (LanguageProportion.Japanese(text) > 0.5f)
                        return "JA?";
                    break;
            }

            if (text.Length < MinTranslateLength || text.Length > MaxTranslateLength)
            {
                return "LEN";
            }

            if (CommentTranslator22Package.Config.SourceLanguage == CommentTranslator22Package.Config.TargetLanguage ||
                CommentTranslator22Package.Config.TargetLanguage == LanguageEnum.Auto)
            {
                return "?>?";
            }

            return null;
        }

        public string HumpUnfold(string humpString)
        {
            humpString = humpString.Replace("\r\n", "\n");
            var ss = humpString.Split(' ');
            var result = "";
            foreach (var s in ss)
            {
                var matcher = Regex.Matches(s, "([A-Z]|^)[a-z]+");
                if (matcher.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (Match match in matcher)
                    {
                        string g = match.Groups[0].Value;
                        sb.Append(g + " ");
                    }

                    result += sb.ToString().TrimEnd() + " ";
                }
                else
                {
                    result += s + " ";
                }
            }
            return result;
        }
    }
}
