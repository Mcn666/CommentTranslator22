namespace CommentTranslator22.Translate.Format
{
    public class ApiRecvFormat
    {
        /// <summary>
        /// 是否翻译成功
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// 翻译结果代码
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 原文本
        /// </summary>
        public string SourceText { get; set; }
        /// <summary>
        /// 结果文本
        /// </summary>
        public string ResultText { get; set; }
    }
}
