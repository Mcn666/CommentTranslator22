using CommentTranslator22.Comment.Support;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Comment
{
    internal class CommentTranslate
    {
        public static async Task<string> TranslateAsync(SnapshotSpan snapshot)
        {
            var str = SearchComment(snapshot.Start);
            if (str == null)
            {
                // 获取鼠标指向的文本
                str = snapshot.GetText().Trim();
                return QueryDictionary(str);
            }
            else
            {
                if (str.Length > CommentTranslator22Package.TranslateClient.MaxTranslateLength)
                {
                    return str;
                }

                if (CommentTranslator22Package.ConfigA.MergeCommentBlock == false)
                {
                    var res = string.Empty;
                    var splitResult = str.Split('\n');
                    foreach (var item in splitResult)
                    {
                        var recv = await CommentTranslator22Package.TranslateClient.TranslateAsync(item);
                        if (recv.Success)
                        {
                            LocalTranslateData.Add(recv);
                            res += recv.ResultText + "\n";
                        }
                        else
                        {
                            res += item + "\n";
                        }
                    }

                    return res.TrimEnd('\n');
                }
                else
                {
                    var res = await CommentTranslator22Package.TranslateClient.TranslateAsync(str);
                    if (res.Success)
                    {
                        LocalTranslateData.Add(res);
                        return res.ResultText;
                    }
                }
            }
            return await Task.FromResult(str);
        }

        /// <summary>
        /// 查询字典
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string QueryDictionary(string str)
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

        private static string SearchComment(SnapshotPoint snapshot)
        {
            var contentType = snapshot.Snapshot.TextBuffer.ContentType.TypeName.ToLower();
            switch (contentType)
            {
                case "c/c++":
                    return Cpp.SearechComment(snapshot);
                case "csharp":
                    return Csharp.SearechComment(snapshot);
            }
            return null;
        }
    }
}
