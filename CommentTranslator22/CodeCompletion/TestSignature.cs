using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.ObjectModel;

namespace CommentTranslator22.CodeCompletion
{
    internal class TestSignature : ISignature
    {
        private ITrackingSpan m_applicableToSpan;
        private string m_content;
        private string m_prettyPrintedContent;
        private string m_documentation;
        private ReadOnlyCollection<IParameter> m_parameters;
        private IParameter m_currentParameter;
        private ITextBuffer m_subjectBuffer;

        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;

        internal TestSignature(ITextBuffer subjectBuffer, ISignature signature)
        {
            this.m_applicableToSpan = signature.ApplicableToSpan;
            this.m_content = signature.Content;
            this.m_prettyPrintedContent = signature.PrettyPrintedContent;
            this.m_documentation = (signature.Documentation == null) ? "" : $"{signature.Documentation}\nTest";
            this.m_parameters = signature.Parameters;
            this.m_currentParameter = signature.CurrentParameter;

            this.m_subjectBuffer = subjectBuffer;
            this.m_subjectBuffer.Changed += new EventHandler<TextContentChangedEventArgs>(OnSubjectBufferChanged);
        }

        public IParameter CurrentParameter
        {
            get { return m_currentParameter; }
            internal set
            {
                if (m_currentParameter != value)
                {
                    IParameter prevCurrentParameter = m_currentParameter;
                    m_currentParameter = value;
                    this.RaiseCurrentParameterChanged(prevCurrentParameter, m_currentParameter);
                }
            }
        }

        private void RaiseCurrentParameterChanged(IParameter prevCurrentParameter, IParameter newCurrentParameter)
        {
            this.CurrentParameterChanged?.Invoke(this,
                new CurrentParameterChangedEventArgs(prevCurrentParameter, newCurrentParameter));
        }

        internal void ComputeCurrentParameter()
        {
            if (Parameters.Count == 0)
            {
                this.CurrentParameter = null;
                return;
            }

            // 字符串中的逗号数是当前参数的索引
            string sigText = ApplicableToSpan.GetText(m_subjectBuffer.CurrentSnapshot);

            int currentIndex = 0;
            int commaCount = 0;
            while (currentIndex < sigText.Length)
            {
                int commaIndex = sigText.IndexOf(',', currentIndex);
                if (commaIndex == -1)
                {
                    break;
                }
                commaCount++;
                currentIndex = commaIndex + 1;
            }

            if (commaCount < Parameters.Count)
            {
                this.CurrentParameter = Parameters[commaCount];
            }
            else
            {
                // 逗号太多，所以使用最后一个参数作为当前参数。
                this.CurrentParameter = Parameters[Parameters.Count - 1];
            }
        }

        internal void OnSubjectBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            this.ComputeCurrentParameter();
        }

        public ITrackingSpan ApplicableToSpan
        {
            get { return (m_applicableToSpan); }
            internal set { m_applicableToSpan = value; }
        }

        public string Content
        {
            get { return (m_content); }
            internal set { m_content = value; }
        }

        public string Documentation
        {
            get { return (m_documentation); }
            internal set { m_documentation = value; }
        }

        public ReadOnlyCollection<IParameter> Parameters
        {
            get { return (m_parameters); }
            internal set { m_parameters = value; }
        }

        public string PrettyPrintedContent
        {
            get { return (m_prettyPrintedContent); }
            internal set { m_prettyPrintedContent = value; }
        }
    }
}
