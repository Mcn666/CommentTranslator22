using EnvDTE;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
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
    [Export(typeof(IAsyncCompletionSourceProvider))]
    [Name("Chemical element dictionary completion provider")]
    [ContentType("text")]
    internal class TestCompletionSourceProvider : IAsyncCompletionSourceProvider
    {
        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        /// <summary>
        /// 创建一个继承 <see cref="IAsyncCompletionSource"/> 接口的实例。
        /// </summary>
        /// <param name="textView"></param>
        /// <returns></returns>
        public IAsyncCompletionSource GetOrCreate(ITextView textView)
        {
            return new TestCompletionSource();
        }
    }
}
