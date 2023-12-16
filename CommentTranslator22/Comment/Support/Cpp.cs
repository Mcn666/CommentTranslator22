using Microsoft.VisualStudio.Text;

namespace CommentTranslator22.Comment.Support
{
    internal class Cpp
    {
        public static string SearechComment(SnapshotPoint snapshot)
        {
            return Csharp.SearechComment(snapshot);
        }
    }
}
