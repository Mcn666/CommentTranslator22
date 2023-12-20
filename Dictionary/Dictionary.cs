using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace Dictionary
{
    public class Dictionary
    {
        public static List<DictionaryResultFormat> FormatList { get; private set; }

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

                if (FormatList == null)
                {
                    string dir = ReadEmbeddedResource($"Dictionary.DataTable.SimpleComparisonTable.json");
                    if (string.IsNullOrEmpty(dir) || dir is null)
                    {
                        return null;
                    }

                    // 反序列化
                    FormatList = JsonConvert.DeserializeObject<List<DictionaryResultFormat>>(dir);
                    if (FormatList == null)
                    {
                        return null;
                    }
                }

                foreach ( var format in FormatList)
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

        /// <summary>
        /// 获取程序集的嵌入资源
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        private static string ReadEmbeddedResource(string resourceName)
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
