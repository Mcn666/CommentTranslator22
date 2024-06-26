﻿namespace CommentTranslator22.Translate.Format
{
    public class ApiSendFormat
    {
        /// <summary>
        /// 请求语言
        /// </summary>
        public LanguageEnum SourceLanguage { get; set; }
        /// <summary>
        /// 目标语言
        /// </summary>
        public LanguageEnum TargetLanguage { get; set; }
        /// <summary>
        /// 待翻译文本
        /// </summary>
        public string SourceText { get; set; }
    }
}
