using CommentTranslator22.Dictionary;
using CommentTranslator22.Popups.CursorPosition.Comment.Support;
using CommentTranslator22.Translate;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.CursorPosition.Comment
{
    internal class CommentTranslate
    {
        public static async Task<IEnumerable<ClassifiedTextRun>> TranslateAsync(SnapshotSpan snapshot)
        {
            List<ClassifiedTextRun> classifieds = new List<ClassifiedTextRun>();
            var strList = SearchComment(snapshot.Start);
            if (strList != null)
            {
                var strLen = 0;
                foreach (var item in strList)
                {
                    strLen += item.Length;
                }
                if (strLen > CommentTranslator22Package.TranslateClient.MaxTranslateLength)
                {
                    return classifieds;
                }

                var index = 1;
                var count = strList.Count();
                foreach (var item in strList)
                {
                    var lineBreak = index++ == count ? "" : "\n";
                    var recv = await CommentTranslator22Package.TranslateClient.TranslateAsync(item);
                    if (recv.Success)
                    {
                        recv.SourceText = item;
                        LocalTranslateData.Add(recv);
                        var temp = new ClassifiedTextRun(
                            PredefinedClassificationTypeNames.Comment, recv.ResultText + lineBreak);
                        classifieds.Add(temp);
                    }
                    else if (string.IsNullOrEmpty(recv.ResultText) == false)
                    {
                        classifieds.Add(new ClassifiedTextRun(
                            PredefinedClassificationTypeNames.Keyword, $"[{recv.Message}]"));
                        classifieds.Add(new ClassifiedTextRun(
                            PredefinedClassificationTypeNames.Comment, recv.ResultText + lineBreak));
                    }
                    else
                    {
                        classifieds.Add(new ClassifiedTextRun(
                            PredefinedClassificationTypeNames.String, $"[{recv.Message}]"));
                        classifieds.Add(new ClassifiedTextRun(
                            PredefinedClassificationTypeNames.Comment, item + lineBreak));
                    }
                }
            }
            return await Task.FromResult(classifieds);
        }

        public static async Task<ClassifiedTextRun> TranslateSignatureAsync(string str)
        {
            var recv = await CommentTranslator22Package.TranslateClient.TranslateAsync(str);
            if (recv.Success)
            {
                return new ClassifiedTextRun(PredefinedClassificationTypeNames.Comment, recv.ResultText + "\n");
            }

            return null;
        }

        /// <summary>
        /// 查询字典
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string QueryDictionary(string str)
        {
            // 如果找不到可以翻译的文本，就检查一下是不是使用了字典，以及字典支持的翻译目标语言，现在还无法确定源文本的语言
            if (CommentTranslator22Package.Config.UseDictionary &&
                CommentTranslator22Package.Config.TargetLanguage == LanguageEnum.简体中文)
            {
                if (Regex.IsMatch(str, "[\u4e00-\u9fff]") == false)
                {
                    var result = string.Empty;
                    var words = GetWordArray(str);
                    if (words != null)
                    {
                        foreach (var word in words)
                        {
                            if (string.IsNullOrEmpty(word))
                            {
                                continue;
                            }

                            var temp = Dictionary.Dictionary.Query(word);
                            if (temp != null)
                            {
                                result += $"{word}   {temp.zh}\n";

                                if (CommentTranslator22Package.Config.UseCharacterStatistics)
                                {
                                    // 将结果保留到临时字符集中
                                    LocalDictionary.AddTempLocalDictionarie(temp);
                                }
                                continue;
                            }
                            else
                            {
                                result += $"{word}\n";
                            }

                            if (CommentTranslator22Package.Config.UseCharacterStatistics && word.Length > 2)
                            {
                                // 将这个字符串保留到另一个字符集中
                                LocalDictionary.AddNoResultCharacter(new Dictionary.DictionaryResultFormat
                                {
                                    en = word,
                                });
                            }
                        }

                        return result.TrimEnd('\n');
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取词组列表
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetWordArray(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 2 || str.Length > 50)
            {
                return null;
            }

            // 删除所有十进制数字和非字母字符
            str = Regex.Replace(str, @"\d+", "");
            str = Regex.Replace(str, "[^A-Za-z]", "");

            // 找到所有的大写字母，然后按大写字母分割字符串
            var chars = Regex.Matches(str, "[A-Z]");
            var strList = Regex.Split(str, "[A-Z]").ToList();
            strList.RemoveAt(0);

            // 将大写字母和分割后的字符串拼接
            for (int i = 0; i < strList.Count; i++)
            {
                if (strList[i] == string.Empty)
                {
                    continue;
                }
                strList[i] = chars[i].Value.ToLower() + strList[i];
            }

            return strList;
        }

        private static IEnumerable<string> SearchComment(SnapshotPoint snapshot)
        {
            var contentType = snapshot.Snapshot.TextBuffer.ContentType.ToString();
            switch (contentType)
            {
                case "C/C++":
                case "CSharp":
                    return CSharp.SearechComment(snapshot);
            }
            return null;
        }
    }
}
