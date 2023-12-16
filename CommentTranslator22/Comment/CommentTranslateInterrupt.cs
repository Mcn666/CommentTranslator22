﻿using System.Text.RegularExpressions;

namespace CommentTranslator22.Comment
{
    internal class CommentTranslateInterrupt
    {
        /// <summary>
        /// 检查这段注释文本是不是标签信息或者代码
        /// </summary>
        /// <param name="text"></param>
        /// <returns>如果为true表示不属于标签信息或代码，否则为false</returns>
        public static bool Check(string text)
        {
            if (CommentTranslator22Package.ConfigB.UseMask)
            {
                foreach (var str in CommentTranslator22Package.ConfigB.MaskType)
                {
                    if (Regex.IsMatch(text, WildCardToRegular(str)))
                        return true;
                }
            }
            return false;
        }

        /// If you want to implement both "*" and "?"
        private static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}