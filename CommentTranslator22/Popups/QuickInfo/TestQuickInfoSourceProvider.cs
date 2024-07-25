using CommentTranslator22.Popups.QuickInfo.Comment;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.QuickInfo
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name(nameof(TestQuickInfoSourceProvider))]
    [ContentType("text")]
    internal class TestQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new TestQuickInfoSource(textBuffer);
        }
    }

    internal class TestQuickInfoSource : IAsyncQuickInfoSource
    {
        private ITextBuffer m_subjectBuffer;
        private bool disposedValue;

        public TestQuickInfoSource(ITextBuffer textBuffer)
        {
            this.m_subjectBuffer = textBuffer;
        }

        public void Dispose()
        {
            if (!disposedValue)
            {
                GC.SuppressFinalize(this); // 告诉GC不用调用终结器来释放对象
                disposedValue = true;
            }
        }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken token)
        {
            try
            {
                var typeName = m_subjectBuffer.ContentType.TypeName;
                var subjectTriggerPoint = session.GetTriggerPoint(m_subjectBuffer.CurrentSnapshot);
                if (subjectTriggerPoint.HasValue == false)
                {
                    return null;
                }

                var querySpan = new SnapshotSpan(subjectTriggerPoint.Value, 0);
                var currentSnapshot = subjectTriggerPoint.Value.Snapshot;
                var applicableToSpan = currentSnapshot.CreateTrackingSpan(querySpan, SpanTrackingMode.EdgeInclusive);
                var snapshotPoint = subjectTriggerPoint.Value;

                // 获取给定位置的单词
                // 使用 navigator.GetExtentOfWord 获取 XML、XAML 中的字符时会导致 VS 卡死
                //var navigator = m_provider.NavigatorService.GetTextStructureNavigator(m_subjectBuffer);
                //var span = navigator.GetExtentOfWord(subjectTriggerPoint.Value).Span;
                var word = GetWord(snapshotPoint.Snapshot.GetText(), snapshotPoint.Position);

                var element = new ContainerElement(ContainerElementStyle.Stacked);

                if (CommentTranslator22Package.Config.TranslateQuickInfoCommentText)
                {
                    var temp = await CommentTranslate.TryTranslateMethodInformationAsync(session, typeName);
                    if (temp != null && temp.Any() == true)
                    {
                        var e = element.Elements.Append(new ClassifiedTextElement(temp));
                        element = new ContainerElement(ContainerElementStyle.Stacked, e);
                    }
                }

                if (CommentTranslator22Package.Config.TranslateGeneralCommentText)
                {
                    var temp = await CommentTranslate.TranslateAsync(snapshotPoint);
                    if (temp != null && temp.Any() == true)
                    {
                        var e = element.Elements.Append(new ClassifiedTextElement(temp));
                        element = new ContainerElement(ContainerElementStyle.Stacked, e);
                    }
                }

                if (CommentTranslator22Package.Config.UseDictionary && word != null)
                {
                    var temp = CommentTranslate.QueryDictionary(word);
                    if (temp != null)
                    {
                        var e = element.Elements.Append(new ClassifiedTextElement(temp));
                        element = new ContainerElement(ContainerElementStyle.Stacked, e);
                    }
                }
                return new QuickInfoItem(applicableToSpan, element);
            }
            catch
            {
                return null;
            }
        }

        public string GetWord(string text, int position)
        {
            if (position < 0 || position >= text.Length)
            {
                return null;
            }

            // 获取单词边界
            int strat = position, end = position;
            while (strat >= 0)
            {
                if (char.IsWhiteSpace(text[strat]) || char.IsPunctuation(text[strat]) || char.IsSymbol(text[strat]))
                {
                    break;
                }
                strat--;
            }
            while (end < text.Length)
            {
                if (char.IsWhiteSpace(text[end]) || char.IsPunctuation(text[end]) || char.IsSymbol(text[end]))
                {
                    break;
                }
                end++;
            }

            if ((end > strat) == false)
            {
                return null;
            }

            // 返回单词
            return text.Substring(strat + 1, end - strat - 1);
        }
    }
}
