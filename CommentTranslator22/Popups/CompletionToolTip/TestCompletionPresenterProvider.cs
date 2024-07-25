using CommentTranslator22.Popups.Command;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using static System.Net.Mime.MediaTypeNames;

namespace CommentTranslator22.Popups.CompletionToolTip
{
    [Export(typeof(ICompletionPresenterProvider))]
    [Name(nameof(TestCompletionPresenterProvider))]
    [ContentType("CSharp")]
    internal class TestCompletionPresenterProvider : ICompletionPresenterProvider
    {
        public CompletionPresenterOptions Options { get; } = new CompletionPresenterOptions(10);

        public ICompletionPresenter GetOrCreate(ITextView textView)
        {
            return new TestCompletionPresenter(textView);
        }
    }

    internal class TestCompletionPresenter : ICompletionPresenter
    {
        private readonly ITextView view;

        public TestCompletionPresenter(ITextView view)
        {
            this.view = view;
        }

        public event EventHandler<CompletionFilterChangedEventArgs> FiltersChanged; // 通知用户更改筛选器的选择状态
        public event EventHandler<CompletionItemSelectedEventArgs> CompletionItemSelected;
        public event EventHandler<CompletionItemEventArgs> CommitRequested; // 事件触发后将当前选项提交到文本中
        public event EventHandler<CompletionClosedEventArgs> CompletionClosed; // 事件触发后关闭代码完成提示

        public void Close()
        {
            CompletionClosed?.Invoke(this, new CompletionClosedEventArgs(view));
            TestAdornmentLayer.CloseWindow(view, typeof(TestCompletionItemWindow));
            TestAdornmentLayer.CloseWindow(view, typeof(TestCompletionDescriptionWindow));
        }

        public void Dispose()
        {
            
        }

        public void Open(IAsyncCompletionSession session, CompletionPresentationViewModel model)
        {
            DoUpdate(session, model);
        }

        public void Update(IAsyncCompletionSession session, CompletionPresentationViewModel model)
        {
            DoUpdate(session, model);
        }

        void DoUpdate(IAsyncCompletionSession session, CompletionPresentationViewModel model)
        {
            // 这个方法用于打开UI并更新内容，问题点：
            //  我目前还没有找到相关的可供参考源，所以UI使用的是WPF控件
            //  UI现在显示在IWpfTextView层中，但VS默认的UI不是绘制在此层
            //  使用鼠标单击选择项后再用键盘上下选择时，存在选择项还原的问题，如果点击两次（非双击）就正常，原因未知
            //  输入字符"."获取成员时，会导致UI关闭，原因未知，但我发现添加到IAdornmentLayer图层的UI被移除了

            var span = session.ApplicableToSpan.GetSpan(view.TextSnapshot);
            var window1 = TestAdornmentLayer.GetWindow(view, span, typeof(TestCompletionItemWindow));
            var window2 = TestAdornmentLayer.GetWindow(view, span, typeof(TestCompletionDescriptionWindow), false);
            if (window1 == default || window2 == default)
            {
                session.Dismiss();
                return;
            }

            var w1 = window1 as TestCompletionItemWindow;
            var w2 = window2 as TestCompletionDescriptionWindow;

            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await w1.UpdateCompletionItemAsync(session, model);
                w1.CommitRequested += Window1_CommitRequested;
                w1.CompletionItemSelected += Window1_CompletionItemSelected;

                var index = model.SelectedItemIndex;
                if (index > -1)
                {
                    var item = model.Items[index].CompletionItem;
                    await w2.UpdateCompletionDescriptionAsync(session, item);
                }
            });
        }

        private void Window1_CompletionItemSelected(object sender, CompletionItemSelectedEventArgs e)
        {
            // 通过引发此事件来更改UI中的选择项，问题点：
            //  触发此事件会导致多次循环
            if (sender is TestCompletionItemWindow window)
            {
                window.CompletionItemSelected -= Window1_CompletionItemSelected;
                CompletionItemSelected?.Invoke(this, e);
            }
        }

        private void Window1_CommitRequested(object sender, CompletionItemEventArgs e)
        {
            // 通过引发此事件来提交完成项，问题点：
            //  提交后编辑器视图没有获得焦点，需要鼠标点击编辑器视图。可能的原因：使用鼠标双击提交完成项时，焦点在UI上
            if (sender is TestCompletionItemWindow window)
            {
                window.CommitRequested -= Window1_CommitRequested;
                CommitRequested?.Invoke(this, e);
                Close();
            }
        }
    }
}
