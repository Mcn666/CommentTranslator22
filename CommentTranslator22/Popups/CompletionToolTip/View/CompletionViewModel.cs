using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace CommentTranslator22.Popups.CompletionToolTip.View
{
    public class CompletionViewModel : ViewModelBase
    {
        public ObservableCollection<StackPanel> CompletionItems { get; set; } = new ObservableCollection<StackPanel>();

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                SetProperty(ref selectedIndex, value);
            }
        }
        private int selectedIndex;

        public object Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }
        private object description;

        public string DescriptionTranslationResult
        {
            get => descriptionTranslationResult;
            set => SetProperty(ref descriptionTranslationResult, value);
        }
        private string descriptionTranslationResult;
    }
}
