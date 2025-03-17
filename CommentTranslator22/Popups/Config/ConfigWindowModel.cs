using CommentTranslator22.Translate;
using System.Collections.Generic;

namespace CommentTranslator22.Popups.Config
{
    public class ConfigWindowModel
    {
        public bool UseDefaultTranslation { get; set; } = true;
        public bool UsePhraseTranslation { get; set; } = true;
        public bool UseDictionaryTranslation { get; set; } = true;
        public int ServerInt { get; set; } = (int)ServerEnum.Bing;
        public ServerEnum TranslationServer => (ServerEnum)ServerInt;
        public int SourceLanguageInt { get; set; } = (int)LanguageEnum.Auto;
        public LanguageEnum SourceLanguage => (LanguageEnum)SourceLanguageInt;
        public int TargetLanguageInt { get; set; } = (int)ConfigWindow.GetCurrentCulture();
        public LanguageEnum TargetLanguage => (LanguageEnum)TargetLanguageInt;
        public string AppId { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public bool UseMask { get; set; } = true;
        public List<string> UseMaskType { get; set; } = new List<string>()
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
    }
}
