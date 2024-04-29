using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CommentTranslator22.Popups.SignatureHelp
{
    internal class TestSignatureHelpSource : ISignatureHelpSource
    {
        private ITextBuffer m_textBuffer;
        private bool disposedValue;

        public TestSignatureHelpSource(ITextBuffer textBuffer)
        {
            m_textBuffer = textBuffer;
        }

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            return;

            var contentType = m_textBuffer.ContentType.ToString();
            if (contentType == "C/C++")
            {
                var signatureList = new List<ISignature>();
                foreach (var item in signatures)
                {
                    var temp = new TestSignature(m_textBuffer, item);
                    signatureList.Add(temp);
                }
                signatures.Clear();
                foreach (var item in signatureList)
                {
                    signatures.Add(item);
                }
            }

        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
            return null;

            if (session.Signatures.Count > 0)
            {
                // 现在还不能匹配数据类型
                // 这种方法如果存在字符串类型的参数就不能正确的匹配
                foreach (var signature in session.Signatures)
                {
                    var strText = signature.ApplicableToSpan.GetText(m_textBuffer.CurrentSnapshot);
                    var strTextNum = Regex.Matches(strText, ",").Count;
                    var sigTextNum = Regex.Matches(signature.Content, ",").Count;
                    if (strTextNum == sigTextNum)
                    {
                        return signature;
                    }
                }
                return session.SelectedSignature;
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
