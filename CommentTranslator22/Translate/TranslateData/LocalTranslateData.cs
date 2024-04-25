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

            if (CommentTranslator22Package.Config.UseLevenshteinDistance)
            {
                foreach (var item in DataList)
                {
                    if (LevenshteinDistance.LevenshteinDistancePercent(item.Source, text) > 0.85f
                        && item.TargetLanguage == CommentTranslator22Package.Config.TargetLanguage)
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
                    if (Equals(item.Source, text)
                        && item.TargetLanguage == CommentTranslator22Package.Config.TargetLanguage)
                    {
                        item.Visits++;
                        return item.Result;
                    }
                }
            }

            return null;
        }

        public static void Add(in ApiRecvFormat recvFormat)
        {
            DataList.Add(new LocalSaveFormat
            {
                TranslationServer = CommentTranslator22Package.Config.TranslationServer,
                SourceLanguage = CommentTranslator22Package.Config.SourceLanguage,
                TargetLanguage = CommentTranslator22Package.Config.TargetLanguage,
                Source = recvFormat.SourceText,
                Result = recvFormat.ResultText,
                Visits = 1,
            });
        }
    }
}
