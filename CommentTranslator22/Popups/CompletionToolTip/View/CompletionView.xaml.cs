using CommentTranslator22.Translate;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private CancellationTokenSource _descriptionCts;
        private ListBox _listBox; // 缓存 ListBox 引用

        public CompletionView()
        {
            InitializeComponent();
            _listBox = ControlFinder.FindByType<ListBox>(this);
            this.Unloaded += CompletionView_Unloaded;
        }

        private void CompletionView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            AdornmentLayerClose();
        }

        #region 装饰层反射调用

        public void AdornmentLayerClose()
        {
            if (_descriptionCts != null)
            {
                _descriptionCts.Cancel();
                _descriptionCts.Dispose();
                _descriptionCts = null;
            }
        }

        public void AdornmentLayerUpdate()
        {
            if (_listBox?.SelectedItem != null)
            {
                _listBox.ScrollIntoView(_listBox.SelectedItem);
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
            if (completionPresentationViewModel == null || !completionPresentationViewModel.ItemList.Any())
                return;

            if (ViewModel.CompletionItems.Count >= completionPresentationViewModel.ItemList.Count)
                return;

            var items = completionPresentationViewModel.ItemList
                .Skip(ViewModel.CompletionItems.Count)
                .Take(count)
                .ToList();

            var newItems = new List<CompletionItemModel>();
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
                    newItems.Add(tp);
                }
            }

            // 批量添加（使用扩展方法）
            ViewModel.CompletionItems.AddRange(newItems);
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

        private async Task SetDescriptionAsync(CompletionItem item, CancellationToken cancellationToken)
        {
            ViewModel.DescriptionTranslationResult = string.Empty;

            var textBlock = new TextBlock() { TextWrapping = System.Windows.TextWrapping.Wrap };
            var description = await item.Source.GetDescriptionAsync(session, item, cancellationToken)
                                         .ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            if (description is ClassifiedTextElement classified && classified.Runs.Any())
            {
                foreach (var run in classified.Runs)
                {
                    var brush = CompletionResources.GetBrush(run);
                    textBlock.Inlines.Add(new Run(run.Text) { Foreground = brush });
                }
            }
            else if (description is ContainerElement container && container.Elements.Any())
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
                    var lastInline = textBlock.Inlines.ElementAt(textBlock.Inlines.Count - 1);
                    textBlock.Inlines.Remove(lastInline);
                }

                _ = SetDescriptionTranslationResultAsync(container, cancellationToken);
            }

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            if (!cancellationToken.IsCancellationRequested)
            {
                ViewModel.Description = textBlock;
            }
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
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                    if (!cancellationToken.IsCancellationRequested)
                        ViewModel.DescriptionTranslationResult = result.TargetText;
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

                    _descriptionCts?.Cancel();
                    _descriptionCts?.Dispose();
                    _descriptionCts = new CancellationTokenSource();
                    var token = _descriptionCts.Token;

                    _ = SetDescriptionAsync(item, token).ContinueWith(t =>
                    {
                        if (t.IsFaulted && !(t.Exception?.InnerException is OperationCanceledException))
                        {
                            // 可记录日志
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

    // ObservableCollection 扩展方法，实现批量添加（仍触发逐个添加，但代码更清晰）
    public static class ObservableCollectionExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }
    }
}