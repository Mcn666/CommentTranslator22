using CommentTranslator22.Translate;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CommentTranslator22.Popups.CompletionToolTip
{
    /// <summary>
    /// 测试用补全工具提示控件
    /// </summary>
    internal class TestCompletionToolTip : TextBlock
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="completion">补全项</param>
        public TestCompletionToolTip(Completion completion)
        {
            // 如果补全项没有描述信息，则进行翻译处理
            if (string.IsNullOrEmpty(completion.Description))
            {
                var resultText = string.Empty;
                var unfolded = TranslationClient.Instance.HumpUnfold(completion.DisplayText).Trim();
                var split = unfolded.Split(' ');

                // 处理单个单词
                if (split.Length == 1)
                {
                    var dictionaryResult = Dictionary.Dictionary.Instance.IndexOf(unfolded);
                    if (dictionaryResult != null)
                    {
                        resultText = $"{unfolded}  {dictionaryResult.zh}";
                    }
                    // 未找到翻译时，resultText保持为空
                }
                // 处理多个单词（短语）
                else
                {
                    var phraseResult = PhraseTranslationData.Instance.GetTranslationEntry(unfolded);
                    if (phraseResult != null)
                    {
                        resultText = $"{unfolded}  {phraseResult.TargetText}";
                    }
                    else
                    {
                        // 异步翻译短语
                        TranslatePhrase(unfolded);

                        // 临时显示单词级翻译结果
                        foreach (var word in split)
                        {
                            var wordResult = Dictionary.Dictionary.Instance.IndexOf(word);
                            resultText += $"{word}  {(wordResult == null ? "??" : wordResult.zh)}\n";
                        }
                    }
                }

                this.Text = resultText.TrimEnd('\n');
                this.FontSize = 12;
                this.Foreground = Brushes.LightGray;
            }
        }

        /// <summary>
        /// 异步翻译短语并缓存结果
        /// </summary>
        /// <param name="phrase">需要翻译的短语</param>
        private void TranslatePhrase(string phrase)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var translationResult = await TranslationClient.Instance.TranslateAsync(phrase);
                PhraseTranslationData.Instance.AddTranslationEntry(phrase, translationResult.TargetText);
            });
        }
    }

    /// <summary>
    /// 测试用补全工具提示提供程序
    /// </summary>
    /// <remarks>
    /// 当前选择的补全项描述信息提示接口。
    /// 在最新的VS2022版本中，C#不再使用此接口，主要用于XML/XAML内容
    /// </remarks>
    [Export(typeof(IUIElementProvider<Completion, ICompletionSession>))]
    [Name(nameof(TestCompletionToolTipProvider))]
    [Order(Before = "RoslynToolTipProvider")] // 覆盖默认的Roslyn提示提供程序
    [ContentType("XML")]
    [ContentType("XAML")]
    internal class TestCompletionToolTipProvider : IUIElementProvider<Completion, ICompletionSession>
    {
        /// <summary>
        /// 获取UI元素
        /// </summary>
        /// <param name="itemToRender">要渲染的补全项</param>
        /// <param name="context">补全会话上下文</param>
        /// <param name="elementType">UI元素类型</param>
        /// <returns>UI元素或null</returns>
        public UIElement GetUIElement(Completion itemToRender, ICompletionSession context, UIElementType elementType)
        {
            if (elementType == UIElementType.Tooltip)
            {
                return new TestCompletionToolTip(itemToRender);
            }

            return null;
        }
    }

    //internal class TestCPPCompletionToolTip : TextBlock
    //{

    //    public TestCPPCompletionToolTip(Completion completion)
    //    {
    //        var strins = completion.Description.Split('\n');
    //    }

    //    void Func(string str)
    //    {

    //    }

    //    [Export(typeof(IUIElementProvider<Completion, ICompletionSession>))]
    //    [Name(nameof(TestCompletionToolTipProvider))]
    //    [Order(Before = "RoslynToolTipProvider")] // Roslyn 是默认的提示提供程序，使用自定义提示，需要覆盖 Roslyn 提示提供程序
    //    [ContentType("C/C++")]
    //    internal class TestCompletionToolTipProvider : IUIElementProvider<Completion, ICompletionSession>
    //    {
    //        public UIElement GetUIElement(Completion itemToRender, ICompletionSession context, UIElementType elementType)
    //        {
    //            if (elementType == UIElementType.Tooltip)
    //            {
    //                return new TestCPPCompletionToolTip(itemToRender);
    //            }
    //            else
    //            {
    //                return null;
    //            }
    //        }
    //    }
    //}
}
