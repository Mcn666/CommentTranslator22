using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace CommentTranslator22.Popups.QuickInfo.Comment.Support
{
    internal class XAML
    {
        public static IEnumerable<string> FindComment(SnapshotPoint snapshot)
        {
            return CommentHelp.FindComment(snapshot, "<!--", "-->");
        }
    }
}
