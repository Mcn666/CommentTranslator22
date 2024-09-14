using CommentTranslator22.Translate;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CommentTranslator22.Popups.CompletionToolTip
{
    /// <summary>
    /// TestCompletionItemView.xaml 的交互逻辑
    /// </summary>
    public partial class TestCompletionItemView : UserControl
    {
        private readonly TestCompletionItemViewModel viewModel = new TestCompletionItemViewModel();
        private IAsyncCompletionSession session;
        private CompletionPresentationViewModel model;

        public TestCompletionItemView()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        /// <summary>
        /// 设置完成项集合
        /// </summary>
        /// <param name="session"></param>
        /// <param name="model"></param>
        public void SetCompletionCollection(IAsyncCompletionSession session, CompletionPresentationViewModel model)
        {
            if (session == null || model == null)
            {
                throw new ArgumentNullException(session == null ? nameof(session) : nameof(model));
            }

            if (this.model != null && this.model.SuggestionItem == model.SuggestionItem)
            {
                SetCompletionSelection(model.SelectedItemIndex);
                return;
            }

            this.session = session;
            this.model = model;
            this.viewModel.CompletionCollection.Clear();

            AddToListBox(model.SelectedItemIndex + 10);
            SetCompletionSelection(model.SelectedItemIndex);
        }


        /// <summary>
        /// 添加到列表框
        /// </summary>
        /// <param name="batchSize"> 批量添加数量 </param>
        private void AddToListBox(int batchSize = 5)
        {
            if (viewModel.CompletionCollection == null || model.Items == null || !model.Items.Any())
            {
                return;
            }

            if (viewModel.CompletionCollection.Count >= model.Items.Count())
            {
                return;
            }

            var batch = model.Items.Skip(viewModel.CompletionCollection.Count).Take(batchSize).ToList();
            foreach (var item in batch)
            {
                if (item.CompletionItem == null)
                {
                    continue;
                }

                var i = item.CompletionItem;
                var tb = new TextBlock();
                var brush = GetBrush(i.Filters);
                if (brush != null)
                {
                    tb.Inlines.Add(new Run(i.DisplayText) { Foreground = brush });
                }
                else
                {
                    tb.Inlines.Add(new Run(i.DisplayText));
                }
                viewModel.CompletionCollection.Add(tb);
            }
        }


        /// <summary>
        /// 设置选中项
        /// </summary>
        /// <param name="index"></param>
        private void SetCompletionSelection(int index)
        {
            if (viewModel == null || this.model == null || this.model.Items == null)
            {
                // 处理 viewModel 或 model 或 Items 为空的情况
                return;
            }

            if (viewModel.SelectedIndex != index && index > -1 && index < this.model.Items.Count())
            {
                viewModel.SelectedIndex = index;
            }

        }

        /// <summary>
        /// 设置描述
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private async Task SetCompletionDescriptionAsync(CompletionItem item)
        {
            viewModel.TranslatedDescription = "...";
            var tb = new TextBlock() { TextWrapping = TextWrapping.Wrap };

            var result = await item.Source.GetDescriptionAsync(session, item, default);
            if (result is ClassifiedTextElement ct && ct.Runs.Count() > 0)
            {
                foreach (var i in ct.Runs)
                {
                    tb.Inlines.Add(new Run(i.Text) { Foreground = GetBrush(i) });
                }
            }
            else if (result is ContainerElement ce && ce.Elements.Count() > 0)
            {
                foreach (var i in ce.Elements)
                {
                    if (i is ClassifiedTextElement cte)
                    {
                        foreach (var j in cte.Runs)
                        {
                            tb.Inlines.Add(new Run(j.Text) { Foreground = GetBrush(j) });
                        }
                    }
                    tb.Inlines.Add(new Run("\n"));
                }

                if (tb.Inlines.Count > 0)
                {
                    var e = tb.Inlines.ElementAt(tb.Inlines.Count - 1);
                    tb.Inlines.Remove(e);
                }

                if (ce.Elements.Count() > 1)
                {
                    SetDescriptionTranslation(ce.Elements.ElementAt(1));
                }
            }
            viewModel.Description = tb;
        }


        /// <summary>
        /// 设置翻译后的描述
        /// </summary>
        /// <param name="obj"></param>
        private void SetDescriptionTranslation(object obj)
        {
            if (obj is ClassifiedTextElement element)
            {
                var sb = new StringBuilder();
                foreach (var run in element.Runs)
                {
                    sb.Append(run.Text);
                }
                var text = sb.ToString();

                _ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
                {
                    var s = MethodAnnotationData.Instance.IndexOf(text) ?? GeneralAnnotationData.Instance.IndexOf(text);
                    if (s != null)
                    {
                        viewModel.TranslatedDescription = s;
                        return;
                    }

                    var r = await TranslateClient.Instance.TranslateAsync(text);
                    if (r.IsSuccess)
                    {
                        viewModel.TranslatedDescription = r.TargetText;
                        MethodAnnotationData.Instance.Add(r, "cs");
                    }
                });
            }
        }

        private Brush GetBrush(ImmutableArray<CompletionFilter> filters)
        {
            if (filters.Count() > 0)
            {
                return GetBrush(filters[0].AccessKey);
            }
            return Brushes.LightGray;
        }

        private Brush GetBrush(ClassifiedTextRun run)
        {
            return GetBrush(run.ClassificationTypeName);
        }

        private Brush GetBrush(string key)
        {
            switch (key)
            {
                case "c":   // 类
                case "d":   // 委托
                case "class name":      // 类名称
                case "delegate name":   // 委托名称
                    return new SolidColorBrush(Color.FromRgb(80, 180, 120));
                case "s":   // 结构体
                case "struct name":     // 结构体名称
                    return new SolidColorBrush(Color.FromRgb(135, 200, 145));
                case "i":   // 接口
                case "e":   // 枚举
                case "interface name":  // 接口名称
                case "enum name":       // 枚举名称
                case "type parameter name": // 类型参数名称 T
                    return new SolidColorBrush(Color.FromRgb(185, 215, 165));
                case "k":   // 关键字
                case "keyword":         // 关键字
                    return new SolidColorBrush(Color.FromRgb(80, 155, 215));
                case "m":   // 方法
                case "method name":     // 方法名称
                    return new SolidColorBrush(Color.FromRgb(220, 220, 155));
                case "l":   // 局部变量和参数
                case "parameter name":  // 参数名称
                    return new SolidColorBrush(Color.FromRgb(155, 220, 255));
                case "t":   // 片段
                    return new SolidColorBrush(Color.FromRgb(215, 160, 220));
                case "a":   // 未导入命名空间的项，在此项中有其所在的命名空间
                    return new SolidColorBrush(Color.FromRgb(125, 125, 125));
                case "n":   // 命名空间
                case "namespace name":  // 命名空间名称
                case "whitespace":      // 空白
                case "punctuation":     // 标点符号
                case "text":            // 文本
                default:
                    return Brushes.LightGray;
            }
        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource is ScrollViewer scrollViewer)
            {
                var vo = scrollViewer.VerticalOffset;
                var sh = scrollViewer.ScrollableHeight;
                if (vo == sh) // 如果已经滑动到底部
                {
                    AddToListBox();
                    scrollViewer.ScrollToVerticalOffset(vo);
                }
            }
        }

        public event EventHandler OnClosed;
        public event EventHandler<CompletionItemEventArgs> CommitRequested;
        public event EventHandler<CompletionItemSelectedEventArgs> CompletionItemSelected;

        public void Close()
        {
            OnClosed?.Invoke(this, EventArgs.Empty);
        }

        private void ListBox_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var index = (sender as ListBox).SelectedIndex;
            if (index < this.model.Items.Count())
            {
                this.CommitRequested?.Invoke(this, new CompletionItemEventArgs(model.Items[index].CompletionItem));
            }
            session?.Dismiss();
        }

        private void ListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var index1 = (sender as ListBox).SelectedIndex;
            var index2 = model.SelectedItemIndex;
            if (index1 != index2 && index1 > -1 && index1 < model?.Items.Count())
            {
                var item = model.Items[index1].CompletionItem;
                this.CompletionItemSelected?.Invoke(this, new CompletionItemSelectedEventArgs(item, false));
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem != null)
                {
                    listBox.UpdateLayout();
                    listBox.ScrollIntoView(listBox.SelectedItem);
                }

                _ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
                {
                    var index = listBox.SelectedIndex;
                    var item = model.Items[index].CompletionItem;
                    await SetCompletionDescriptionAsync(item);
                });
            }
        }
    }

    public class TestCompletionItemViewModel : ViewModelBase
    {
        public ObservableCollection<object> CompletionCollection { get; } = new ObservableCollection<object>();

        public TextBlock Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }
        private TextBlock _description;

        public string TranslatedDescription
        {
            get => _translatedDescription;
            set => SetProperty(ref _translatedDescription, value);
        }
        private string _translatedDescription = "...";

        public int SelectedIndex
        {
            get => _selectedIndex;
            set => SetProperty(ref _selectedIndex, value);
        }
        private int _selectedIndex;
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }
    }
}
