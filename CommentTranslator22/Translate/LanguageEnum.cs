namespace CommentTranslator22.Translate
{
    public enum LanguageEnum
    {
        Auto,
        English,
        简体中文,
        繁體中文,
        日本語,
    }

    public enum ServerEnum
    {
        Bing,
        Google,
        Baidu,
    }

    internal class LanguageCode
    {
        public static string[][] Code { get; set; } = new string[][]
        {
            new string[]{"auto-detect", "en", "zh-Hans", "zh-Hant", "ja"}, //Bing
            new string[]{"auto",        "en", "zh-CN",   "zh-TW",   "ja"}, //Google
            new string[]{"auto",        "en", "zh",      "cht",     "jp"}, //Baidu
        };
    }
}
