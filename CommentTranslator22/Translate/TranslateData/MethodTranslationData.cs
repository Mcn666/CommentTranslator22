// MethodTranslationData.cs
namespace CommentTranslator22.Translate.TranslateData
{
    internal class MethodTranslationData : BaseTranslationData
    {
        internal static MethodTranslationData Instance => Nested.instance;

        private class Nested
        {
            internal static readonly MethodTranslationData instance = CreateInstance<MethodTranslationData>();
            static Nested() { }
        }

        // 方法翻译数据继承所有旧数据
        protected override bool ShouldMigrateOldData() => true;
    }
}