using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;

namespace CommentTranslator22.Config
{
    public class ConfigB : DialogPage
    {

        #region 屏蔽的部分
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
        };
        #endregion


        [DisplayName("使用字典")]
        [Description("如果是支持的语言则会自动翻译，目前只支持【英文->中文】")]
        public bool UseDictionary { get; set; } = false;

        [DisplayName("使用相似度算法")]
        [Description("相似度大于85%的注释使用相同的结果")]
        public bool UseLevenshteinDistance { get; set; } = true;

        [DisplayName("覆盖代码完成提示信息")]
        [Description("暂未完成")]
        public bool UseCoverCodeCompletionPrompt { get; set; } = false;


        public void ReloadSetting(ConfigB config)
        {
            this.UseMask = config.UseMask;
            this.UseMaskType = config.UseMaskType;
            this.UseDictionary = config.UseDictionary;
            this.UseLevenshteinDistance = config.UseLevenshteinDistance;
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            base.OnApply(e);
            if (e.ApplyBehavior == ApplyKind.Apply)
            {
                CommentTranslator22Package.ConfigB = this;
            }
        }
    }
}
