using CommentTranslator22.Comment;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.CursorPosition
{
    internal class TestQuickInfoSource : IAsyncQuickInfoSource
    {
        private TestQuickInfoSourceProvider m_provider;
        private ITextBuffer m_subjectBuffer;
        private bool disposedValue;

        public TestQuickInfoSource(TestQuickInfoSourceProvider quickInfoSourceProvider, ITextBuffer textBuffer)
        {
            this.m_provider = quickInfoSourceProvider;
            this.m_subjectBuffer = textBuffer;
        }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken cancellationToken)
        {
            var beginTime = DateTime.UtcNow;
            var subjectTriggerPoint = session.GetTriggerPoint(m_subjectBuffer.CurrentSnapshot);
            if (subjectTriggerPoint.HasValue == false)
            {
                return null;
            }

            var currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            var querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);
            var applicableToSpan = currentSnapshot.CreateTrackingSpan(querySpan, SpanTrackingMode.EdgeInclusive);

            // 检查光标所指向的行
            var navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_subjectBuffer);
            var extent = navigator.GetExtentOfWord(subjectTriggerPoint.Value);

            // 最终显示的信息
            var scres = await CommentTranslate.TranslateAsync(extent.Span);
            if (scres.Any() == true)
            {
                var tempTime = (DateTime.UtcNow - beginTime).TotalSeconds.ToString();
                var tempElement = new ContainerElement(ContainerElementStyle.Stacked,
                    new ClassifiedTextElement(
                        new ClassifiedTextRun(PredefinedClassificationTypeNames.Comment, tempTime)),
                    new ClassifiedTextElement(scres));
                return new QuickInfoItem(applicableToSpan, tempElement);
            }

            // 执行到这里，只能获取光标所指向的文本进行字典查询了
            var dres = CommentTranslate.QueryDictionary(extent.Span.GetText().Trim());
            if (dres == string.Empty)
            {
                return null;
            }

            // 计算这次使用的时间
            var deltaTime = DateTime.UtcNow - beginTime;
            var str = $"{deltaTime.TotalSeconds}\n{dres}";

            var element = new ContainerElement(ContainerElementStyle.Stacked,
                new ClassifiedTextElement(
                    new ClassifiedTextRun(PredefinedClassificationTypeNames.Comment, str)));

            return new QuickInfoItem(applicableToSpan, element);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~TestQuickInfoSource()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
