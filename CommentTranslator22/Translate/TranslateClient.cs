using CommentTranslator22.Arithmetic;
using CommentTranslator22.Translate.Enum;
using CommentTranslator22.Translate.Format;
using CommentTranslator22.Translate.Server;
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
                    Body = text,
                    Data = res,
                };
            }

            if (Preprocessing(text) == true)
            {
                return new ApiRecvFormat();
            }

            var request = new ApiSendFormat()
            {
                FromLanguage = CommentTranslator22Package.ConfigA.LanguageFrom,
                ToLanguage = CommentTranslator22Package.ConfigA.LanguageTo,
                Body = text,
            };

            return await ExecuteAsync(request);
        }

        public async Task<ApiRecvFormat> ExecuteAsync(ApiSendFormat apiRequest)
        {
            apiRequest.Body = apiRequest.Body.Replace("\r\n", "\n");
            apiRequest.Body = HumpUnfold(apiRequest.Body);

            switch (CommentTranslator22Package.ConfigA.Servers)
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
            if (CommentTranslator22Package.ConfigA.LanguageFrom == CommentTranslator22Package.ConfigA.LanguageTo ||
                CommentTranslator22Package.ConfigA.LanguageTo == LanguageEnum.Auto)
            {
                return true;
            }

            if (text.Length < MinTranslateLength ||
                text.Length > MaxTranslateLength)
            {
                return true;
            }

            switch (CommentTranslator22Package.ConfigA.LanguageTo)
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
