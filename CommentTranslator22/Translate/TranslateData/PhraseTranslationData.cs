// PhraseTranslationData.cs
namespace CommentTranslator22.Translate.TranslateData
{
    internal class PhraseTranslationData : BaseTranslationData
    {
        internal static PhraseTranslationData Instance => Nested.instance;

        private class Nested
        {
            internal static readonly PhraseTranslationData instance = CreateInstance<PhraseTranslationData>();
            static Nested() { }
        }

        // 短语翻译数据可以选择性地从旧数据中迁移
        protected override bool ShouldMigrateOldData()
        {
            // 这里可以添加特定逻辑来筛选只与短语相关的数据
            // 目前返回false，不从旧数据迁移
            return false;
        }
    }
}