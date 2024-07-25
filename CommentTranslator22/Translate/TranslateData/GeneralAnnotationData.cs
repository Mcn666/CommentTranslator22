using CommentTranslator22.Popups;
using CommentTranslator22.Translate.Format;
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
            public bool IsInitializationComplete = false;
            public List<GeneralAnnotationDataFormat> DataFormats;
        }

        GeneralAnnotationDataFileFormat Format { get; set; } = new GeneralAnnotationDataFileFormat();

        GeneralAnnotationData()
        {
            Format.MainPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            Format.MainPath += "/CommentTranslator22/GeneralAnnotationData";
            Format.InfoFileName = $"{Format.MainPath}/SolutionInfo.txt";
            TestSolutionEvents.Instance.SolutionCloseFunc.Add(SaveAllData);
        }

        void ReadAllData()
        {
            _ = ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var dte2 = Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2;
                var solution = dte2.Solution;
                Format.SolutionName = Path.GetFileName(solution.FullName);     //解决方案名称
                Format.SolutionPath = Path.GetDirectoryName(solution.FullName);//解决方案路径
                AffirmLocalFolderExists();
                AffirmLocalFileExists();
                ReadData(Format.InfoFileName);
                Format.IsInitializationComplete = Format.DataFormats != null;
            });
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
                        if (temp.Count() == 2 && Equals(temp[1], Format.SolutionPath))
                        {
                            Format.SolutionDataName = temp[0];
                            ReadSolutionData($"{Format.MainPath}/{temp[0]}");
                            return;
                        }
                    }
                    Format.SolutionDataName = null;
                    Format.DataFormats = new List<GeneralAnnotationDataFormat>();
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
                    Format.DataFormats = JsonConvert.DeserializeObject<List<GeneralAnnotationDataFormat>>(str);
                }
            }
        }

        void SaveAllData()
        {
            if (Format.IsInitializationComplete == false)
            {
                return;
            }

            if (Format.SolutionPath == null
                || Format.SolutionPath.Length < 10
                || Format.DataFormats == null
                || Format.DataFormats.Count == 0)
            {
                return;
            }

            if (Format.SolutionDataName == null)
            {
                // 使用时间生成的文件几乎不会重复
                Format.SolutionDataName = DateTime.Now.ToFileTime().ToString() + ".txt";
                using (var sw = new StreamWriter(Format.InfoFileName, true))
                {
                    sw.WriteLine(Format.SolutionDataName + "|" + Format.SolutionPath);
                }
            }

            Sort(ref Format.DataFormats);
            Save($"{Format.MainPath}/{Format.SolutionDataName}");

            Format.IsInitializationComplete = false;
        }

        void Save(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (var sw = new StreamWriter(fs))
                {
                    if (Format.MaximumStorageCount < Format.DataFormats.Count)
                    {
                        var index = Format.MaximumStorageCount;
                        var count = Format.DataFormats.Count - index;
                        Format.DataFormats.RemoveRange(index, count);
                    }

                    var json = JsonConvert.SerializeObject(Format.DataFormats, Formatting.Indented);
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
            if (Directory.Exists(Format.MainPath) == false)
            {
                Directory.CreateDirectory(Format.MainPath);
            }
        }

        void AffirmLocalFileExists()
        {
            if (File.Exists(Format.InfoFileName) == false)
            {
                File.Create(Format.InfoFileName).Close();
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

            if (Format.DataFormats.Any(f => f.SourceText == temp.SourceText) == false)
            {
                Format.DataFormats.Add(temp);
            }
        }

        /// <summary>
        /// 寻找翻译结果，如果存在则返回翻译结果，否则为null
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string IndexOf(string text)
        {
            if (Format.IsInitializationComplete == false)
            {
                ReadAllData();
            }
            if (Format.IsInitializationComplete == false)
            {
                return null;
            }


            if (CommentTranslator22Package.Config.UseLevenshteinDistance)
            {
                foreach (var i in Format.DataFormats)
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
                foreach (var i in Format.DataFormats)
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
