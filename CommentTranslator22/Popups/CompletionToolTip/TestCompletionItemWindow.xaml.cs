using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CommentTranslator22.Popups.CompletionToolTip
{
    /// <summary>
    /// TestCompletionItemWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TestCompletionItemWindow : UserControl
    {
        public TestCompletionDescriptionWindow DescriptionWindow { get; set; }

        public TestCompletionItemWindow(TestCompletionDescriptionWindow window)
        {
            InitializeComponent();
            listBox.ItemsSource = new ObservableCollection<TextBlock>();
            DescriptionWindow = window;
        }

        IAsyncCompletionSession session;
        CompletionPresentationViewModel model;

        public Task UpdateCompletionItemAsync(IAsyncCompletionSession session, CompletionPresentationViewModel model)
        {
            if (this.model != null && this.model.SuggestionItem == model.SuggestionItem)
            {
                UpdateSelectItemIndex(model.SelectedItemIndex);
                return Task.CompletedTask;
            }
            else
            {
                this.session = session;
                this.model = model;
            }

            UpdateCompletionItems(model);
            UpdateSelectItemIndex(model.SelectedItemIndex);
            return Task.CompletedTask;
        }

        void UpdateCompletionItems(CompletionPresentationViewModel model)
        {
            if (listBox.ItemsSource is ObservableCollection<TextBlock> existingItems)
            {
                existingItems.Clear();

                foreach (var item in model.Items)
                {
                    var i = item.CompletionItem;
                    var block = new TextBlock();
                    block.Inlines.Add(new Run(i.DisplayText) { Foreground = GetBrush(i.Filters) });
                    existingItems.Add(block);
                }
            }
        }

        Brush GetBrush(ImmutableArray<CompletionFilter> filters)
        {
            if (filters.Count() > 0)
            {
                return GetBrush(filters[0].AccessKey);
            }
            return Brushes.LightGray;
        }

        public static Brush GetBrush(string key)
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

        void UpdateSelectItemIndex(int index)
        {
            if (listBox.SelectedIndex != index && index > -1 && index < listBox.Items.Count)
            {
                listBox.SelectedIndex = index;
            }
        }

        public event EventHandler OnClosed;

        public void Close()
        {
            OnClosed?.Invoke(this, EventArgs.Empty);
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 当选择项发生变化时，描述信息需要更新
            var index1 = listBox.SelectedIndex;
            var index2 = model.SelectedItemIndex;
            if (index1 != index2 && index1 > -1 && index1 < model.Items.Count())
            {
                var completion = this.model.Items[index1].CompletionItem;
                _ = this.DescriptionWindow.UpdateCompletionDescriptionAsync(session, completion);
            }

            // 检查选择项是否在视图中，如果不在，则滚动滑动条
            if (listBox.SelectedItem != null)
            {
                listBox.UpdateLayout();
                listBox.ScrollIntoView(listBox.SelectedItem);
            }

            if (DescriptionWindow != null)
            {
                Canvas.SetLeft(this.DescriptionWindow, this.VisualOffset.X + this.ActualWidth);
                Canvas.SetTop(this.DescriptionWindow, this.VisualOffset.Y);
            }
        }

        private void listBox_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (DescriptionWindow != null)
            {
                Canvas.SetLeft(this.DescriptionWindow, this.VisualOffset.X + this.ActualWidth);
                Canvas.SetTop(this.DescriptionWindow, this.VisualOffset.Y);
            }
        }

        public event EventHandler<CompletionItemEventArgs> CommitRequested;

        private void listBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var index = listBox.SelectedIndex;
            if (index < model.Items.Count())
            {
                CommitRequested?.Invoke(this, new CompletionItemEventArgs(model.Items[index].CompletionItem));
            }
            session?.Dismiss();
        }

        public event EventHandler<CompletionItemSelectedEventArgs> CompletionItemSelected;

        private void listBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var index1 = listBox.SelectedIndex;
            var index2 = model.SelectedItemIndex;
            if (index1 != index2 && index1 > -1 && index1 < model?.Items.Count())
            {
                var completion = this.model.Items[index1].CompletionItem;
                this.CompletionItemSelected?.Invoke(this, new CompletionItemSelectedEventArgs(completion, false));
            }
        }
    }
}
