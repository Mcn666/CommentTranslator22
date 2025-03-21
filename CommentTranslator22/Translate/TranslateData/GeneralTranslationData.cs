using CommentTranslator22.Translate.Format;

namespace CommentTranslator22.Translate.TranslateData
{
    internal class GeneralTranslationData : TranslationData
    {
        internal static GeneralTranslationData Instance => Nested.instance;

        class Nested
        {
            internal static GeneralTranslationData instance = new GeneralTranslationData();
            static Nested() { }
        }

        internal GeneralTranslationData() : base()
        {
        }

        internal ApiRecvFormat GetTranslationResult(string key)
        {
            var entry = GetTranslationEntry(key);
            if (entry != null)
            {
                return new ApiRecvFormat()
                {
                    SourceText = key,
                    TargetText = entry.TargetText
                };
            }
            return null;
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
