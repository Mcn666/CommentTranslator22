using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommentTranslator22.Dictionary
{
    internal class DictionaryUseData
    {
        public static DictionaryUseData Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            internal static DictionaryUseData instance = new DictionaryUseData();

            static Nested() { }
        }

        class LocalDictionaryDataFormat
        {
            public int VisitsCount;
            public DictionaryFormat DictionaryFormat;
        }

        class LocalDictionaryDataFileFormat
        {
            public static string MainFolder;
            public string FileName;
            public int MaximumStorageCount;
            public List<LocalDictionaryDataFormat> DataFormats = new List<LocalDictionaryDataFormat>();
        }

        List<LocalDictionaryDataFileFormat> FileFormats { get; set; } = new List<LocalDictionaryDataFileFormat>
        {
            new LocalDictionaryDataFileFormat{MaximumStorageCount = 10000, FileName = "default.txt"},
            new LocalDictionaryDataFileFormat{MaximumStorageCount = 10000, FileName = "unfound.txt"},
        };

        public enum StorageEnum
        {
            Default,
            Unfound
        }

        DictionaryUseData()
        {
            LocalDictionaryDataFileFormat.MainFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            LocalDictionaryDataFileFormat.MainFolder += "/CommentTranslator22/DictionaryUseData";
            AffirmLocalFolderExists();
            AffirmLocalFileExists();
            ReadAllData();
        }

        void ReadAllData()
        {
            foreach (var i in FileFormats)
            {
                Read(LocalDictionaryDataFileFormat.MainFolder, i.FileName, ref i.DataFormats);
            }
        }

        void Read(string filePath, string fileName, ref List<LocalDictionaryDataFormat> format)
        {
            using (var fs = new FileStream($"{filePath}/{fileName}", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var str = sr.ReadToEnd();
                    var temp = JsonConvert.DeserializeObject<List<LocalDictionaryDataFormat>>(str);
                    if (temp != null)
                    {
                        format = temp;
                    }
                }
            }
        }

        public void SaveAllData()
        {
            foreach (var i in FileFormats)
            {
                Sort(ref i.DataFormats);
                Save(LocalDictionaryDataFileFormat.MainFolder, i.FileName, i.MaximumStorageCount, i.DataFormats);
            }
        }

        void Sort(ref List<LocalDictionaryDataFormat> list)
        {
            // 将经常使用的字符集移动到前面位置
            list.Sort(delegate (LocalDictionaryDataFormat a, LocalDictionaryDataFormat b)
            {
                return a.VisitsCount > b.VisitsCount ? -1 : 1;
            });
        }

        void Save(string filePath, string fileName, int length, List<LocalDictionaryDataFormat> format)
        {
            using (var fs = new FileStream($"{filePath}/{fileName}", FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (var sw = new StreamWriter(fs))
                {
                    // 如果超出了最大保留值，就移除范围值后的所有项
                    if (format.Count > length)
                    {
                        var count = format.Count - length;
                        format.RemoveRange(length, count);
                    }

                    var jsonStr = JsonConvert.SerializeObject(format, Formatting.Indented);
                    sw.WriteLine(jsonStr);
                }
            }
        }

        void AffirmLocalFolderExists()
        {
            if (Directory.Exists(LocalDictionaryDataFileFormat.MainFolder) == false)
            {
                Directory.CreateDirectory(LocalDictionaryDataFileFormat.MainFolder);
            }
        }

        void AffirmLocalFileExists()
        {
            foreach (var i in FileFormats)
            {
                if (File.Exists($"{LocalDictionaryDataFileFormat.MainFolder}/{i.FileName}"))
                {
                    continue;
                }
                File.Create($"{LocalDictionaryDataFileFormat.MainFolder}/{i.FileName}").Close();
            }
        }

        public void Add(DictionaryFormat format, StorageEnum storage)
        {
            var index = (int)storage;
            if (index > FileFormats.Count || format == null)
            {
                return;
            }

            if (FileFormats[index].DataFormats.Any(f => f.DictionaryFormat.en == format.en))
            {
                var temp = FileFormats[index].DataFormats.First(f => f.DictionaryFormat.en == format.en);
                temp.VisitsCount++;
            }
            else
            {
                FileFormats[index].DataFormats.Add(new LocalDictionaryDataFormat()
                {
                    VisitsCount = 1,
                    DictionaryFormat = format
                });
            }
        }
    }
}
