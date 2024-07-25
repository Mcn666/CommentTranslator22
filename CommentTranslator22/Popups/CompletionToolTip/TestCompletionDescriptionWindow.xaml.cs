using CommentTranslator22.Translate;
using CommentTranslator22.Translate.TranslateData;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CommentTranslator22.Popups.CompletionToolTip
{
    /// <summary>
    /// TestCompletionDescriptionWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TestCompletionDescriptionWindow : UserControl
    {
        public TestCompletionDescriptionWindow()
        {
            InitializeComponent();
            DataContext = new BindingData();
        }

        public async Task UpdateCompletionDescriptionAsync(IAsyncCompletionSession session, CompletionItem item)
        {
            textBlock.Inlines.Clear();
            (DataContext as BindingData).TranslateResult = "...";

            var result = await item.Source.GetDescriptionAsync(session, item, default);
            if (result is ClassifiedTextElement ct && ct.Runs.Count() > 0)
            {
                foreach (var i in ct.Runs)
                {
                    textBlock.Inlines.Add(new Run(i.Text) { Foreground = GetBrush(i) });
                }
            }
            else if (result is ContainerElement ce && ce.Elements.Count() > 0)
            {
                foreach (var i in ce.Elements)
                {
                    if (i is ClassifiedTextElement element)
                    {
                        foreach (var ii in element.Runs)
                        {
                            textBlock.Inlines.Add(new Run(ii.Text) { Foreground = GetBrush(ii) });
                        }
                    }
                    textBlock.Inlines.Add(new Run("\n"));
                }
                var e = textBlock.Inlines.ElementAt(textBlock.Inlines.Count - 1);
                textBlock.Inlines.Remove(e);

                if (ce.Elements.Count() > 1)
                {
                    TranslateText(ce.Elements.ElementAt(1));
                }
            }
        }

        void TranslateText(object o)
        {
            if (o is ClassifiedTextElement element)
            {
                var sb = new StringBuilder();
                foreach (var i in element.Runs)
                {
                    sb.Append(i.Text);
                }
                var text = sb.ToString();

                _ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
                {
                    var s = MethodAnnotationData.Instance.IndexOf(text) ?? GeneralAnnotationData.Instance.IndexOf(text);
                    if (s != null)
                    {
                        (DataContext as BindingData).TranslateResult = s;
                        return;
                    }

                    var r = await TranslateClient.Instance.TranslateAsync(text);
                    if (r.IsSuccess)
                    {
                        (DataContext as BindingData).TranslateResult = r.TargetText;
                        MethodAnnotationData.Instance.Add(r, "cs");
                    }
                });
            }
        }

        Brush GetBrush(ClassifiedTextRun run)
        {
            return TestCompletionItemWindow.GetBrush(run.ClassificationTypeName);
        }

        public event EventHandler OnClosed;

        public void Close()
        {
            OnClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    public class BindingData : INotifyPropertyChanged
    {
        private string translateResult = string.Empty;
        public string TranslateResult
        {
            get { return translateResult; }
            set
            {
                if (translateResult != value)
                {
                    translateResult = value;
                    OnPropertyChanged(nameof(TranslateResult));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
