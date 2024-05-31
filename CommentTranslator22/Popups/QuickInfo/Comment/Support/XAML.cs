using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommentTranslator22.Popups.QuickInfo.Comment.Support
{
    internal class XAML
    {
        public static IEnumerable<string> SearchComment(SnapshotPoint snapshot)
        {
            return CommentDispose.SearchComment(snapshot, "<!--", "-->");
        }

        
    }
}
