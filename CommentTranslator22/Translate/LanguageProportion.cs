using System.Text.RegularExpressions;

namespace CommentTranslator22.Translate
{
    internal class LanguageProportion
    {
        public static float Chinese(string str)
        {
            float res = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (Regex.IsMatch(str[i].ToString(), "[\u4e00-\u9fff]")
                    || char.IsPunctuation(str[i])
                    || char.IsDigit(str[i]))
                {
                    res++;
                }
            }

            return res / str.Length;
        }

        public static float English(string str)
        {
            float res = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (Regex.IsMatch(str[i].ToString(), "[a-zA-Z]")
                    || char.IsPunctuation(str[i])
                    || char.IsDigit(str[i]))
                {
                    res++;
                }
            }

            return res / str.Length;
        }

        public static float Japanese(string str)
        {
            float res = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (Regex.IsMatch(str[i].ToString(), "[\u3040-\u30ff\u31f0-\u31ff]")
                    || char.IsPunctuation(str[i])
                    || char.IsDigit(str[i]))
                {
                    res++;
                }
            }

            return res / str.Length;
        }
    }
}
