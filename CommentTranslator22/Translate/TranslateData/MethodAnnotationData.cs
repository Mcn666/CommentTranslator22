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
            public int StorageLength;
            public List<MethodAnnotationDataFormat> DataFormats = new List<MethodAnnotationDataFormat>();
        }

        List<MethodAnnotationDataFileFormat> FileFormats { get; set; } = new List<MethodAnnotationDataFileFormat>
        {
            new MethodAnnotationDataFileFormat { StorageLength = 1000, FileName = "default.txt"},
        };

        MethodAnnotationData()
        {
            MethodAnnotationDataFileFormat.MainFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            MethodAnnotationDataFileFormat.MainFolder += "/CommentTranslator22/MethodAnnotationData";
            AffirmLocalFolderExists();
            AffirmLocalFileExists();
            ReadAllData();
        }

        void ReadAllData()
        {
            foreach (var i in FileFormats)
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

        public void SaveAllData()
        {
            foreach (var i in FileFormats)
            {
                if (i.DataFormats.Count == 0)
                {
                    continue;
                }
                Sort(ref i.DataFormats);
                Save(MethodAnnotationDataFileFormat.MainFolder, i.FileName, i.StorageLength, i.DataFormats);
            }
        }

        void Sort(ref List<MethodAnnotationDataFormat> formats)
        {
            formats.Sort(delegate (MethodAnnotationDataFormat a, MethodAnnotationDataFormat b)
            {
                return a.VisitsCount > b.VisitsCount ? -1 : 1;
            });
        }

        void Save(string filePath, string fileName, int length, List<MethodAnnotationDataFormat> formats)
        {
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
            foreach (var i in FileFormats)
            {
                if (File.Exists($"{MethodAnnotationDataFileFormat.MainFolder}/{i.FileName}"))
                {
                    continue;
                }
                File.Create($"{MethodAnnotationDataFileFormat.MainFolder}/{i.FileName}").Close();
            }
        }

        public void Add(ApiRecvFormat format)
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

            if (FileFormats[0].DataFormats.Any(f => f == temp) == false)
            {
                FileFormats[0].DataFormats.Add(temp);
            }
        }

        public string IndexOf(string str)
        {
            if (CommentTranslator22Package.Config.UseLevenshteinDistance)
            {
                foreach (var i in FileFormats[0].DataFormats)
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
                foreach (var i in FileFormats[0].DataFormats)
                {
                    if (Equals(i.SourceText, str)
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
