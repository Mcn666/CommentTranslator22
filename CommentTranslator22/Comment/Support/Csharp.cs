using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace CommentTranslator22.Comment.Support
{
    internal class Csharp
    {
        public static string SearechComment(SnapshotPoint snapshot)
        {
            return SearchCommentScopeOne(snapshot)
                ?? SearchCommentScopeTwo(snapshot);
        }

        public static string MergeSearchResultOne(in List<string> lines)
        {
            string s = CommentTranslator22Package.ConfigA.MergeCommentBlock ? " " : "\n";
            string res = null;
            foreach (var item in lines)
            {
                int count = 0;
                var str = item.TrimStart();
                foreach (var i in str)
                {
                    if (char.IsPunctuation(i))
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                res += str.Substring(count).TrimStart() + s;
            }

            return res.Trim();
        }

        public static string SearchCommentScopeOne(SnapshotPoint snapshot)
        {
            // 检查鼠标所指向的这一行是否使用了第一种注释
            var lineText = snapshot.GetContainingLine().Extent.GetText();
            var index = lineText.IndexOf("//");
            if (index == -1) return null;

            // 获取鼠标所指向的行号，然后分割快照
            int lineNumber = snapshot.GetContainingLineNumber();
            var splitResult = snapshot.Snapshot.GetText().Replace("\r", "").Split('\n');
            if (splitResult.Length < 1 || splitResult.Length < lineNumber) return null;
            lineText = splitResult[lineNumber].Substring(index + 2);
            if (CommentTranslateInterrupt.Check(lineText)) return null;

            bool addPreviousLine = true, addNextLine = true;
            List<string> lines = new List<string>
            {
                lineText
            };

            for (int i = 1; i < 10; i++)
            {
                if (addPreviousLine && lineNumber - i > -1)
                {
                    index = splitResult[lineNumber - i].IndexOf("//");
                    if (index == -1 ||
                        CommentTranslateInterrupt.Check(splitResult[lineNumber - i]))
                    {
                        addPreviousLine = false;
                    }
                    else
                    {
                        var temp = splitResult[lineNumber - i].Substring(index + 2).TrimEnd();
                        if (temp == "" || temp.Length < 3)
                        {
                            addPreviousLine = false;
                        }
                        else
                        {
                            lines.Insert(0, temp);
                        }
                    }
                }
                if (addNextLine && lineNumber + i < splitResult.Length)
                {
                    index = splitResult[lineNumber + i].IndexOf("//");
                    if (index == -1 ||
                        CommentTranslateInterrupt.Check(splitResult[lineNumber + i]))
                    {
                        addNextLine = false;
                    }
                    else
                    {
                        var temp = splitResult[lineNumber + i].Substring(index + 2).TrimEnd();
                        if (temp == "" || temp.Length < 3)
                        {
                            addNextLine = false;
                        }
                        else
                        {
                            lines.Add(temp);
                        }
                    }
                }
            }
            return MergeSearchResultOne(lines);
        }

        public static string MergeSearchResultTwo(in string[] lines)
        {
            string s = CommentTranslator22Package.ConfigA.MergeCommentBlock ? " " : "\n";
            string res = null;
            foreach (var item in lines)
            {
                int count = 0;
                var str = item.TrimStart();
                foreach (var i in str)
                {
                    if (char.IsPunctuation(i))
                    {
                        count++;
                    }
                    else
                    {
                        break;
                    }
                }
                res += str.Substring(count).TrimStart() + s;
            }
            return res.Trim();
        }

        public static string SearchCommentScopeTwo(SnapshotPoint snapshot)
        {
            var pos = snapshot.Position;
            var res = snapshot.Snapshot.GetText();

            var index1 = res.LastIndexOf("/*", pos);
            if (index1 != -1)
            {
                var temp = res.LastIndexOf("*/", pos); ;
                if (temp != -1 && temp > index1)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            var index2 = res.IndexOf("*/", pos);
            if (index2 != -1)
            {
                var temp = res.IndexOf("/*", pos);
                if (temp != -1 && temp < index2)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            var strSplitResult = res.Substring(index1, index2 - index1 + 2).Replace("\r", "").Split('\n');

            return MergeSearchResultTwo(strSplitResult);
        }
    }
}
