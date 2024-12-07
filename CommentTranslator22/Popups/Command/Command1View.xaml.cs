using CommentTranslator22.Popups.CompletionToolTip;
using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace CommentTranslator22.Popups.Command
{
    /// <summary>
    /// Command1View.xaml 的交互逻辑
    /// </summary>
    public partial class Command1View : UserControl
    {
        private readonly Command1ViewModel viewModel = new Command1ViewModel();

        public Command1View()
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public void TranslateText(string text)
        {
            viewModel.TranslateInfo = "(translating...)";
            viewModel.TranslateText = text;

            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                text = TranslateClient.Instance.HumpUnfold(text);
                var result = await TranslateClient.Instance.TranslateAsync(text);
                if (result.IsSuccess)
                {
                    viewModel.TranslateInfo = "(success)";
                    viewModel.TranslateText = result.TargetText;
                }
                else
                {
                    viewModel.TranslateInfo = "(failed)";
                    viewModel.TranslateText = "";
                }
            });
        }

        public event EventHandler OnClosed;

        public void Close()
        {
            OnClosed?.Invoke(this, EventArgs.Empty);
        }
    }

    public class Command1ViewModel : ViewModelBase
    {
        public string TranslateInfo
        {
            get => _translateInfo;
            set => SetProperty(ref _translateInfo, value);
        }
        private string _translateInfo = "";

        public string TranslateText
        {
            get => _translateText;
            set => SetProperty(ref _translateText, value);
        }
        private string _translateText = "";
    }
}
