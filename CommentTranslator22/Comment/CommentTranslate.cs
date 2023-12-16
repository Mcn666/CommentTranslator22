using CommentTranslator22.Comment.Support;
using CommentTranslator22.Translate;
using CommentTranslator22.Translate.Format;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Comment
{
    internal class CommentTranslate
    {
        public static async Task<string> TranslateAsync(SnapshotSpan snapshot)
        {
            var text = SearchComment(snapshot.Start);
            if (text == null)
            {
                // 获取鼠标指向的文本
                text = snapshot.GetText().Trim();

                // 如果找不到可以翻译的文本，就检查一下是不是使用了字典，以及字典支持的翻译目标语言，现在还无法确定源文本的语言
                if (CommentTranslator22Package.ConfigB.UseDictionary &&
                    CommentTranslator22Package.ConfigA.LanguageTo == Translate.Enum.LanguageEnum.简体中文)
                {
                    if (Regex.IsMatch(text, @"[\u4e00-\u9fff]"))
                    {
                        return "";
                    }
                    //Dictionary.Dictionary.Query(awaitTranslateText);
                }
            }
            else
            {
                if (text.Length > CommentTranslator22Package.TranslateClient.MaxTranslateLength)
                {
                    return text;
                }

                if (CommentTranslator22Package.ConfigA.MergeCommentBlock == false)
                {
                    var res = string.Empty;
                    var splitResult = text.Split('\n');
                    foreach (var item in splitResult)
                    {
                        var recv = await CommentTranslator22Package.TranslateClient.TranslateAsync(item);
                        if (recv.Success)
                        {
                            LocalTranslateData.Add(recv);
                            res += recv.Data + "\n";
                        }
                        else
                        {
                            res += item + "\n";
                        }
                    }

                    var index = res.LastIndexOf("\n");
                    if (index != -1)
                        res = res.Remove(index);
                    return res;
                }
                else
                {
                    var res = await CommentTranslator22Package.TranslateClient.TranslateAsync(text);
                    if (res.Success)
                    {
                        LocalTranslateData.Add(res);
                        return res.Data;
                    }
                }
            }
            return await Task.FromResult(text);
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
