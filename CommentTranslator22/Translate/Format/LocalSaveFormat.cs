namespace CommentTranslator22.Translate.Format
{
    public class LocalSaveFormat
    {
        /// <summary>
        /// 访问次数
        /// </summary>
        public int Visits { get; set; }
        /// <summary>
        /// 翻译服务器
        /// </summary>
        public ServerEnum TranslationServer { get; set; }
        /// <summary>
        /// 翻译前的语言
        /// </summary>
        public LanguageEnum SourceLanguage { get; set; }
        /// <summary>
        /// 翻译后的语言
        /// </summary>
        public LanguageEnum TargetLanguage { get; set; }
        /// <summary>
        /// 翻译前的字段
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 翻译后的字段
        /// </summary>
        public string Result { get; set; }
    }
}
