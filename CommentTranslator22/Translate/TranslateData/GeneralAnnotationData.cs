using CommentTranslator22.Translate.Format;
using Microsoft.VisualStudio.LocalLogger;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CommentTranslator22.Translate.TranslateData
{
    internal class GeneralAnnotationData
    {
        public static GeneralAnnotationData Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            internal static GeneralAnnotationData instance = new GeneralAnnotationData();

            static Nested() { }
        }

        class GeneralAnnotationDataFormat
        {
            public int VisitsCount;
            public string Server;
            public string SourceLanguage;
            public string TargetLanguage;
            public string SourceText;
            public string TargetText;
            public LanguageEnum LanguageEnumCode;
        }

        class GeneralAnnotationDataFileFormat
        {
            public string MainPath;
            public string InfoFileName;
            public string SolutionName;
            public string SolutionPath;
            public string SolutionDataName;
            public int MaximumStorageCount = 1000;
            public List<GeneralAnnotationDataFormat> DataFormats;
        }

        GeneralAnnotationDataFileFormat FileFormat { get; set; } = new GeneralAnnotationDataFileFormat();

        GeneralAnnotationData()
        {
            FileFormat.MainPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            FileFormat.MainPath += "/CommentTranslator22/GeneralAnnotationData";
            FileFormat.InfoFileName = $"{FileFormat.MainPath}/SolutionInfo.txt";
        }

        public void ReadAllData()
        {
            var dte2 = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
            var solution = dte2.Solution;
            FileFormat.SolutionName = Path.GetFileName(solution.FullName);     //解决方案名称
            FileFormat.SolutionPath = Path.GetDirectoryName(solution.FullName);//解决方案路径
            //FileFormat.SolutionPath = Directory.GetCurrentDirectory();

            AffirmLocalFolderExists();
            AffirmLocalFileExists();
            ReadData(FileFormat.InfoFileName);
        }

        void ReadData(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var lines = sr.ReadToEnd().Replace("\r\n", "\n").Split('\n');
                    foreach (var line in lines)
                    {
                        var temp = line.Split('|');
                        if (temp.Count() == 2 && Equals(temp[1], FileFormat.SolutionPath))
                        {
                            FileFormat.SolutionDataName = temp[0];
                            ReadSolutionData($"{FileFormat.MainPath}/{temp[0]}");
                            return;
                        }
                    }
                    FileFormat.SolutionDataName = null;
                    FileFormat.DataFormats = new List<GeneralAnnotationDataFormat>();
                }
            }
        }

        void ReadSolutionData(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    var str = sr.ReadToEnd();
                    FileFormat.DataFormats = JsonConvert.DeserializeObject<List<GeneralAnnotationDataFormat>>(str);
                }
            }
        }

        public void SaveAllData()
        {
            if (FileFormat.SolutionPath == null || FileFormat.SolutionPath.Length < 10 ||
                FileFormat.DataFormats == null || FileFormat.DataFormats.Count == 0)
            {
                return;
            }

            if (FileFormat.SolutionDataName == null)
            {
                // 使用时间生成的文件几乎不会重复
                FileFormat.SolutionDataName = DateTime.Now.ToFileTime().ToString() + ".txt";
                using (var sw = new StreamWriter(FileFormat.InfoFileName, true))
                {
                    sw.WriteLine(FileFormat.SolutionDataName + "|" + FileFormat.SolutionPath);
                }
            }

            Sort(ref FileFormat.DataFormats);
            Save($"{FileFormat.MainPath}/{FileFormat.SolutionDataName}");
        }

        void Save(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (var sw = new StreamWriter(fs))
                {
                    if (FileFormat.MaximumStorageCount < FileFormat.DataFormats.Count)
                    {
                        var index = FileFormat.MaximumStorageCount;
                        var count = FileFormat.DataFormats.Count - index;
                        FileFormat.DataFormats.RemoveRange(index, count);
                    }

                    var json = JsonConvert.SerializeObject(FileFormat.DataFormats, Formatting.Indented);
                    sw.WriteLine(json);
                }
            }
        }

        void Sort(ref List<GeneralAnnotationDataFormat> formats)
        {
            formats.Sort(delegate (GeneralAnnotationDataFormat a, GeneralAnnotationDataFormat b)
            {
                return a.VisitsCount > b.VisitsCount ? -1 : 1;
            });
        }

        void AffirmLocalFolderExists()
        {
            if (Directory.Exists(FileFormat.MainPath) == false)
            {
                Directory.CreateDirectory(FileFormat.MainPath);
            }
        }

        void AffirmLocalFileExists()
        {
            if (File.Exists(FileFormat.InfoFileName) == false)
            {
                File.Create(FileFormat.InfoFileName).Close();
            }
        }

        /// <summary>
        /// 添加结果到列表
        /// </summary>
        /// <param name="format"></param>
        public void Add(ApiRecvFormat format)
        {
            if (format.IsSuccess == false)
            {
                return;
            }

            var temp = new GeneralAnnotationDataFormat
            {
                Server = CommentTranslator22Package.Config.TranslationServer.ToString(),
                SourceLanguage = CommentTranslator22Package.Config.SourceLanguage.ToString(),
                TargetLanguage = CommentTranslator22Package.Config.TargetLanguage.ToString(),
                SourceText = format.SourceText,
                TargetText = format.TargetText,
                VisitsCount = 1,
                LanguageEnumCode = CommentTranslator22Package.Config.TargetLanguage,
            };

            if (FileFormat.DataFormats.Any(f => f.SourceText == temp.SourceText) == false)
            {
                FileFormat.DataFormats.Add(temp);
            }
        }

        /// <summary>
        /// 寻找翻译结果，如果存在则返回翻译结果，否则为null
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string IndexOf(string text)
        {
            if (CommentTranslator22Package.Config.UseLevenshteinDistance)
            {
                foreach (var i in FileFormat.DataFormats)
                {
                    if (LevenshteinDistance.LevenshteinDistancePercent(i.SourceText, text) > 0.85f
                        && i.LanguageEnumCode == CommentTranslator22Package.Config.TargetLanguage)
                    {
                        i.VisitsCount++;
                        return i.TargetText;
                    }
                }
            }
            else
            {
                foreach (var i in FileFormat.DataFormats)
                {
                    if (Equals(i.SourceText, text)
                        && i.LanguageEnumCode == CommentTranslator22Package.Config.TargetLanguage)
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
