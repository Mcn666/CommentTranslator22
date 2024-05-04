using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.CompletionSource
{
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType("plaintext")]
    [Name("token completion")]
    internal class TestCompletionSourceProvider : ICompletionSourceProvider
    {
        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
        {
            return new TestCompletionSource(this, textBuffer);
        }
    }
}
