// BaseTranslationData.cs
using CommentTranslator22.Translate.Format;

namespace CommentTranslator22.Translate.TranslateData
{
    internal abstract class BaseTranslationData : TranslationData
    {
        protected static T CreateInstance<T>() where T : BaseTranslationData, new()
        {
            return new T();
        }

        internal ApiRecvFormat GetTranslationResult(string key)
        {
            var entry = GetTranslationEntry(key);
            return entry == null ? null : new ApiRecvFormat()
            {
                SourceText = key,
                TargetText = entry.TargetText
            };
        }

        protected override void SaveData()
        {
            base.SaveTranslationData();
        }

        protected override void LoadData()
        {
            base.LoadTranslationData();
        }
    }
}