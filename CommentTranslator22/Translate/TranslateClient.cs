using CommentTranslator22.Arithmetic;
using CommentTranslator22.Translate.Enum;
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

        public async Task<ApiRecvFormat> TranslateAsync(string text)
        {
            var res = LocalTranslateData.SeekTranslateResult(text);
            if (res != null)
            {
                return new ApiRecvFormat
                {
                    Success = true,
                    Code = -1,
                    SourceText = text,
                    ResultText = res,
                };
            }

            if (Preprocessing(text) == true)
            {
                return new ApiRecvFormat();
            }

            var request = new ApiSendFormat()
            {
                SourceLanguage = CommentTranslator22Package.ConfigA.SourceLanguage,
                TargetLanguage = CommentTranslator22Package.ConfigA.TargetLanguage,
                SourceText = text,
            };

            return await ExecuteAsync(request);
        }

        public async Task<ApiRecvFormat> ExecuteAsync(ApiSendFormat apiRequest)
        {
            apiRequest.SourceText = apiRequest.SourceText.Replace("\r\n", "\n");
            apiRequest.SourceText = HumpUnfold(apiRequest.SourceText);

            switch (CommentTranslator22Package.ConfigA.TranslationServer)
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

        private bool Preprocessing(string text)
        {
            if (CommentTranslator22Package.ConfigA.SourceLanguage == CommentTranslator22Package.ConfigA.TargetLanguage ||
                CommentTranslator22Package.ConfigA.TargetLanguage == LanguageEnum.Auto)
            {
                return true;
            }

            if (text.Length < MinTranslateLength ||
                text.Length > MaxTranslateLength)
            {
                return true;
            }

            switch (CommentTranslator22Package.ConfigA.TargetLanguage)
            {
                case LanguageEnum.English:
                    if (LanguageProportion.English(text) > 0.7f)
                        return true;
                    break;
                case LanguageEnum.简体中文:
                    if (LanguageProportion.Chinese(text) > 0.7f)
                        return true;
                    break;
            }

            if (LocalTranslateData.SeekAwaitTranslateText(text))
            {
                return true;
            }
            return false;
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
