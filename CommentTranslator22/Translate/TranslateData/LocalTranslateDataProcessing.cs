using CommentTranslator22.Translate.Format;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CommentTranslator22.Translate.TranslateData
{
    internal class LocalTranslateDataProcessing
    {
        public static string RootPath { get; private set; } //保存在本地的数据的根路径
        public static string SolutionName { get; private set; }
        public static string SolutionPath { get; private set; }
        public static string SoltuionDataPath { get; private set; }
        public static string SolutionDataName { get; private set; }

        public static List<LocalSaveFormat> DataList { get; set; }

        public static void Load()
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            //var dte2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            //var solution = dte2.Solution;
            //SolutionName = Path.GetFileName(solution.FullName);     //解决方案名称
            //SolutionPath = Path.GetDirectoryName(solution.FullName);//解决方案路径
            RootPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/CommentTranslator22";
            SolutionPath = Directory.GetCurrentDirectory();
            SoltuionDataPath = RootPath + "/TranslateData";


            DataList = new List<LocalSaveFormat> { };

            AffirmLocalDataStruct();
            LoadSolutionInfo(RootPath + "/SolutionInfo");
        }

        public static void Unload()
        {
            if (DataList.Count > 0)
            {
                AffirmLocalDataStruct();
                SaveTranslateInfo(RootPath + "/SolutionInfo");
            }

            DataList = null;
        }

        private static void AffirmLocalDataStruct()
        {
            // 确认路径是否存在，如果没有就重建它
            if (Directory.Exists(RootPath) == false)
            {
                //Directory.CreateDirectory(RootPath);
                Directory.CreateDirectory(SoltuionDataPath);
            }

            // 确认文件是否存在，如果没有就创建它
            if (File.Exists(RootPath + "/SolutionInfo") == false) File.Create(RootPath + "/SolutionInfo").Close();

        }

        private static void LoadSolutionInfo(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var splits = line.Split('|');
                        if (Equals(splits[0], SolutionPath))
                        {
                            SolutionDataName = splits[1];
                            LoadSolutionData(SoltuionDataPath + "/" + SolutionDataName);
                            sr.Close();
                            return;
                        }
                    }
                    SolutionDataName = null;
                    sr.Close();
                }
            }
        }

        private static void LoadSolutionData(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var sr = new StreamReader(fs))
                {
                    LoadSolutionData(sr);
                    sr.Close();
                }
            }

        }

        private static void LoadSolutionData(StreamReader sr)
        {
            var str = sr.ReadToEnd();
            DataList = JsonConvert.DeserializeObject<List<LocalSaveFormat>>(str);
            //    var lineSplitResult = line.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries);
            //    LocalTranslateData.DataList.Add(new LocalSaveFormat
            //    {
            //        Server = (ServerEnum)System.Enum.Parse(typeof(ServerEnum), lineSplitResult[0]),
            //        FromLanguage = (LanguageEnum)System.Enum.Parse(typeof(LanguageEnum), lineSplitResult[1]),
            //        ToLanguage = (LanguageEnum)System.Enum.Parse(typeof(LanguageEnum), lineSplitResult[2]),
            //        ReadCount = int.Parse(lineSplitResult[3]),
            //        Body = lineSplitResult[4],
            //        Data = lineSplitResult[5]
            //    });
        }

        private static void SaveTranslateInfo(string path)
        {
            if (SolutionDataName == null)
            {
                SolutionDataName = DateTime.Now.ToFileTime().ToString();//使用时间生成的文件几乎不会重复

                using (var sw = new StreamWriter(path, true))
                {
                    sw.WriteLine(SolutionPath + "|" + SolutionDataName);
                    sw.Close();
                }
            }

            SaveTranslateData(SoltuionDataPath + "/" + SolutionDataName);
        }

        private static void SaveTranslateData(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    SaveTranslateData(sw);
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        private static void SaveTranslateData(StreamWriter sw)
        {
            DataList.Sort(delegate (LocalSaveFormat x, LocalSaveFormat y)
            {
                if (x.Visits > y.Visits)
                    return -1;
                else
                    return 1;
            });

            if (CommentTranslator22Package.ConfigA.NumberOfTranslationsSaved < DataList.Count)
            {
                var index = CommentTranslator22Package.ConfigA.NumberOfTranslationsSaved;
                var count = DataList.Count - index;
                DataList.RemoveRange(index, count);
            }

            var jsonStr = JsonConvert.SerializeObject(DataList, Formatting.Indented);
            sw.Write(jsonStr);
        }
    }
}
