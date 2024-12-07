using CommentTranslator22.Popups;
using CommentTranslator22.Translate.Format;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommentTranslator22.Translate.TranslateData
{
    internal class MethodAnnotationData
    {
        public static MethodAnnotationData Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            internal static MethodAnnotationData instance = new MethodAnnotationData();

            static Nested() { }
        }

        class MethodAnnotationDataFormat
        {
            public int VisitsCount;
            public string Server;
            public string SourceLanguage;
            public string TargetLanguage;
            public string SourceText;
            public string TargetText;
            public LanguageEnum TargetLanguageCode;
        }

        class MethodAnnotationDataFileFormat
        {
            public static string MainFolder;
            public string FileName;
            public int MaximumStorageCount;
            public List<MethodAnnotationDataFormat> DataFormats = new List<MethodAnnotationDataFormat>();
        }

        List<MethodAnnotationDataFileFormat> Formats { get; set; } = new List<MethodAnnotationDataFileFormat>
        {
            new MethodAnnotationDataFileFormat { MaximumStorageCount = 3000, FileName = "default.txt"},
            new MethodAnnotationDataFileFormat { MaximumStorageCount = 3000, FileName = "cpp.txt"},
            new MethodAnnotationDataFileFormat { MaximumStorageCount = 3000, FileName = "cs.txt"},
        };

        MethodAnnotationData()
        {
            MethodAnnotationDataFileFormat.MainFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            MethodAnnotationDataFileFormat.MainFolder += "/CommentTranslator22/MethodAnnotationData";
            AffirmLocalFolderExists();
            AffirmLocalFileExists();
            ReadAllData();
            TestSolutionEvents.Instance.SolutionCloseFunc.Add(SaveAllData);
        }

        void ReadAllData()
        {
            foreach (var i in Formats)
            {
                Read(MethodAnnotationDataFileFormat.MainFolder, i.FileName, ref i.DataFormats);
            }
        }

        void Read(string filePath, string fileName, ref List<MethodAnnotationDataFormat> formats)
        {
            using (var fs = new FileStream($"{filePath}/{fileName}", FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var str = sr.ReadToEnd();
                    var temp = JsonConvert.DeserializeObject<List<MethodAnnotationDataFormat>>(str);
                    if (temp != null)
                    {
                        formats = temp;
                    }
                }
            }
        }

        void SaveAllData()
        {
            foreach (var i in Formats)
            {
                if (i.DataFormats.Count == 0)
                {
                    continue;
                }
                Save(MethodAnnotationDataFileFormat.MainFolder, i.FileName, i.MaximumStorageCount, i.DataFormats);
            }
        }

        void Sort(ref List<MethodAnnotationDataFormat> formats)
        {
            formats.Sort(delegate (MethodAnnotationDataFormat a, MethodAnnotationDataFormat b)
            {
                return a.VisitsCount > b.VisitsCount ? -1 : 1;
            });
        }

        void Join(ref List<MethodAnnotationDataFormat> formats1, in List<MethodAnnotationDataFormat> formats2)
        {
            foreach (var i in formats2)
            {
                if (formats1.Any(f => f.SourceText == i.SourceText) == true)
                {
                    if (i.VisitsCount > 1) // 防止重复记录
                    {
                        i.VisitsCount--;
                    }
                    continue;
                }
                formats1.Add(i);
            }

            Sort(ref formats1);
        }

        void Save(string filePath, string fileName, int length, List<MethodAnnotationDataFormat> formats)
        {
            var temp = new List<MethodAnnotationDataFormat>();
            Read(filePath, fileName, ref temp);
            Join(ref formats, temp);

            using (var fs = new FileStream($"{filePath}/{fileName}", FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (var sw = new StreamWriter(fs))
                {
                    if (formats.Count > length)
                    {
                        var count = formats.Count - length;
                        formats.RemoveRange(length, count);
                    }

                    var jsonStr = JsonConvert.SerializeObject(formats, Formatting.Indented);
                    sw.WriteLine(jsonStr);
                }
            }
        }

        void AffirmLocalFolderExists()
        {
            if (Directory.Exists(MethodAnnotationDataFileFormat.MainFolder) == false)
            {
                Directory.CreateDirectory(MethodAnnotationDataFileFormat.MainFolder);
            }
        }

        void AffirmLocalFileExists()
        {
            foreach (var i in Formats)
            {
                if (File.Exists($"{MethodAnnotationDataFileFormat.MainFolder}/{i.FileName}"))
                {
                    continue;
                }
                File.Create($"{MethodAnnotationDataFileFormat.MainFolder}/{i.FileName}").Close();
            }
        }

        public void Add(ApiRecvFormat format, string language = "default")
        {
            if (format.IsSuccess == false)
            {
                return;
            }

            var temp = new MethodAnnotationDataFormat()
            {
                VisitsCount = 1,
                Server = CommentTranslator22Package.Config.TranslationServer.ToString(),
                SourceLanguage = CommentTranslator22Package.Config.SourceLanguage.ToString(),
                TargetLanguage = CommentTranslator22Package.Config.TargetLanguage.ToString(),
                SourceText = format.SourceText,
                TargetText = format.TargetText,
                TargetLanguageCode = CommentTranslator22Package.Config.TargetLanguage,
            };

            foreach (var i in Formats)
            {
                if (i.DataFormats.Any(f => f.SourceText == temp.SourceText) == true)
                {
                    return;
                }
            }
            foreach (var i in Formats)
            {
                if (i.FileName == $"{language}.txt")
                {
                    i.DataFormats.Add(temp);
                }
            }
        }

        public string IndexOf(string str)
        {
            foreach (var i in Formats)
            {
                var r = IndexOf(i.DataFormats, str);
                if (r != null)
                {
                    return r;
                }
            }
            return null;
        }

        string IndexOf(List<MethodAnnotationDataFormat> formats, string str)
        {
            if (CommentTranslator22Package.Config.UseLevenshteinDistance)
            {
                foreach (var i in formats)
                {
                    if (LevenshteinDistance.LevenshteinDistancePercent(i.SourceText, str) > 0.85f
                        && i.TargetLanguageCode == CommentTranslator22Package.Config.TargetLanguage)
                    {
                        i.VisitsCount++;
                        return i.TargetText;
                    }
                }
            }
            else
            {
                foreach (var i in formats)
                {
                    if (i.SourceText == str
                        && i.TargetLanguageCode == CommentTranslator22Package.Config.TargetLanguage)
                    {
                        i.VisitsCount++;
                        return i.TargetText;
                    }
                }
            }
            return null;
        }
    }
}
