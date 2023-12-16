using CommentTranslator22.Translate.Enum;

namespace CommentTranslator22.Translate.Format
{
    public class ApiSendFormat
    {
        /// <summary>
        /// 请求语言
        /// </summary>
        public LanguageEnum FromLanguage { get; set; }
        /// <summary>
        /// 目标语言
        /// </summary>
        public LanguageEnum ToLanguage { get; set; }
        /// <summary>
        /// 翻译内容
        /// </summary>
        public string Body { get; set; }
    }
}
