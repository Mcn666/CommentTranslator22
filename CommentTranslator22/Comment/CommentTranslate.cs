using CommentTranslator22.Comment.Support;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Comment
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

        /// <summary>
        /// 查询字典
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string QueryDictionary(string str)
        {
            // 如果找不到可以翻译的文本，就检查一下是不是使用了字典，以及字典支持的翻译目标语言，现在还无法确定源文本的语言
            if (CommentTranslator22Package.ConfigB.UseDictionary &&
                CommentTranslator22Package.ConfigA.TargetLanguage == Translate.Enum.LanguageEnum.简体中文)
            {
                if (Regex.IsMatch(str, "[\u4e00-\u9fff]") == false)
                {
                    var result = string.Empty;
                    var words = Dictionary.ParseString.GetWordArray(str);
                    if (words != null)
                    {
                        foreach (var word in words)
                        {
                            var temp = Dictionary.Dictionary.Query(word);
                            if (temp != null)
                            {
                                result += $"{word}   {temp.zh}\n";
                            }
                        }
                        return result.TrimEnd('\n');
                    }
                }
            }
            return string.Empty;
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
