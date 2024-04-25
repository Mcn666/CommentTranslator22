using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace CommentTranslator22
{
    public class CommentTranslator22Config : DialogPage
    {
        #region 翻译设置
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

        [Category("翻译设置")]
        [DisplayName("保存的翻译数量")]
        public int NumberOfTranslationsSaved { get; set; } = 1000;
        #endregion

        #region 屏蔽设置
        [Category("屏蔽设置")]
        [DisplayName("使用屏蔽")]
        public bool UseMask { get; set; } = true;

        [Category("屏蔽设置")]
        [DisplayName("屏蔽类型")]
        // UITypeEditor => System.Drawing.Design.UITypeEditor
        [Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design", typeof(UITypeEditor))]
        public List<string> UseMaskType { get; set; } = new List<string>
        {
            "<*>",
            "<*>*<*>",
            "?* ?* = ?*(*);",
            "?* ?* = ?*(*,",
            "?* ?*(*);",
            "?* ?*(*,",
            "?*.?*(*);",
            "?*.?*(*,",
            "?* (?*)?*;",
            "?*<?*> ?*;",
            "*?param *",
            "*http*://*",
            "?*/?*/?*",
            "?*\\?*\\?*",
            "?*:*;",
        };
        #endregion

        [DisplayName("使用字典")]
        [Description("如果是支持的语言则会自动翻译，目前只支持【英文->中文】")]
        public bool UseDictionary { get; set; } = false;

        [DisplayName("使用相似度算法")]
        [Description("相似度大于85%的注释使用相同的结果")]
        public bool UseLevenshteinDistance { get; set; } = true;

        [DisplayName("使用字符统计")]
        public bool UseCharacterStatistics { get; set; } = true;

        [DisplayName("覆盖代码完成提示信息")]
        [Description("暂未完成")]
        public bool UseCoverCodeCompletionPrompt { get; set; } = false;




        /// <summary>
        /// 重新加载配置，用于VS重启后能够正确的加载设置
        /// </summary>
        /// <param name="config"></param>
        public void ReloadSetting(CommentTranslator22Config config)
        {
            // 翻译设置
            this.TranslationServer = config.TranslationServer;
            this.SourceLanguage = config.SourceLanguage;
            this.TargetLanguage = config.TargetLanguage;
            this.MergeCommentBlock = config.MergeCommentBlock;
            this.NumberOfTranslationsSaved = config.NumberOfTranslationsSaved;

            // 屏蔽设置
            this.UseMask = config.UseMask;
            this.UseMaskType = config.UseMaskType;


            this.UseDictionary = config.UseDictionary;
            this.UseCharacterStatistics = config.UseCharacterStatistics;
            this.UseLevenshteinDistance = config.UseLevenshteinDistance;
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            if (e.ApplyBehavior == ApplyKind.Apply)
            {
                CommentTranslator22Package.Config = this;
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
