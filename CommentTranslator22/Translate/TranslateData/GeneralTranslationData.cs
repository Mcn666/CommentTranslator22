// GeneralTranslationData.cs
namespace CommentTranslator22.Translate.TranslateData
{
    internal class GeneralTranslationData : BaseTranslationData
    {
        internal static GeneralTranslationData Instance => Nested.instance;

        private class Nested
        {
            internal static readonly GeneralTranslationData instance = CreateInstance<GeneralTranslationData>();
            static Nested() { }
        }

        // 通用翻译数据继承所有旧数据
        protected override bool ShouldMigrateOldData() => true;
    }
}