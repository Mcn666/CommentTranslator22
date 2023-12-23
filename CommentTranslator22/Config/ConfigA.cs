using CommentTranslator22.Translate.Enum;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel;
using static Microsoft.VisualStudio.VSConstants;

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



        /// <summary>
        /// 重新加载配置，用于VS重启后能够正确的加载设置
        /// </summary>
        /// <param name="config"></param>
        public void ReloadSetting(ConfigA config)
        {
            this.TranslationServer = config.TranslationServer;
            this.SourceLanguage = config.SourceLanguage;
            this.TargetLanguage = config.TargetLanguage;
            this.MergeCommentBlock = config.MergeCommentBlock;
            this.NumberOfTranslationsSaved = config.NumberOfTranslationsSaved;
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            if (e.ApplyBehavior == ApplyKind.Apply)
            {
                CommentTranslator22Package.ConfigA = this;
            }
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
