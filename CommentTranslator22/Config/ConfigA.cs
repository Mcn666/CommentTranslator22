using CommentTranslator22.Translate.Enum;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace CommentTranslator22.Config
{
    public class ConfigA : DialogPage
    {
        [Category("翻译设置")]
        [DisplayName("翻译服务器")]
        public ServerEnum TranslationServer { get; set; } = ServerEnum.Bing;

        [Category("翻译设置")]
        [DisplayName("翻译源语言")]
        public LanguageEnum SourceLanguage { get; set; } = LanguageEnum.Auto;

        [Category("翻译设置")]
        [DisplayName("翻译目标语言")]
        public LanguageEnum TargetLanguage { get; set; } = GetCurrentCulture();

        [Category("翻译设置")]
        [DisplayName("多行合并")]
        public bool MergeCommentBlock { get; set; } = false;


        [Category("保存设置")]
        [DisplayName("保存的翻译数量")]
        public int NumberOfTranslationsSaved { get; set; } = 1000;



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
            CommentTranslator22Package.ConfigA.TranslationServer = TranslationServer;
            CommentTranslator22Package.ConfigA.SourceLanguage = SourceLanguage;
            CommentTranslator22Package.ConfigA.TargetLanguage = TargetLanguage;
            CommentTranslator22Package.ConfigA.MergeCommentBlock = MergeCommentBlock;

            CommentTranslator22Package.ConfigA.NumberOfTranslationsSaved = NumberOfTranslationsSaved;
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
