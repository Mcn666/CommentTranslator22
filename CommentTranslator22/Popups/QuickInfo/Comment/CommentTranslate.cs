using CommentTranslator22.Dictionary;
using CommentTranslator22.Popups.QuickInfo.Comment.Support;
using CommentTranslator22.Translate;
using CommentTranslator22.Translate.Format;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.QuickInfo.Comment
{
    internal static class CommentTranslate
    {
        #region 公共方法

        /// <summary>
        /// 获取方法的XML文档注释并翻译
        /// </summary>
        public static async Task<IEnumerable<ClassifiedTextRun>> GetMethodExplanationAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            try
            {
                var symbol = await GetSymbolFromSessionAsync(session, cancellationToken);
                if (symbol != null)
                {
                    var doc = symbol.GetDocumentationCommentXml();
                    if (doc != null)
                    {
                        var memberDoc = MemberDoc.FromXml(doc);
                        return await TranslateMemberDocAsync(memberDoc);
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录日志或处理异常
                System.Diagnostics.Debug.WriteLine($"GetMethodExplanationAsync error: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// 尝试翻译方法或变量的注释文本
        /// </summary>
        public static async Task<IEnumerable<ClassifiedTextRun>> TryTranslateMethodInformationAsync(IAsyncQuickInfoSession session, string typeName)
        {
            var text = TryGetMethodInformation(session, typeName);
            if (text != null)
            {
                CommentHelp.StringPretreatment(ref text);
                var translationResult = await GetTranslationWithCacheAsync(MethodTranslationData.Instance, text);

                if (translationResult != null)
                {
                    var classifieds = new List<ClassifiedTextRun>();
                    CreateClassifiedTextRun(new[] { translationResult }, ref classifieds);
                    return classifieds;
                }
            }

            return null;
        }

        /// <summary>
        /// 翻译鼠标所在位置的附近的注释文本
        /// </summary>
        public static async Task<IEnumerable<ClassifiedTextRun>> TranslateAsync(SnapshotPoint snapshot)
        {
            var strList = CommentHelp.FindComment(snapshot);
            if (strList == null || !strList.Any())
            {
                return new List<ClassifiedTextRun>();
            }

            var classifieds = new List<ClassifiedTextRun>();

            // .NET Framework 4.7.2 不支持异步LINQ，使用传统循环
            var results = new List<ApiRecvFormat>();
            foreach (var str in strList)
            {
                var result = await GetTranslationWithCacheAsync(GeneralTranslationData.Instance, str);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            CreateClassifiedTextRun(results, ref classifieds);

            return classifieds;
        }

        /// <summary>
        /// 从字典获取单词翻译
        /// </summary>
        public static Task<IEnumerable<ClassifiedTextRun>> FetchDictionaryEntriesAsync(string str)
        {
            if (!IsValidWordString(str))
            {
                return Task.FromResult<IEnumerable<ClassifiedTextRun>>(null);
            }

            var words = SplitWords(str);
            if (words == null || !words.Any())
            {
                return Task.FromResult<IEnumerable<ClassifiedTextRun>>(null);
            }

            var runs = new List<ClassifiedTextRun>
            {
                new ClassifiedTextRun(PredefinedClassificationTypeNames.Keyword, "[SimpleDictionary]")
            };

            foreach (var word in words)
            {
                var dictionaryEntry = Dictionary.Dictionary.Instance.IndexOf(word);
                runs.Add(new ClassifiedTextRun(PredefinedClassificationTypeNames.ExcludedCode, word));
                runs.Add(new ClassifiedTextRun(
                    PredefinedClassificationTypeNames.Comment,
                    $"[{GetDictionaryTranslation(dictionaryEntry)}]"));
            }

            return Task.FromResult<IEnumerable<ClassifiedTextRun>>(runs);
        }

        /// <summary>
        /// 翻译词组
        /// </summary>
        public static async Task<IEnumerable<ClassifiedTextRun>> TranslateWordsAsync(string str)
        {
            if (!IsValidWordString(str))
            {
                return null;
            }

            var words = SplitWords(str);
            if (words == null || words.Count() < 2)
            {
                return null;
            }

            var phrase = string.Join(" ", words);
            var translationResult = await GetTranslationWithCacheAsync(PhraseTranslationData.Instance, phrase);

            if (translationResult != null && string.IsNullOrEmpty(translationResult.TargetText) == false)
            {
                var runs = new List<ClassifiedTextRun>
                {
                    new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Keyword,
                        translationResult.IsSuccess ? "[Internet]" : "[Buffer]"),
                    new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.ExcludedCode,
                        phrase + " "),
                    new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Comment,
                        $"[{translationResult.TargetText}]")
                };
                return runs;
            }

            return null;
        }

        #endregion

        #region 私有辅助方法

        /// <summary>
        /// 从会话中获取符号
        /// </summary>
        private static async Task<ISymbol> GetSymbolFromSessionAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var buffer = session.TextView.TextBuffer;
            if (buffer == null) return null;

            var snapshotPoint = session.GetTriggerPoint(buffer).GetPoint(buffer.CurrentSnapshot);
            var semanticContext = new SemanticContext();
            await semanticContext.UpdateAsync(buffer, cancellationToken);

            var semanticModel = semanticContext.SemanticModel;
            if (semanticModel == null) return null;

            var compilationUnitSyntax = semanticModel.SyntaxTree.GetCompilationUnitRoot(cancellationToken);
            var token = compilationUnitSyntax.FindToken(snapshotPoint, true);
            var node = compilationUnitSyntax.FindNode(token.Span, true, true);

            return semanticModel.GetSymbolInfo(node).Symbol ?? semanticModel.GetDeclaredSymbol(node);
        }

        /// <summary>
        /// 翻译成员文档
        /// </summary>
        private static async Task<IEnumerable<ClassifiedTextRun>> TranslateMemberDocAsync(MemberDoc memberDoc)
        {
            if (memberDoc == null) return null;

            var keys = new List<string> { memberDoc.Summary, memberDoc.Returns };
            var results = new List<ApiRecvFormat>();

            // .NET Framework 4.7.2 不支持异步LINQ，使用传统循环
            foreach (var key in keys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    var result = await GetTranslationWithCacheAsync(MethodTranslationData.Instance, key);
                    if (result != null)
                    {
                        results.Add(result);
                    }
                }
            }

            var classifieds = new List<ClassifiedTextRun>();
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i] == null) continue;

                var memberName = i == 0 ? "Summary" : "Returns";
                CreateClassifiedTextRun(new[] { results[i] }, ref classifieds, memberName);

                if (i == 0 && results.Count > 1 && results[1] != null)
                {
                    classifieds.Add(new ClassifiedTextRun("", "\n"));
                }
            }

            return classifieds.Any() ? classifieds : null;
        }

        /// <summary>
        /// 获取带缓存的翻译结果
        /// </summary>
        private static async Task<ApiRecvFormat> GetTranslationWithCacheAsync(BaseTranslationData translationData, string key)
        {
            if (string.IsNullOrEmpty(key)) return null;

            // 首先尝试从缓存获取
            var cachedResult = translationData.GetTranslationResult(key);
            if (cachedResult != null) return cachedResult;

            // 缓存未命中，调用API翻译
            var apiResult = await TranslationClient.Instance.TranslateAsync(key);
            if (apiResult.IsSuccess)
            {
                translationData.AddTranslationEntry(key, apiResult.TargetText);
            }

            return apiResult;
        }

        /// <summary>
        /// 创建分类文本运行
        /// </summary>
        private static void CreateClassifiedTextRun(IEnumerable<ApiRecvFormat> formats, ref List<ClassifiedTextRun> runs, string memberDoc = null)
        {
            foreach (var format in formats)
            {
                if (format == null) continue;

                var isLast = format == formats.LastOrDefault();
                var separator = isLast ? "" : "\n";

                if (format.IsSuccess)
                {
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Keyword,
                        $"[{memberDoc ?? "Internet"}]"));
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Comment,
                        $"{format.TargetText}{separator}"));
                }
                else if (string.IsNullOrEmpty(format.TargetText))
                {
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.String,
                        $"[{format.Message}]"));
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Comment,
                        $"{format.SourceText}{separator}"));
                }
                else
                {
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Keyword,
                        $"[{memberDoc ?? "Buffer"}]"));
                    runs.Add(new ClassifiedTextRun(
                        PredefinedClassificationTypeNames.Comment,
                        $"{format.TargetText}{separator}"));
                }
            }
        }

        /// <summary>
        /// 尝试从 IAsyncQuickInfoSession 中获取鼠标指向的方法或变量的注解
        /// </summary>
        private static string TryGetMethodInformation(IAsyncQuickInfoSession session, string typeName)
        {
            if (session.Properties.PropertyList.Count == 0)
            {
                return null;
            }

            var quickInfoSession = session.Properties.PropertyList[0].Value as IQuickInfoSession;
            if (quickInfoSession == null)
            {
                return null;
            }

            foreach (ContainerElement container in quickInfoSession.QuickInfoContent.Cast<ContainerElement>())
            {
                if (typeName == "C/C++")
                {
                    return ExtractCppMethodInformation(container.Elements);
                }

                return ExtractCsMethodInformation(container.Elements);
            }

            return null;
        }

        /// <summary>
        /// 提取C#方法信息
        /// </summary>
        private static string ExtractCsMethodInformation(IEnumerable<object> elements)
        {
            foreach (ContainerElement element in elements.Cast<ContainerElement>())
            {
                if (element.Elements.Count() < 2) continue;

                var firstElement = element.Elements.ElementAt(0);
                var secondElement = element.Elements.ElementAt(1);

                if (firstElement is ContainerElement && secondElement is ClassifiedTextElement textElement)
                {
                    if (textElement.Runs.Count() == 1)
                    {
                        return textElement.Runs.ElementAt(0).Text;
                    }
                    else if (textElement.Runs.Count() > 1)
                    {
                        var sb = new StringBuilder();
                        foreach (var run in textElement.Runs)
                        {
                            sb.Append(run.Text);
                        }
                        return sb.ToString();
                    }
                }
                break;
            }

            return null;
        }

        /// <summary>
        /// 提取C++方法信息
        /// </summary>
        private static string ExtractCppMethodInformation(IEnumerable<object> elements)
        {
            if (elements.Count() <= 3)
            {
                return null;
            }

            var container = elements.ElementAt(2) as ContainerElement;
            if (container == null)
            {
                return null;
            }

            if (container.Elements.Count() != 1)
            {
                return null;
            }

            var textElement = container.Elements.ElementAt(0) as ClassifiedTextElement;
            if (textElement == null)
            {
                return null;
            }

            if (textElement.Runs.Count() == 1)
            {
                return textElement.Runs.ElementAt(0).Text;
            }
            else if (textElement.Runs.Count() > 1)
            {
                var firstRunText = textElement.Runs.ElementAt(0).Text;
                if (firstRunText.Contains("扩展到:") || firstRunText.Contains("大小:"))
                {
                    return null;
                }

                var sb = new StringBuilder();
                foreach (var run in textElement.Runs)
                {
                    var processedText = run.Text
                        .Replace("\result\n", "\n")
                        .Replace("\result", "\n")
                        .Replace("\n", "");
                    CommentHelp.StringPretreatment(ref processedText);
                    sb.Append(processedText);
                }
                return sb.ToString();
            }

            return null;
        }

        /// <summary>
        /// 获取字典翻译
        /// </summary>
        private static string GetDictionaryTranslation(DictionaryFormat format)
        {
            if (format == null) return "??";

            switch (CommentTranslator22Package.Config.TargetLanguage)
            {
                case LanguageEnum.English:
                    return format.en;
                case LanguageEnum.简体中文:
                    return format.zh;
                case LanguageEnum.繁體中文:
                    return format.cht;
                case LanguageEnum.日本語:
                    return format.ja;
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 检查是否为有效的单词字符串
        /// </summary>
        private static bool IsValidWordString(string text)
        {
            return !string.IsNullOrEmpty(text) &&
                   text.Length >= 2 &&
                   text.Length <= 50 &&
                   Regex.IsMatch(text, @"^[A-Za-z_]+$");
        }

        /// <summary>
        /// 拆分字符串为单词
        /// </summary>
        private static IEnumerable<string> SplitWords(string text)
        {
            if (!IsValidWordString(text)) return null;

            var words = new List<string>();
            var parts = text.Split('_');

            foreach (var part in parts)
            {
                var cleanPart = Regex.Replace(part, "[^A-Za-z]", "");
                if (cleanPart.Length < 2) continue;

                // 全大写单词
                if (Regex.IsMatch(cleanPart, "^[A-Z]+$"))
                {
                    words.Add(cleanPart);
                    continue;
                }

                // 驼峰命名拆分
                var matches = Regex.Matches(cleanPart, "([A-Z]|^)[a-z]+");
                foreach (Match match in matches)
                {
                    words.Add(match.Value);
                }
            }

            return words.Any() ? words : null;
        }

        #endregion
    }
}