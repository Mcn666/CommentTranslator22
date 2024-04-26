using System.Text.RegularExpressions;

namespace CommentTranslator22.Translate
{
    /// <summary>
    /// 检查语言文字在字符串中的占比
    /// </summary>
    internal class LanguageProportion
    {
        public static float Chinese(string str)
        {
            var temp = Regex.Replace(str, "[^\u4e00-\u9fff]", "");
            return (float)temp.Length / str.Length;
        }

        public static float English(string str)
        {
            var temp = Regex.Replace(str, "[^a-zA-Z]", "");
            return (float)temp.Length / str.Length;
        }

        public static float Japanese(string str)
        {
            var temp = Regex.Replace(str, "[^\u3040-\u30ff\u31f0-\u31ff]", "");
            return (float)temp.Length / str.Length;
        }
    }
}
