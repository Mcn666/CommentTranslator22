using CommentTranslator22.Arithmetic;
using CommentTranslator22.Translate.Enum;
using CommentTranslator22.Translate.Format;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CommentTranslator22.Translate
{
    internal class LocalTranslateData
    {
        public static string RootPath { get; private set; } //保存在本地的数据的根路径
        public static string SolutionName { get; private set; }
        public static string SolutionPath { get; private set; }
        public static string SoltuionDataPath { get; private set; }
        public static string SolutionDataName { get; private set; }

        private static List<LocalSaveFormat> LocalData { get; set; }
        private static List<string> AwaitTranslateList { get; set; }


        /// <summary>
        /// 寻找翻译结果，如果存在则返回翻译结果，否则为null
        /// </summary>
        public static string SeekTranslateResult(string text)
        {
            if (LocalData == null) return null;

            if (CommentTranslator22Package.ConfigB.UseLevenshteinDistance)
            {
                foreach (var item in LocalData)
                {
                    if (LevenshteinDistance.LevenshteinDistancePercent(item.Body, text) > 0.85f)
                    {
                        item.ReadCount++;
                        return item.Data;
                    }
                }
            }
            else
            {
                foreach (var item in LocalData)
                {
                    if (Equals(item.Body, text))
                    {
                        item.ReadCount++;
                        return item.Data;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 寻找等待翻译的文本，如果存在返回true，否则返回false
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool SeekAwaitTranslateText(string text)
        {
            if (AwaitTranslateList == null) return true;

            if (CommentTranslator22Package.ConfigB.UseLevenshteinDistance)
            {
                foreach (var item in AwaitTranslateList)
                {
                    if (LevenshteinDistance.LevenshteinDistancePercent(item, text) > 0.85f)
                    {
                        return true;
                    }
                }
            }
            else
            {
                foreach (var item in AwaitTranslateList)
                {
                    if (Equals(item, text))
                    {
                        return true;
                    }
                }
            }

            AwaitTranslateList.Add(text);
            return false;
        }

        public static void Add(in ApiRecvFormat recvFormat)
        {
            if (recvFormat.Code == -1) return;

            AwaitTranslateList.Remove(recvFormat.Body);
            LocalData.Add(new LocalSaveFormat
            {
                Server = CommentTranslator22Package.ConfigA.Servers,
                FromLanguage = CommentTranslator22Package.ConfigA.LanguageFrom,
                ToLanguage = CommentTranslator22Package.ConfigA.LanguageTo,
                Body = recvFormat.Body,
                Data = recvFormat.Data,
                ReadCount = 1,
            });
        }

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
            

            LocalData = new List<LocalSaveFormat> { };
            AwaitTranslateList = new List<string> { };

            AffirmLocalDataStruct();
            LoadSolutionInfo(RootPath + "/SolutionInfo");
        }

        public static void Unload()
        {
            if (LocalData.Count > 0)
            {
                AffirmLocalDataStruct();
                SaveTranslateInfo(RootPath + "/SolutionInfo");
            }


            LocalData = null;
            AwaitTranslateList = null;
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
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                var lineSplitResult = line.Split(new string[] { "@@@" }, StringSplitOptions.RemoveEmptyEntries);
                LocalData.Add(new LocalSaveFormat
                {
                    Server = (ServerEnum)System.Enum.Parse(typeof(ServerEnum), lineSplitResult[0]),
                    FromLanguage = (LanguageEnum)System.Enum.Parse(typeof(LanguageEnum), lineSplitResult[1]),
                    ToLanguage = (LanguageEnum)System.Enum.Parse(typeof(LanguageEnum), lineSplitResult[2]),
                    ReadCount = int.Parse(lineSplitResult[3]),
                    Body = lineSplitResult[4],
                    Data = lineSplitResult[5]
                });
            }
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
            LocalData.Sort(delegate (LocalSaveFormat x, LocalSaveFormat y)
            {
                if (x.ReadCount > y.ReadCount)
                    return -1;
                else
                    return 1;
            });

            var temp = "@@@";
            var max = CommentTranslator22Package.ConfigA.TranslateResultMaximumSave;
            var count = LocalData.Count > max ? max : LocalData.Count;
            for (var i = 0; i < count; i++)
            {
                var res = LocalData[i].Server +
                    temp + LocalData[i].FromLanguage +
                    temp + LocalData[i].ToLanguage +
                    temp + LocalData[i].ReadCount +
                    temp + LocalData[i].Body +
                    temp + LocalData[i].Data;
                sw.WriteLine(res);
            }

            //var jsonStr = JsonConvert.SerializeObject(formats, Formatting.Indented);
        }
    }
}
