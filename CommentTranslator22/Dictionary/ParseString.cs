using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CommentTranslator22.Dictionary
{
    internal class ParseString
    {
        const char SplitValue = '_';

        /// <summary>
        /// 拆分字符串
        /// 例: foor-bar 拆分为 [foo, bar]
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetWordArray(string character)
        {
            var RegexValue = Regex.Matches(character, @"[A-Z\s]{2,}").Cast<Match>();
            if (RegexValue.Count() > 0)
            {
                character = RegexValue.Select(I =>
                {
                    string tempValue = I.Value;
                    // 将大写的单词转换为小写的
                    return tempValue.Replace(tempValue, Pascalize(tempValue.ToLower()));
                }).Aggregate(string.Empty, (totall, current) =>
                {
                    /** 将字符串连接起来 */
                    return totall + current;
                });
            }

            // 判断是否是字母并且全部大写
            if (Regex.IsMatch(character, @"^[A-Z]+$"))
            {
                return new List<string>()
                {
                    character.ToLower(),
                };
            }
            var getWords = Decamelize(character).Split(SplitValue).ToList();
            return new List<string>(getWords).Distinct();
        }

        private static string Decamelize(string input, string separator = "_")
        {
            return SeparateWords(input, separator).ToLower();
        }

        /// <summary>
        /// 将一段小驼峰命名法的单词分隔，并使用下划线连接
        /// </summary>
        /// <param name="input"></param>
        /// <param name="separator"></param>
        /// <param name="split"></param>
        /// <returns></returns>
        private static string SeparateWords(string input, string separator = "_", Regex split = null)
        {
            split = split ?? new Regex("(?=[A-Z])");
            // 使用正则表达式分隔字符串
            string[] words = split.Split(input);
            //使用分隔符连接字符串
            return string.Join(separator, words);
        }

        /// <summary>
        /// 判断字符串是否数字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static bool IsNumerical(string input)
        {
            if (double.TryParse(input, out double result))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 转换为小驼峰命名法
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string Camelize(string input)
        {
            if (IsNumerical(input))
            {
                return input;
            }
            //将文本的特殊字符删除，例如 -、_
            input = Regex.Replace(input, @"[\-_\s]+(.)?", delegate (Match match)
            {
                var chr = match.Groups[1].Value;
                return chr != "" ? chr.ToUpper() : "";
            });
            // 将首字母转换为小写
            return char.ToLower(input[0]) + input.Substring(1);
        }

        /// <summary>
        /// 大驼峰命名法
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string Pascalize(string input)
        {
            if (input.Length < 1)
            {
                return input;
            }
            string camelized = Camelize(input);
            // 将首字母转换为大写
            return char.ToUpper(camelized[0]) + camelized.Substring(1);
        }

        /// <summary>
        /// 判断一段文本是否为数字
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static bool IsNumerical(object obj)
        {
            if (double.TryParse(obj.ToString(), out double result))
            {
                return true;
            }
            return false;
        }
    }
}
