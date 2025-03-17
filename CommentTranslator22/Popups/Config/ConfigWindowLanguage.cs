using CommentTranslator22.Translate;
using System.Collections.Generic;

namespace CommentTranslator22.Popups.Config
{
    public class ConfigWindowLanguage
    {
        public class Language
        {
            public string English { get; set; }
            public string Chinese { get; set; }
        }

        public static Dictionary<string, Language> Languages { get; set; } = new Dictionary<string, Language>()
        {
            { "ut", new Language() { English = "Translation", Chinese = "翻译" } },
            { "up", new Language() { English = "Phrase", Chinese = "短语翻译" } },
            { "ud", new Language() { English = "Dictionary", Chinese = "简易字典翻译" } },
            { "um", new Language() { English = "UseMask", Chinese = "代码屏蔽" } },
            { "ts", new Language() { English = "Server", Chinese = "服务器" } },
            { "sl", new Language() { English = "Source", Chinese = "源语言" } },
            { "tl", new Language() { English = "Target", Chinese = "翻译为" } },
        };

        public static string GetLanguage(string key)
        {
            if (Languages.ContainsKey(key))
            {
                var language = ConfigWindow.GetCurrentCulture();
                switch (language)
                {
                    case LanguageEnum.简体中文:
                    case LanguageEnum.繁體中文:
                        return Languages[key].Chinese;
                    case LanguageEnum.English:
                    default:
                        return Languages[key].English;
                }
            }
            return string.Empty;
        }
    }
}
