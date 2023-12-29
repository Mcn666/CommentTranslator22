using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Dictionary
{
    public class Dictionary
    {
        public static List<List<DictionaryResultFormat>> FormatLists { get; private set; }

        /// <summary>
        /// 获取字典，并且查找其翻译
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static DictionaryResultFormat Query(string word)
        {
            try
            {
                if (string.IsNullOrEmpty(word) || word.Length < 2)
                {
                    return null;
                }

                if (FormatLists == null)
                {
                    if (LoadResource() == false)
                    {
                        return null;
                    }
                }

                foreach (var format in FormatLists[word[0] - 'a'])
                {
                    if (Equals(format.en, word))
                    {
                        return format;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool LoadResource()
        {
            FormatLists = new List<List<DictionaryResultFormat>>();
            for (int i = 0; i < 26; i++)
            {
                var resourceName = $"Dictionary.DataTable.words-{(char)('a' + i)}.json";
                var str = AssemblyResource.GetResource(resourceName);
                if (string.IsNullOrEmpty(str))
                    break;

                var formats = JsonConvert.DeserializeObject<List<DictionaryResultFormat>>(str);
                if (formats == null)
                    break;

                FormatLists.Add(formats);
                if (i == 25 && FormatLists.Count == i + 1)
                    return true;
            }
            FormatLists = null;
            return false;
        }
    }
}
