namespace CommentTranslator22.Translate.Enum
{
    internal class LanguageCode
    {
        public static string[][] Code { get; set; } = new string[][]
        {
            // 此顺序与 LanguageEnum 顺序一致
            new string[]{"auto-detect", "en", "zh-Hans", "zh-Hant", "ja"}, //Bing
            new string[]{"auto",        "en", "zh-CN",   "zh-TW",   "ja"}, //Google
        };

        //public static string n = Code[(int)TranslateServerEnum.Bing][(int)LanguageEnum.简体中文];
        //public static string n = Code[TranslateServerEnum.Bing.GetHashCode()][LanguageEnum.简体中文.GetHashCode()];
    }
}
