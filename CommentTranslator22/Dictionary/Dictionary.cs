using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace CommentTranslator22.Dictionary
{
    internal class Dictionary
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
                var resourceName = $"CommentTranslator22.Dictionary.Data.words-{(char)('a' + i)}.json";
                var str = GetResource(resourceName);
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

        /// <summary>
        /// 获取程序集的嵌入资源
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        private static string GetResource(string resourceName)
        {
            try
            {
                // 获取当前程序集
                Assembly assembly = Assembly.GetExecutingAssembly();

                // 获取资源流
                using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (resourceStream == null)
                    {
                        return null;
                    }

                    // 使用资源流进行操作
                    using (StreamReader reader = new StreamReader(resourceStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
