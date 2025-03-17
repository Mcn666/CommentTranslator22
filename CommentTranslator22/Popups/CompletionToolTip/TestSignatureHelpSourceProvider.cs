using CommentTranslator22.Popups.CompletionToolTip.View;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace CommentTranslator22.Popups.CompletionToolTip
{
    [Export(typeof(ISignatureHelpSourceProvider))]
    [Name(nameof(TestSignatureHelpSourceProvider))]
    [ContentType("CSharp")]
    internal class TestSignatureHelpSourceProvider : ISignatureHelpSourceProvider
    {
        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer)
        {
            return new TestSignatureHelpSource(textBuffer);
        }
    }

    internal class TestSignatureHelpSource : ISignatureHelpSource
    {
        private readonly ITextBuffer textBuffer;

        public TestSignatureHelpSource(ITextBuffer textBuffer)
        {
            this.textBuffer = textBuffer;
        }

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            // 这个方法被调用时，更改完成提示工具的位置，避免被签名提示工具遮挡
            session.Dismissed += Session_Dismissed;
            var point = session.GetTriggerPoint(textBuffer).GetPoint(textBuffer.CurrentSnapshot);
            var span = session.TextView.GetTextElementSpan(point);
            var view = session.TextView;
            TestAdornmentLayer.AdjustViewPosition<CompletionView>(view, span);
        }

        private void Session_Dismissed(object sender, System.EventArgs e)
        {
            // 这个事件触发时，停止更改完成提示工具的位置
            if (sender is ISignatureHelpSession session)
            {
                var view = session.TextView;
                TestAdornmentLayer.AdjustViewPositionEnd<CompletionView>(view);
            }
        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
            return null;
        }

        public void Dispose()
        {

        }
    }
}
