using CommentTranslator22.Translate.Format;

namespace CommentTranslator22.Translate.TranslateData
{
    internal class PhraseTranslationData : TranslationData
    {
        internal static PhraseTranslationData Instance => Nested.instance;

        class Nested
        {
            internal static PhraseTranslationData instance = new PhraseTranslationData();
            static Nested() { }
        }

        internal PhraseTranslationData() : base()
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

        protected override void LoadData()
        {
            base.LoadData(nameof(PhraseTranslationData));
        }

        protected override void SaveData()
        {
            base.SaveData(nameof(PhraseTranslationData));
        }
    }
}
