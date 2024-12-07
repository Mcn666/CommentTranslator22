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
        [DisplayName("Appid")]
        [Description("只有使用百度翻译时才会生效")]
        public string AppId { get; set; }

        [Category("翻译设置")]
        [DisplayName("Appkey")]
        [Description("只有使用百度翻译时才会生效")]
        public string SecretKey { get; set; }
        #endregion

        #region 翻译内容
        [Category("翻译内容")]
        [DisplayName("翻译快速信息文本")]
        [Description("鼠标指向函数、方法、变量、类、枚举等文本时，翻译弹出时的部分快速信息文本")]
        public bool TranslateQuickInfoCommentText { get; set; } = true;

        [Category("翻译内容")]
        [DisplayName("翻译普通注释文本")]
        [Description("翻译鼠标所指向的注释文本及其附近的注释文本")]
        public bool TranslateGeneralCommentText { get; set; } = true;

        [Category("翻译内容")]
        [DisplayName("使用字典")]
        [Description("如果是支持的语言则会自动翻译")]
        public bool UseDictionary { get; set; } = true;

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
            "?* ?*}",
            "?* ?*;",
            "?*?*)",
        };
        #endregion

        [DisplayName("使用相似度算法")]
        [Description("相似度大于85%的注释使用相同的结果")]
        public bool UseLevenshteinDistance { get; set; } = false;

        [DisplayName("缓解UI卡顿问题")]
        [Description("如果更改了此项，需要重启VS。用于控制布局更新时检查UI状态的选项")]
        public bool UseUiLimit { get; set; } = false;



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

            // 翻译内容
            this.TranslateQuickInfoCommentText = config.TranslateQuickInfoCommentText;
            this.TranslateGeneralCommentText = config.TranslateGeneralCommentText;
            this.UseDictionary = config.UseDictionary;

            // 屏蔽设置
            this.UseMask = config.UseMask;
            this.UseMaskType = config.UseMaskType;

            this.UseLevenshteinDistance = config.UseLevenshteinDistance;
            this.UseUiLimit = config.UseUiLimit;
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
