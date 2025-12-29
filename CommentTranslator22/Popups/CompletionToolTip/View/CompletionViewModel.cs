using System.Collections.ObjectModel;
using System.Windows.Media;

namespace CommentTranslator22.Popups.CompletionToolTip.View
{
    public class CompletionViewModel : ViewModelBase
    {
        public ObservableCollection<CompletionItemModel> CompletionItems { get; set; } = new ObservableCollection<CompletionItemModel>();

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

    public class CompletionItemModel
    {
        public string Text { get; set; }
        //public string Description { get; set; }
        //public string DescriptionTranslationResult { get; set; }
        public ImageSource Icon { get; set; }
        //public Brush Background { get; set; }
        public Brush Foreground { get; set; }
    }
}
