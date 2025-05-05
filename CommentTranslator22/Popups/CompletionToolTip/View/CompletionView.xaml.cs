using CommentTranslator22.Translate;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace CommentTranslator22.Popups.CompletionToolTip.View
{
    /// <summary>
    /// CompletionView.xaml 的交互逻辑
    /// </summary>
    public partial class CompletionView : UserControl
    {
        private IAsyncCompletionSession session;
        private CompletionPresentationViewModel completionPresentationViewModel;
        private bool isNoViewOperationChangingSelectedIndex;

        public CompletionView()
        {
            InitializeComponent();
        }

        #region 装饰层反射调用

        public void AdornmentLayerClose()
        {

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

        /// <summary>
        /// 填充完成列表
        /// </summary>
        private void PopulateCompletionList(int count = 10)
        {
            if (completionPresentationViewModel == null || completionPresentationViewModel.ItemList.Any() == false)
            {
                return;
            }

            if (ViewModel.CompletionItems.Count >= completionPresentationViewModel.ItemList.Count)
            {
                return;
            }

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

        private async Task SetDescriptionAsync(CompletionItem item)
        {
            ViewModel.DescriptionTranslationResult = string.Empty;

            var textBlock = new TextBlock() { TextWrapping = System.Windows.TextWrapping.Wrap };
            var description = await item.Source.GetDescriptionAsync(session, item, default);
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

                if (textBlock.Inlines.Count > 0) // 移除最后一个换行符
                {
                    var element = textBlock.Inlines.ElementAt(textBlock.Inlines.Count - 1);
                    textBlock.Inlines.Remove(element);
                }

                _ = SetDescriptionTranslationResultAsync(container);
            }
            
            ViewModel.Description = textBlock;
        }

        private async Task SetDescriptionTranslationResultAsync(ContainerElement container)
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
                    result = await TranslationClient.Instance.TranslateAsync(text);
                    if (result.IsSuccess)
                    {
                        MethodTranslationData.Instance.AddTranslationEntry(result.SourceText, result.TargetText);
                    }
                }
                if (result != null)
                {
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
                if (vo == sh) // 如果已经滑动到底部
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
                    _ = SetDescriptionAsync(item);
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
