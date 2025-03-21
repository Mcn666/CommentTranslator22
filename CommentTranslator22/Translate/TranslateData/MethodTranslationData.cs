using CommentTranslator22.Translate.Format;

namespace CommentTranslator22.Translate.TranslateData
{
    internal class MethodTranslationData : TranslationData
    {
        internal static MethodTranslationData Instance => Nested.instance;

        class Nested
        {
            internal static MethodTranslationData instance = new MethodTranslationData();
            static Nested() { }
        }

        internal MethodTranslationData() : base()
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
            base.LoadTranslationData();
        }

        protected override void SaveData()
        {
            base.SaveTranslationData();
        }
    }
}
