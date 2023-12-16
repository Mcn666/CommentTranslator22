using CommentTranslator22.Popup;
using CommentTranslator22.Translate.Enum;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace CommentTranslator22.Config
{
    public class ConfigA : DialogPage
    {
        [Category("翻译设置")]
        [DisplayName("翻译服务器")]
        public ServerEnum Servers { get; set; } = ServerEnum.Bing;

        [Category("翻译设置")]
        [DisplayName("翻译源语言")]
        public LanguageEnum LanguageFrom { get; set; } = LanguageEnum.Auto;

        [Category("翻译设置")]
        [DisplayName("翻译目标语言")]
        public LanguageEnum LanguageTo { get; set; } = GetCurrentCulture();

        [Category("翻译设置")]
        [DisplayName("多行合并")]
        public bool MergeCommentBlock { get; set; } = false;


        [Category("保存设置")]
        [DisplayName("翻译结果保存数量")]
        public int TranslateResultMaximumSave { get; set; } = 1000;



        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            if (e.ApplyBehavior == ApplyKind.Apply)
            {
                SaveToSetting();
            }
        }

        public void SaveToSetting()
        {
            CommentTranslator22Package.ConfigA.Servers = Servers;
            CommentTranslator22Package.ConfigA.LanguageFrom = LanguageFrom;
            CommentTranslator22Package.ConfigA.LanguageTo = LanguageTo;
            CommentTranslator22Package.ConfigA.MergeCommentBlock = MergeCommentBlock;

            CommentTranslator22Package.ConfigA.TranslateResultMaximumSave = TranslateResultMaximumSave;
        }

        private static LanguageEnum GetCurrentCulture()
        {
            string currentCulture = System.Globalization.CultureInfo.CurrentCulture.Name;
            switch (currentCulture)
            {
                case "ja-JP":
                    return LanguageEnum.日本語;
                case "zh-CN":
                    return LanguageEnum.简体中文;
                case "zh-TW":
                    return LanguageEnum.繁體中文;
                case "en-US":
                    return LanguageEnum.English;
                default:
                    return LanguageEnum.简体中文;
            }
        }
    }
}
