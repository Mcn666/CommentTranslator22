using CommentTranslator22.Popups.QuickInfo.Comment;
using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.QuickInfo
{
    [Export(typeof(IAsyncQuickInfoSourceProvider))]
    [Name(nameof(TestQuickInfoSourceProvider))]
    [ContentType("code")]
    internal class TestQuickInfoSourceProvider : IAsyncQuickInfoSourceProvider
    {
        public IAsyncQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer)
        {
            return new TestQuickInfoSource(textBuffer);
        }
    }

    internal class TestQuickInfoSource : IAsyncQuickInfoSource
    {
        private readonly ITextBuffer _subjectBuffer;
        private bool _disposed;

        public TestQuickInfoSource(ITextBuffer textBuffer)
        {
            _subjectBuffer = textBuffer;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        public async Task<QuickInfoItem> GetQuickInfoItemAsync(IAsyncQuickInfoSession session, CancellationToken token)
        {
            try
            {
                var triggerPoint = session.GetTriggerPoint(_subjectBuffer.CurrentSnapshot);
                if (!triggerPoint.HasValue)
                {
                    return null;
                }

                var snapshotPoint = triggerPoint.Value;
                var applicableToSpan = CreateTrackingSpan(snapshotPoint);
                var containerElement = await BuildQuickInfoContentAsync(session, snapshotPoint);

                return new QuickInfoItem(applicableToSpan, containerElement);
            }
            catch (Exception)
            {
                // Log the exception if needed
                // Logger.Log(ex);
                return null;
            }
        }

        private ITrackingSpan CreateTrackingSpan(SnapshotPoint snapshotPoint)
        {
            var querySpan = new SnapshotSpan(snapshotPoint, 0);
            return snapshotPoint.Snapshot.CreateTrackingSpan(querySpan, SpanTrackingMode.EdgeInclusive);
        }

        private async Task<ContainerElement> BuildQuickInfoContentAsync(IAsyncQuickInfoSession session, SnapshotPoint snapshotPoint)
        {
            var tasks = new List<Task<ContainerElement>>();
            var timeout = TimeSpan.FromSeconds(1); // Timeout for each task

            if (CommentTranslator22Package.Config.UseDefaultTranslation)
            {
                tasks.Add(TaskExecutor.RunWithTimeoutAsync(() => GetMethodInformationAsync(session), timeout));
                tasks.Add(TaskExecutor.RunWithTimeoutAsync(() => GetGeneralCommentAsync(snapshotPoint), timeout));
            }

            if (CommentTranslator22Package.Config.UsePhraseTranslation)
            {
                tasks.Add(TaskExecutor.RunWithTimeoutAsync(() => GetPhraseTranslationResultAsync(snapshotPoint), timeout));
            }

            if (CommentTranslator22Package.Config.UseDictionaryTranslation)
            {
                tasks.Add(TaskExecutor.RunWithTimeoutAsync(() => GetDictionaryInformationAsync(snapshotPoint), timeout));
            }

            var results = await Task.WhenAll(tasks);

            // Combine all non-null results into a single ContainerElement
            var elements = results.Where(r => r != null).SelectMany(r => r.Elements);
            return new ContainerElement(ContainerElementStyle.Stacked, elements);
        }

        private async Task<ContainerElement> GetMethodInformationAsync(IAsyncQuickInfoSession session)
        {
            var methodInfo = await CommentTranslate.TryTranslateMethodInformationAsync(session, _subjectBuffer.ContentType.TypeName);
            return methodInfo != null && methodInfo.Any()
                ? CreateContainerElement(methodInfo)
                : null;
        }

        private async Task<ContainerElement> GetGeneralCommentAsync(SnapshotPoint snapshotPoint)
        {
            var commentInfo = await CommentTranslate.TranslateAsync(snapshotPoint);
            return commentInfo != null && commentInfo.Any()
                ? CreateContainerElement(commentInfo)
                : null;
        }

        private async Task<ContainerElement> GetPhraseTranslationResultAsync(SnapshotPoint snapshotPoint)
        {
            var word = ExtractWord(snapshotPoint.Snapshot.GetText(), snapshotPoint.Position);
            if (word == null) return null;

            var translationResult = await CommentTranslate.TranslateWordsAsync(word);
            return translationResult != null && translationResult.Any()
                ? CreateContainerElement(translationResult)
                : null;
        }

        private async Task<ContainerElement> GetDictionaryInformationAsync(SnapshotPoint snapshotPoint)
        {
            var word = ExtractWord(snapshotPoint.Snapshot.GetText(), snapshotPoint.Position);
            if (word == null) return null;

            var dictionaryInfo = await CommentTranslate.FetchDictionaryEntriesAsync(word);
            return dictionaryInfo != null && dictionaryInfo.Any()
                ? CreateContainerElement(dictionaryInfo)
                : null;
        }

        private ContainerElement CreateContainerElement(IEnumerable<ClassifiedTextRun> textRuns)
        {
            return new ContainerElement(ContainerElementStyle.Stacked, new ClassifiedTextElement(textRuns));
        }

        private string ExtractWord(string text, int position)
        {
            if (position < 0 || position >= text.Length)
            {
                return null;
            }

            int start = position;
            int end = position;

            while (start >= 0 && char.IsLetter(text[start]))
            {
                start--;
            }

            while (end < text.Length && char.IsLetter(text[end]))
            {
                end++;
            }

            return start < end ? text.Substring(start + 1, end - start - 1) : null;
        }
    }
}