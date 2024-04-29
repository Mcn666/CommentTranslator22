using CommentTranslator22.Popups.QuickInfo.Comment;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.QuickInfo
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

            var querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);
            var currentSnapshot = subjectTriggerPoint.Value.Snapshot;
            var applicableToSpan = currentSnapshot.CreateTrackingSpan(querySpan, SpanTrackingMode.EdgeInclusive);

            // 检查光标所指向的行
            var navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_subjectBuffer);
            var span = navigator.GetExtentOfWord(subjectTriggerPoint.Value).Span;

            if (CommentTranslator22Package.Config.TranslateQuickInfoCommentText)
            {
                var temp = await CommentTranslate.TryTranslateMethodInformationAsync(session);
                if (temp != null && temp.Any() == true)
                {
                    var time = (DateTime.UtcNow - beginTime).TotalSeconds.ToString();
                    var element = new ContainerElement(ContainerElementStyle.Stacked,
                        new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Comment, time)),
                        new ClassifiedTextElement(temp));
                    return new QuickInfoItem(applicableToSpan, element);
                }
            }

            if (CommentTranslator22Package.Config.TranslateGeneralCommentText)
            {
                var temp = await CommentTranslate.TranslateAsync(span);
                if (temp != null && temp.Any() == true)
                {
                    var time = (DateTime.UtcNow - beginTime).TotalSeconds.ToString();
                    var element = new ContainerElement(ContainerElementStyle.Stacked,
                        new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Comment, time)),
                        new ClassifiedTextElement(temp));
                    return new QuickInfoItem(applicableToSpan, element);
                }
            }

            if (CommentTranslator22Package.Config.UseDictionary)
            {
                var temp = CommentTranslate.QueryDictionary(span.GetText().Trim());
                if (temp != null)
                {
                    var time = (DateTime.UtcNow - beginTime).TotalSeconds.ToString();
                    var element = new ContainerElement(ContainerElementStyle.Stacked,
                        new ClassifiedTextElement(
                            new ClassifiedTextRun(PredefinedClassificationTypeNames.Comment, time)),
                        new ClassifiedTextElement(temp));
                    return new QuickInfoItem(applicableToSpan, element);
                }
            }
            return null;
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
