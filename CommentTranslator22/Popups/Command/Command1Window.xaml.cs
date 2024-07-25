using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Windows.Controls;

namespace CommentTranslator22.Popups.Command
{
    /// <summary>
    /// Command1Window.xaml 的交互逻辑
    /// </summary>
    public partial class Command1Window : UserControl
    {
        public Command1Window()
        {
            InitializeComponent();
            DataContext = new BindingData();
        }


        public void TranslateText(string text)
        {
            (DataContext as BindingData).TranslateResult = "...";

            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                text = TranslateClient.Instance.HumpUnfold(text);
                var r = await TranslateClient.Instance.TranslateAsync(text);
                if (r.IsSuccess)
                {
                    (DataContext as BindingData).TranslateResult = r.TargetText;
                }
                else
                {
                    (DataContext as BindingData).TranslateResult = "translation failure";
                }
            });
        }

        public event EventHandler OnClosed;

        public void Close()
        {
            OnClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    public class BindingData : INotifyPropertyChanged
    {
        private string translateResult;
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
