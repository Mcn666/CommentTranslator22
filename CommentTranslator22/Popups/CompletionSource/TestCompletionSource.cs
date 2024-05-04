using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.CompletionSource
{
    internal class TestCompletionSource : ICompletionSource
    {
        public TestCompletionSource(TestCompletionSourceProvider provider, ITextBuffer textBuffer)
        {

        }

        public void AugmentCompletionSession(ICompletionSession session, IList<CompletionSet> completionSets)
        {

        }

        public void Dispose()
        {

        }
    }
}
