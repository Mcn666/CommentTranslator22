﻿using CommentTranslator22.Dictionary;
using CommentTranslator22.Popups.QuickInfo.Comment.Support;
using CommentTranslator22.Translate;
using CommentTranslator22.Translate.Format;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
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
        public static async Task<IEnumerable<ClassifiedTextRun>> TryTranslateMethodInformationAsync(IAsyncQuickInfoSession session)
        {
            var str = TryGetMethodInformation(session);
            if (str != null)
            {
                var s = TranslateClient.Instance.HumpUnfold(str);
                var r = MethodAnnotationData.Instance.IndexOf(s);
                if (r == null)
                {
                    var recv = await TranslateClient.Instance.TranslateAsync(str);

                    // 在这里将翻译后的方法注释保存到 ?? 方法中
                    MethodAnnotationData.Instance.Add(recv);

                    CreateClassifiedTextRun(recv, out var runs);
                    return runs;
                }
                else
                {
                    var recv = new ApiRecvFormat()
                    {
                        Message = "buf",
                        TargetText = r,
                    };
                    CreateClassifiedTextRun(recv, out var runs);
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
        public static string TryGetMethodInformation(IAsyncQuickInfoSession session)
        {
            if (session.Properties.PropertyList.Count > 0)
            {
                var quick = session.Properties.PropertyList[0].Value as IQuickInfoSession;
                foreach (ContainerElement item in quick.QuickInfoContent.Cast<ContainerElement>())
                {
                    foreach (ContainerElement element in item.Elements.Cast<ContainerElement>())
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

        /// <summary>
        /// 翻译鼠标所在位置的附近的注释文本
        /// </summary>
        /// <param name="snapshot"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<ClassifiedTextRun>> TranslateAsync(SnapshotSpan snapshot)
        {
            List<ClassifiedTextRun> classifieds = new List<ClassifiedTextRun>();
            var strList = SearchComment(snapshot.Start);
            if (strList != null)
            {
                foreach (var item in strList)
                {
                    var s = TranslateClient.Instance.HumpUnfold(item);
                    var r = GeneralAnnotationData.Instance.IndexOf(s);
                    if (r == null)
                    {
                        var recv = await TranslateClient.Instance.TranslateAsync(s);
                        GeneralAnnotationData.Instance.Add(recv);

                        CreateClassifiedTextRun(recv, out var runs);
                        classifieds.AddRange(runs);
                    }
                    else
                    {
                        var recv = new ApiRecvFormat()
                        {
                            Message = "buf",
                            TargetText = r,
                        };
                        CreateClassifiedTextRun(recv, out var runs);
                        classifieds.AddRange(runs);
                    }
                }
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
            var contentType = snapshot.Snapshot.TextBuffer.ContentType.ToString();
            switch (contentType)
            {
                case "C/C++":
                case "CSharp":
                    return CSharp.SearechComment(snapshot);
            }
            return null;
        }

        static void CreateClassifiedTextRun(ApiRecvFormat recv, out List<ClassifiedTextRun> runs)
        {
            runs = new List<ClassifiedTextRun>();

            if (recv.IsSuccess)
            {
                runs.Add(new ClassifiedTextRun(
                    PredefinedClassificationTypeNames.Comment, $"{recv.TargetText}\n"));
            }
            else if (string.IsNullOrEmpty(recv.TargetText))
            {
                runs.Add(new ClassifiedTextRun(
                    PredefinedClassificationTypeNames.String, $"[{recv.Message}]"));
                runs.Add(new ClassifiedTextRun(
                    PredefinedClassificationTypeNames.Comment, $"{recv.SourceText}\n"));
            }
            else
            {
                runs.Add(new ClassifiedTextRun(
                    PredefinedClassificationTypeNames.Keyword, $"[{recv.Message}]"));
                runs.Add(new ClassifiedTextRun(
                    PredefinedClassificationTypeNames.Comment, $"{recv.TargetText}\n"));
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
                var words = GetWordCollection(str);
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
                                LocalDictionaryData.Instance.Add(temp, LocalDictionaryData.StorageEnum.Default);
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
                            LocalDictionaryData.Instance.Add(new DictionaryFormat
                            {
                                en = str,
                            }, LocalDictionaryData.StorageEnum.Unfound);
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
        /// <param name="str"></param>
        /// <returns></returns>
        public static MatchCollection GetWordCollection(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length < 2 || str.Length > 50)
            {
                return null;
            }

            // 将所有十进制数字替换为空
            //str = Regex.Replace(str, @"\d+", "");

            // 将所有非字母字符替换为空
            str = Regex.Replace(str, "[^A-Za-z]", "");

            // 匹配以大写字母开始后跟随一个或多个小写字母的单词,
            // 或者，如果字符串以小写字母开始，则这些小写字母序列也算匹配（因为^可以匹配到字符串的开始，意味着紧跟其后的[a-z]+可以开始匹配）
            var matchCollection = Regex.Matches(str, "([A-Z]|^)[a-z]+");
            return matchCollection;
        }

    }
}