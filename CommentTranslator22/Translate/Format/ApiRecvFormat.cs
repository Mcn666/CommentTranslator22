﻿using System.Net;

namespace CommentTranslator22.Translate.Format
{
    public class ApiRecvFormat
    {
        /// <summary>
        /// 是否翻译成功
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 响应代码
        /// </summary>
        public HttpStatusCode Code { get; set; }

        public string Message { get; set; }
        /// <summary>
        /// 待翻译文件
        /// </summary>
        public string SourceText { get; set; }
        /// <summary>
        /// 翻译后的文本
        /// </summary>
        public string TargetText { get; set; }
    }
}
