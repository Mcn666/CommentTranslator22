using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommentTranslator22.Dictionary
{
    internal static class LocalDictionary
    {
        internal class LocalDictionaryFormat
        {
            public int VisitsCount;
            public DictionaryResultFormat DictionaryResult;
        }

        static readonly string savePathFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/CommentTranslator22/LocalDictionary";
        static readonly string[] savePathFile =
        {
            "frequently use character repertoire.txt",
            "no result character.txt"
        };
        static readonly int[] saveMaxQuantity =
        {
            10000,
            10000
        };
        static List<LocalDictionaryFormat>[] localDictionaries =
        {
            new List<LocalDictionaryFormat> {},
            new List<LocalDictionaryFormat> {}
        };
        static List<LocalDictionaryFormat> tempLocalDictionarie = new List<LocalDictionaryFormat>();


        public static void AddTempLocalDictionarie(DictionaryResultFormat format)
        {
            foreach (var item in tempLocalDictionarie)
            {
                if (item.DictionaryResult == format)
                {
                    item.VisitsCount++;
                    return;
                }
                else
                {
                    continue;
                }
            }

            tempLocalDictionarie.Add(new LocalDictionaryFormat
            {
                VisitsCount = 1,
                DictionaryResult = format
            });
        }

        static void AddFrequentlyUseCharacterRepertoire(LocalDictionaryFormat format)
        {
            foreach (var item in localDictionaries[0])
            {
                if (item.DictionaryResult == format.DictionaryResult)
                {
                    item.VisitsCount += format.VisitsCount;
                    return;
                }
                else
                {
                    continue;
                }
            }

            localDictionaries[0].Add(format);
        }

        public static void AddNoResultCharacter(DictionaryResultFormat format)
        {
            foreach (var item in localDictionaries[1])
            {
                if (item.DictionaryResult.en == format.en)
                {
                    item.VisitsCount += 1;
                    return;
                }
                else
                {
                    continue;
                }
            }

            localDictionaries[1].Add(new LocalDictionaryFormat()
            {
                VisitsCount = 1,
                DictionaryResult = format
            });
        }

        static void Read(string filePath, ref List<LocalDictionaryFormat> localDictionaryFormats)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var str = sr.ReadToEnd();
                    var temp = JsonConvert.DeserializeObject<List<LocalDictionaryFormat>>(str);
                    if (temp != null)
                    {
                        localDictionaryFormats = temp;
                    }
                    sr.Close();
                }
            }
        }

        public static void ReadAllData()
        {
            // 检查文件结构，再以共享读写的方式打开需要使用到的文件
            AffirmLocalPathStruct();

            for (int i = 0; i < localDictionaries.Count(); i++)
            {
                Read($"{savePathFolder}/{savePathFile[i]}", ref localDictionaries[i]);
            }
        }

        static void Save(string filePath, int length, List<LocalDictionaryFormat> localDictionaryFormats)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (var sw = new StreamWriter(fs))
                {
                    // 如果超出了最大保留值，就移除范围值后的所有项
                    if (localDictionaryFormats.Count > length)
                    {
                        var count = localDictionaryFormats.Count - length;
                        localDictionaryFormats.RemoveRange(length, count);
                    }

                    var jsonStr = JsonConvert.SerializeObject(localDictionaryFormats, Formatting.Indented);
                    sw.WriteLine(jsonStr);

                    sw.Flush();
                    sw.Close();
                }
            }
        }

        public static void SaveAllData()
        {
            // 检查文件结构
            AffirmLocalPathStruct();

            if (tempLocalDictionarie.Count > 3)
            {
                Sort(ref tempLocalDictionarie);
                foreach (var item in tempLocalDictionarie)
                {
                    AddFrequentlyUseCharacterRepertoire(item);
                }
            }

            for (int i = 0; i < localDictionaries.Count(); i++)
            {
                if (localDictionaries[i].Count < 3)
                {
                    continue;
                }
                Sort(ref localDictionaries[i]);
                Save($"{savePathFolder}/{savePathFile[i]}", saveMaxQuantity[i], localDictionaries[i]);
            }
        }

        static void Sort(ref List<LocalDictionaryFormat> list)
        {
            // 将经常使用的字符集移动到前面位置
            list.Sort(delegate (LocalDictionaryFormat a, LocalDictionaryFormat b)
            {
                return a.VisitsCount > b.VisitsCount ? -1 : 1;
            });
        }

        static void AffirmLocalPathStruct()
        {
            if (Directory.Exists(savePathFolder) == false)
            {
                Directory.CreateDirectory(savePathFolder);
            }

            foreach (var item in savePathFile)
            {
                if (string.IsNullOrEmpty(item) == true || File.Exists($"{savePathFolder}/{item}") == true)
                {
                    continue;
                }
                File.Create($"{savePathFolder}/{item}");
            }
        }
    }
}
