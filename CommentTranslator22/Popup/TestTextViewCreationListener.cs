﻿using CommentTranslator22.Config;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace CommentTranslator22.Popup
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class TestTextViewCreationListener : IWpfTextViewCreationListener
    {
        public void TextViewCreated(IWpfTextView textView)
        {
            TestSolutionEvents.Create();
        }
    }
}
