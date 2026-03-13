using CommentTranslator22.Translate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.QuickInfo.Comment
{
    internal class LanguageResources
    {
        private static readonly Dictionary<LanguageEnum, Dictionary<string, string>> _resources
        = new Dictionary<LanguageEnum, Dictionary<string, string>>
        {
            {
                LanguageEnum.简体中文, new Dictionary<string, string>
                {
                    { "Internet", "网络翻译" },
                    { "Buffer", "本地缓存" },
                    { "SimpleDictionary", "简易词典" },
                    { "Summary", "简述" },
                    { "Returns", "结果" },
                }
            },
            {
                LanguageEnum.English, new Dictionary<string, string>
                {
                    { "Internet", "Internet" },
                    { "Buffer", "Buffer" },
                    { "SimpleDictionary", "SimpleDictionary" },
                    { "Summary", "Summary" },
                    { "Returns", "Returns" }
                }
            }
        };

        public static string GetLocalizedString(string key)
        {
            var currentCulture = GetCurrentCulture();
            if (_resources.TryGetValue(currentCulture, out var dict) &&
                dict.TryGetValue(key, out var value))
            {
                return value;
            }
            return key; // 默认返回原 key
        }

        public static LanguageEnum GetCurrentCulture()
        {
            string currentCulture = System.Globalization.CultureInfo.CurrentCulture.Name;
            switch (currentCulture)
            {
                case "zh-CN":
                    return LanguageEnum.简体中文;;
                default:
                    return LanguageEnum.English;
            }
        }
    }
}
