using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;

namespace CommentTranslator22.Popup.StatementCompletion
{
    internal class TestSignatureHelpSource : ISignatureHelpSource
    {
        private ITextBuffer m_textBuffer;
        private bool disposedValue;
        private List<ISignature> SignaturesList = new List<ISignature>();

        public TestSignatureHelpSource(ITextBuffer textBuffer)
        {
            m_textBuffer = textBuffer;
        }

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            //if (CommentTranslator22Package.ConfigB.UseCoverCodeCompletionPrompt == false) return;
            return;

            foreach (var item in signatures)
            {
                SignaturesList.Add(item);
            }
        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
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
        // ~TestSignatureHelpSource()
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
