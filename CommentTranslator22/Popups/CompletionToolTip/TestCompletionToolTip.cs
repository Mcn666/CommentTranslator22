using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CommentTranslator22.Popups.CompletionToolTip
{
    internal class TestCompletionToolTip : TextBlock
    {
        public TestCompletionToolTip(Completion completion)
        {
            if (string.IsNullOrEmpty(completion.Description))
            {
                var str = "";
                var strings = TranslationClient.Instance.HumpUnfold(completion.DisplayText).Split(' ');
                foreach (var s in strings)
                {
                    var r = Dictionary.Dictionary.Instance.IndexOf(s);
                    if (r != null)
                    {
                        str += $"{s}  {r.zh}\n";
                    }
                }

                this.Text = str.TrimEnd('\n');
                this.FontSize = 12;
                this.Foreground = Brushes.LightGray;
            }
        }

        /// <summary>
        /// 当前选择的补全项描述信息提示接口。在最新的VS2022版本中，C#不再使用此接口
        /// </summary>
        [Export(typeof(IUIElementProvider<Completion, ICompletionSession>))]
        [Name(nameof(TestCompletionToolTipProvider))]
        [Order(Before = "RoslynToolTipProvider")] // Roslyn 是默认的提示提供程序，使用自定义提示，需要覆盖 Roslyn 提示提供程序
        [ContentType("XML")]
        [ContentType("XAML")]
        internal class TestCompletionToolTipProvider : IUIElementProvider<Completion, ICompletionSession>
        {
            public UIElement GetUIElement(Completion itemToRender, ICompletionSession context, UIElementType elementType)
            {
                if (elementType == UIElementType.Tooltip)
                {
                    return new TestCompletionToolTip(itemToRender);
                }
                else
                {
                    return null;
                }
            }
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
