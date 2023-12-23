using CommentTranslator22.Arithmetic;
using CommentTranslator22.Translate.Format;

namespace CommentTranslator22.Translate.TranslateData
{
    internal class LocalTranslateData : LocalTranslateDataProcessing
    {
        /// <summary>
        /// 寻找翻译结果，如果存在则返回翻译结果，否则为null
        /// </summary>
        public static string SeekTranslateResult(string text)
        {
            if (DataList == null) return null;

            if (CommentTranslator22Package.ConfigB.UseLevenshteinDistance)
            {
                foreach (var item in DataList)
                {
                    if (LevenshteinDistance.LevenshteinDistancePercent(item.Source, text) > 0.85f)
                    {
                        item.Visits++;
                        return item.Result;
                    }
                }
            }
            else
            {
                foreach (var item in DataList)
                {
                    if (Equals(item.Source, text))
                    {
                        item.Visits++;
                        return item.Result;
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

            AwaitTranslateList.Remove(recvFormat.SourceText);
            DataList.Add(new LocalSaveFormat
            {
                TranslationServer = CommentTranslator22Package.ConfigA.TranslationServer,
                SourceLanguage = CommentTranslator22Package.ConfigA.SourceLanguage,
                TargetLanguage = CommentTranslator22Package.ConfigA.TargetLanguage,
                Source = recvFormat.SourceText,
                Result = recvFormat.ResultText,
                Visits = 1,
            });
        }
    }
}
