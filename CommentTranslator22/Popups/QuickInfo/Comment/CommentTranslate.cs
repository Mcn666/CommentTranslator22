using CommentTranslator22.Dictionary;
using CommentTranslator22.Popups.QuickInfo.Comment.Support;
using CommentTranslator22.Translate;
using CommentTranslator22.Translate.Format;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.RemoteSettings;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.QuickInfo.Comment
{
    internal static class CommentTranslate
    {
        /// <summary>
        /// 尝试翻译方法或变量的注释文本
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ClassifiedTextRun>> TryTranslateMethodInformationAsync(IAsyncQuickInfoSession session, SnapshotPoint snapshot)
        {
            var str = TryGetMethodInformation(session, snapshot.Snapshot.TextBuffer.ContentType.ToString());
            if (str != null)
            {
                var s = TranslateClient.Instance.HumpUnfold(str);
                var r = MethodAnnotationData.Instance.IndexOf(s);
                if (r == null)
                {
                    var recv = await TranslateClient.Instance.TranslateAsync(str);

                    // 在这里将翻译后的方法注释保存到 ?? 方法中
                    MethodAnnotationData.Instance.Add(recv);

                    var temp = new List<ApiRecvFormat> { recv };
                    CreateClassifiedTextRun(temp, out var runs);
                    return runs;
                }
                else
                {
                    var recv = new ApiRecvFormat()
                    {
                        Message = "buf",
                        TargetText = r,
                    };

                    var temp = new List<ApiRecvFormat> { recv };
                    CreateClassifiedTextRun(temp, out var runs);
                    return runs;
                }
            }
            return null;
        }

        /// <summary>
        /// 尝试从 IAsyncQuickInfoSession 中获取鼠标指向的方法或变量的注解
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        public static string TryGetMethodInformation(IAsyncQuickInfoSession session, string content)
        {
            if (session.Properties.PropertyList.Count > 0)
            {
                // 这个 Value 的类型应该为 Microsoft.VisualStudio.Language.Intellisense.Implementation.LegacyQuickInfoSession
                // 但是我还没有找到这个类型所在的命名空间，现在使用的是类型强转
                var quick = session.Properties.PropertyList[0].Value as IQuickInfoSession;
                foreach (ContainerElement i in quick.QuickInfoContent.Cast<ContainerElement>())
                {
                    if (content == "C/C++")
                    {
                        return TryGetCppMethodInformation(i.Elements);
                    }
                    foreach (ContainerElement element in i.Elements.Cast<ContainerElement>())
                    {
                        if (element.Elements.Count() > 1)
                        {
                            ClassifiedTextElement textElement = element.Elements.ElementAt(1) as ClassifiedTextElement;
                            if (textElement.Runs.Count() == 1)
                            {
                                ClassifiedTextRun run = textElement.Runs.ElementAt(0);
                                return run.Text;
                            }
                        }
                        break;
                    }
                    break;
                }
            }
            return null;
        }

        static string TryGetCppMethodInformation(IEnumerable<object> elements)
        {
            if (elements.Count() > 3
                    && elements.ElementAt(2).GetType() == typeof(ContainerElement))
            {
                var element = elements.ElementAt(2) as ContainerElement;
                if (element.Elements.Count() == 1
                    && element.Elements.ElementAt(0).GetType() == typeof(ClassifiedTextElement))
                {
                    var textElement = element.Elements.ElementAt(0) as ClassifiedTextElement;
                    if (textElement.Runs.Count() == 1)
                    {
                        var run = textElement.Runs.ElementAt(0);
                        return run.Text;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 翻译鼠标所在位置的附近的注释文本
        /// </summary>
        /// <param name="snapshot"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ClassifiedTextRun>> TranslateAsync(SnapshotPoint snapshot)
        {
            List<ClassifiedTextRun> classifieds = new List<ClassifiedTextRun>();
            var strList = SearchComment(snapshot);
            if (strList != null)
            {
                var f = new List<ApiRecvFormat>();
                foreach (var i in strList)
                {
                    var s = TranslateClient.Instance.HumpUnfold(i);
                    var r = GeneralAnnotationData.Instance.IndexOf(s);
                    if (r == null)
                    {
                        var recv = await TranslateClient.Instance.TranslateAsync(s);
                        f.Add(recv);
                        GeneralAnnotationData.Instance.Add(recv);
                    }
                    else
                    {
                        var recv = new ApiRecvFormat()
                        {
                            Message = "buf",
                            TargetText = r,
                        };
                        f.Add(recv);
                    }
                }
                CreateClassifiedTextRun(f, out var runs);
                classifieds.AddRange(runs);
            }
            return await Task.FromResult(classifieds);
        }

        /// <summary>
        /// TranslateAsync 方法需要使用的部分
        /// </summary>
        /// <param name="snapshot"></param>
        /// <returns></returns>
        static IEnumerable<string> SearchComment(SnapshotPoint snapshot)
        {
            var content = snapshot.Snapshot.TextBuffer.ContentType.ToString();
            switch (content)
            {
                case "C/C++":
                case "CSharp":
                    return CSharp.SearechComment(snapshot);
            }
            return null;
        }

        static void CreateClassifiedTextRun(List<ApiRecvFormat> formats, out List<ClassifiedTextRun> runs)
        {
            runs = new List<ClassifiedTextRun>();

            for (int i = 0; i < formats.Count; i++)
            {
                var temp = (i == formats.Count - 1) ? "" : "\n";
                if (formats[i].IsSuccess)
                {
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Comment, $"{formats[i].TargetText}{temp}"));
                }
                else if (string.IsNullOrEmpty(formats[i].TargetText))
                {
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.String, $"[{formats[i].Message}]"));
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Comment, $"{formats[i].SourceText}{temp}"));
                }
                else
                {
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Keyword, $"[{formats[i].Message}]"));
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Comment, $"{formats[i].TargetText}{temp}"));
                }
            }
        }

        /// <summary>
        /// 使用字典查询传入的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static ClassifiedTextRun QueryDictionary(string str)
        {
            // 如果找不到可以翻译的文本，就检查一下是不是使用了字典，以及字典支持的翻译目标语言，现在还无法确定源文本的语言
            if (CommentTranslator22Package.Config.TargetLanguage == LanguageEnum.简体中文
                && Regex.IsMatch(str, "[\u4e00-\u9fff]") == false)
            {
                var words = GetWordAnrray(str);
                if (words != null)
                {
                    var result = "";
                    foreach (var word in words)
                    {
                        str = word.ToString();
                        var temp = Dictionary.Dictionary.Instance.Query(str.ToLower());
                        if (temp != null)
                        {
                            result += $"{str}  {temp.zh}\n";

                            if (CommentTranslator22Package.Config.UseCharacterStatistics)
                            {
                                // 将结果保留到临时字符集中
                                DictionaryUseData.Instance.Add(temp, DictionaryUseData.StorageEnum.Default);
                            }
                            continue;
                        }
                        else
                        {
                            result += $"{str}\n";
                        }

                        if (CommentTranslator22Package.Config.UseCharacterStatistics && str.Length > 2)
                        {
                            // 将这个字符串保留到另一个字符集中
                            DictionaryUseData.Instance.Add(new DictionaryFormat
                            {
                                en = str,
                            }, DictionaryUseData.StorageEnum.Unfound);
                        }
                    }
                    return new ClassifiedTextRun(
                    PredefinedClassificationTypeNames.Comment, $"{result.TrimEnd('\n')}");
                }
            }
            return null;
        }

        /// <summary>
        /// 拆分字符串，获取词组集合
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static IEnumerable<object> GetWordAnrray(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 2 || text.Length > 50)
            {
                return null;
            }

            // 先按下划线分割这个字符串
            var strList = new List<object>();
            var strings = text.Split('_');
            foreach (var s in strings)
            {
                // 将所有非字母字符替换为空
                var temp = Regex.Replace(s, "[^A-Za-z]", "");
                if (temp.Length < 2)
                {
                    continue;
                }

                // 检查是否全部为大写字母
                if (Regex.IsMatch(temp, "^[A-Z]+$"))
                {
                    strList.Add(temp);
                    continue;
                }

                // 匹配以大写字母开始后跟随一个或多个小写字母的单词,
                // 如果字符串以小写字母开始，则这些小写字母序列也算匹配（因为^可以匹配到字符串的开始，意味着紧跟其后的[a-z]+可以开始匹配）
                var matches = Regex.Matches(temp, "([A-Z]|^)[a-z]+");
                foreach (var macth in matches)
                {
                    strList.Add(macth);
                }
            }
            return strList;
        }

    }
}
