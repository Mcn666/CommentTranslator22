using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.CompletionSource
{
    internal class TestCompletionSource : IAsyncCompletionSource
    {
        public TestCompletionSource()
        {
        }

        /// <summary>
        /// 初始化完成源。
        /// </summary>
        /// <param name="trigger">触发值</param>
        /// <param name="triggerLocation">触发位置</param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public CompletionStartData InitializeCompletion(CompletionTrigger trigger, SnapshotPoint triggerLocation, CancellationToken token)
        {
            var c = trigger.Character;
            if (char.IsNumber(c) || char.IsPunctuation(c) || c == '\n'
                || trigger.Reason == CompletionTriggerReason.Backspace
                || trigger.Reason == CompletionTriggerReason.Deletion)
            {
                return CompletionStartData.DoesNotParticipateInCompletion;
            }

            return CompletionStartData.ParticipatesInCompletionIfAny;
        }

        /// <summary>
        /// 获取完成项列表。
        /// </summary>
        /// <param name="session"></param>
        /// <param name="trigger"></param>
        /// <param name="triggerLocation"></param>
        /// <param name="applicableToSpan"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<CompletionContext> GetCompletionContextAsync(IAsyncCompletionSession session, CompletionTrigger trigger, SnapshotPoint triggerLocation, SnapshotSpan applicableToSpan, CancellationToken token)
        {
            return Task.FromResult<CompletionContext>(null);
        }

        /// <summary>
        /// 获取完成项的描述。
        /// </summary>
        /// <param name="session"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<object> GetDescriptionAsync(IAsyncCompletionSession session, CompletionItem item, CancellationToken token)
        {

            return Task.FromResult<object>(null);
        }
    }
}
