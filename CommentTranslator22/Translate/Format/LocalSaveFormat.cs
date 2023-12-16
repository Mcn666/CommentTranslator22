using CommentTranslator22.Translate.Enum;

namespace CommentTranslator22.Translate.Format
{
    public class LocalSaveFormat
    {
        /// <summary>
        /// 访问次数
        /// </summary>
        public int ReadCount { get; set; }
        /// <summary>
        /// 翻译服务器
        /// </summary>
        public ServerEnum Server { get; set; }
        /// <summary>
        /// 翻译前的语言
        /// </summary>
        public LanguageEnum FromLanguage { get; set; }
        /// <summary>
        /// 翻译后的语言
        /// </summary>
        public LanguageEnum ToLanguage { get; set; }
        /// <summary>
        /// 翻译前的字段
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 翻译后的字段
        /// </summary>
        public string Data { get; set; }
    }
}
