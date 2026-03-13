using CommentTranslator22.Translate;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace CommentTranslator22.Popups.CompletionToolTip.View
{
    /// <summary>
    /// CompletionView.xaml 的交互逻辑
    /// </summary>
    public partial class CompletionView : UserControl, IAdornmentLayerView
    {
        private IAsyncCompletionSession session;
        private CompletionPresentationViewModel completionPresentationViewModel;
        private bool isNoViewOperationChangingSelectedIndex;
        private CancellationTokenSource _descriptionCts; // 用于取消描述加载任务

        public CompletionView()
        {
            InitializeComponent();
        }

        #region 装饰层反射调用

        public void AdornmentLayerClose()
        {
            // 可在此释放资源或取消任务
            _descriptionCts?.Cancel();
        }

        public void AdornmentLayerUpdate()
        {
            var listBox = ControlFinder.FindByType<ListBox>(this);
            if (listBox != null && listBox.SelectedItem != null)
            {
                listBox.ScrollIntoView(listBox.SelectedItem);
            }
        }

        #endregion

        #region 更新完成列表和描述

        public void SetCompletionItems(IAsyncCompletionSession session, CompletionPresentationViewModel completionPresentationViewModel)
        {
            var index = completionPresentationViewModel.SelectedItemIndex;

            if (this.completionPresentationViewModel != null
                && this.completionPresentationViewModel.SuggestionItem == completionPresentationViewModel.SuggestionItem)
            {
                ChangeSelectedIndex(index);
                return;
            }

            this.session = session;
            this.completionPresentationViewModel = completionPresentationViewModel;
            this.ViewModel.CompletionItems.Clear();
            PopulateCompletionList();
            ChangeSelectedIndex(index);
        }

        private CompletionViewModel ViewModel => DataContext as CompletionViewModel;

        private void PopulateCompletionList(int count = 10)
        {
            if (completionPresentationViewModel == null || completionPresentationViewModel.ItemList.Any() == false)
                return;

            if (ViewModel.CompletionItems.Count >= completionPresentationViewModel.ItemList.Count)
                return;

            var items = completionPresentationViewModel.ItemList.Skip(ViewModel.CompletionItems.Count).Take(count).ToList();
            foreach (var item in items)
            {
                if (item.CompletionItem != null)
                {
                    var ci = item.CompletionItem;
                    var tp = new CompletionItemModel()
                    {
                        Icon = CompletionResources.GetCompletionImage(ci.Filters),
                        Text = ci.DisplayText,
                        Foreground = CompletionResources.GetBrush(ci.Filters),
                    };
                    ViewModel.CompletionItems.Add(tp);
                }
            }
        }

        private void ChangeSelectedIndex(int index)
        {
            if (this.completionPresentationViewModel != null && this.completionPresentationViewModel.ItemList != null)
            {
                if (ViewModel.SelectedIndex != index && index > -1 && index <= this.completionPresentationViewModel.ItemList.Count)
                {
                    if (index > ViewModel.CompletionItems.Count)
                    {
                        var count = index - ViewModel.CompletionItems.Count + 5;
                        PopulateCompletionList(count);
                    }

                    ViewModel.SelectedIndex = index;
                    isNoViewOperationChangingSelectedIndex = true;
                }
            }
        }

        // 修改为接受 CancellationToken
        private async Task SetDescriptionAsync(CompletionItem item, CancellationToken cancellationToken)
        {
            ViewModel.DescriptionTranslationResult = string.Empty;

            var textBlock = new TextBlock() { TextWrapping = System.Windows.TextWrapping.Wrap };
            var description = await item.Source.GetDescriptionAsync(session, item, cancellationToken)
                                         .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            if (description is ClassifiedTextElement classified && classified.Runs.Count() > 0)
            {
                foreach (var run in classified.Runs)
                {
                    var brush = CompletionResources.GetBrush(run);
                    textBlock.Inlines.Add(new Run(run.Text) { Foreground = brush });
                }
            }
            else if (description is ContainerElement container && container.Elements.Count() > 0)
            {
                foreach (var element in container.Elements)
                {
                    if (element is ClassifiedTextElement classified1)
                    {
                        foreach (var run in classified1.Runs)
                        {
                            var brush = CompletionResources.GetBrush(run);
                            textBlock.Inlines.Add(new Run(run.Text) { Foreground = brush });
                        }
                    }
                    textBlock.Inlines.Add(new LineBreak());
                }

                if (textBlock.Inlines.Count > 0)
                {
                    var element = textBlock.Inlines.ElementAt(textBlock.Inlines.Count - 1);
                    textBlock.Inlines.Remove(element);
                }

                // 启动翻译任务（注意传递 cancellationToken）
                _ = SetDescriptionTranslationResultAsync(container, cancellationToken);
            }

            // 回到 UI 线程更新 Description
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    ViewModel.Description = textBlock;
                }
            });
        }

        private async Task SetDescriptionTranslationResultAsync(ContainerElement container, CancellationToken cancellationToken)
        {
            if (container.Elements.Count() > 1 && container.Elements.ElementAt(1) is ClassifiedTextElement element)
            {
                var sb = new System.Text.StringBuilder();
                foreach (var run in element.Runs)
                {
                    sb.Append(run.Text);
                }
                var text = sb.ToString();

                var result = MethodTranslationData.Instance.GetTranslationResult(text);
                if (result == null)
                {
                    // 翻译可能耗时，检查取消令牌
                    cancellationToken.ThrowIfCancellationRequested();
                    result = await TranslationClient.Instance.TranslateAsync(text)
                                                      .ConfigureAwait(false);
                    if (result.IsSuccess)
                    {
                        MethodTranslationData.Instance.AddTranslationEntry(result.SourceText, result.TargetText);
                    }
                }
                if (result != null && !cancellationToken.IsCancellationRequested)
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            ViewModel.DescriptionTranslationResult = result.TargetText;
                    });
                }
            }
        }

        #endregion

        #region 事件

        public event EventHandler<CompletionItemEventArgs> CommitRequested;
        public event EventHandler<CompletionItemSelectedEventArgs> CompletionItemSelected;

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer scrollViewer)
            {
                var vo = scrollViewer.VerticalOffset;
                var sh = scrollViewer.ScrollableHeight;
                if (vo == sh)
                {
                    PopulateCompletionList();
                    scrollViewer.ScrollToVerticalOffset(vo);
                }
            }
        }

        private void ListBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var index = ViewModel.SelectedIndex;
            if (index < this.completionPresentationViewModel.ItemList.Count)
            {
                var item = completionPresentationViewModel.ItemList.ElementAt(index).CompletionItem;
                CommitRequested?.Invoke(this, new CompletionItemEventArgs(item));
            }
            session?.Dismiss();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem != null)
                {
                    listBox.ScrollIntoView(listBox.SelectedItem);
                }

                if (ViewModel.SelectedIndex > -1 && ViewModel.SelectedIndex < completionPresentationViewModel.ItemList.Count)
                {
                    var item = completionPresentationViewModel.ItemList.ElementAt(ViewModel.SelectedIndex).CompletionItem;

                    // 取消之前的任务，创建新的 CancellationTokenSource
                    _descriptionCts?.Cancel();
                    _descriptionCts = new CancellationTokenSource();
                    var token = _descriptionCts.Token;

                    // 启动新任务，不等待
                    _ = SetDescriptionAsync(item, token).ContinueWith(t =>
                    {
                        if (t.IsFaulted && !(t.Exception?.InnerException is OperationCanceledException))
                        {
                            // 记录异常（可选）
                        }
                    }, TaskScheduler.Default);
                }

                if (isNoViewOperationChangingSelectedIndex)
                {
                    isNoViewOperationChangingSelectedIndex = false;
                }

                if (ViewModel.SelectedIndex > ViewModel.CompletionItems.Count &&
                    ViewModel.SelectedIndex < this.completionPresentationViewModel.ItemList.Count)
                {
                    var count = ViewModel.SelectedIndex - ViewModel.CompletionItems.Count + 5;
                    PopulateCompletionList(count);
                }
            }
        }

        #endregion
    }
}