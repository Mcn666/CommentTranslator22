using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace CommentTranslator22.Comment.Support
{
    internal class CSharp
    {
        public static IEnumerable<string> SearechComment(SnapshotPoint snapshot)
        {
            return SearchCommentScopeOne(snapshot)
                ?? SearchCommentScopeTwo(snapshot);
        }

        public static void StringPretreatment(ref string str)
        {
            var count = 0;
            var temp = str.TrimStart();
            foreach (var s in str)
            {
                if (char.IsPunctuation(s))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            str = temp.Substring(count).Trim();
        }

        public static IEnumerable<string> MergeSearchResult(in List<string> lines)
        {
            if (CommentTranslator22Package.ConfigA.MergeCommentBlock)
            {
                var str = string.Empty;
                foreach (var line in lines)
                {
                    str += line + ' ';
                }
                return new List<string> { str };
            }
            return lines;
        }

        public static IEnumerable<string> SearchCommentScopeOne(SnapshotPoint snapshot)
        {
            // 检查鼠标所指向的这一行是否使用了第一种注释
            var lineText = snapshot.GetContainingLine().Extent.GetText();
            var index = lineText.LastIndexOf("//");
            if (index == -1) return null;

            // 获取鼠标所指向的行号，然后按行分割快照文本
            int lineNumber = snapshot.GetContainingLineNumber();
            var splitResult = snapshot.Snapshot.GetText().Replace("\r\n", "\n").Split('\n');
            if (splitResult.Length < 1 || splitResult.Length < lineNumber) return null;

            // 检查鼠标所指向的这一行是否属于不执行翻译的类型
            lineText = splitResult[lineNumber].Substring(index + 2);
            StringPretreatment(ref lineText);
            if (CommentTranslateInterrupt.Check(lineText)) return null;

            bool addPreviousLine = true, addNextLine = true;
            List<string> lines = new List<string> { lineText };

            for (int i = 1; i < 10; i++)
            {
                if (addPreviousLine && lineNumber - i > -1)
                {
                    index = splitResult[lineNumber - i].LastIndexOf("//");
                    if (index == -1)
                    {
                        addPreviousLine = false;
                    }
                    else
                    {
                        var temp = splitResult[lineNumber - i].Substring(index + 2).TrimEnd();
                        StringPretreatment(ref temp);
                        if (temp == string.Empty || CommentTranslateInterrupt.Check(temp))
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
                    index = splitResult[lineNumber + i].LastIndexOf("//");
                    if (index == -1)
                    {
                        addNextLine = false;
                    }
                    else
                    {
                        var temp = splitResult[lineNumber + i].Substring(index + 2).TrimEnd();
                        StringPretreatment(ref temp);
                        if (temp == "" || CommentTranslateInterrupt.Check(temp))
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
            return MergeSearchResult(lines);
        }

        public static IEnumerable<string> SearchCommentScopeTwo(SnapshotPoint snapshot)
        {
            var pos = snapshot.Position;
            var str = snapshot.Snapshot.GetText();

            var index1 = str.LastIndexOf("/*", pos);
            if (index1 != -1)
            {
                var temp = str.LastIndexOf("*/", pos); ;
                if (temp != -1 && temp > index1)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            var index2 = str.IndexOf("*/", pos);
            if (index2 != -1)
            {
                var temp = str.IndexOf("/*", pos);
                if (temp != -1 && temp < index2)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            var splitResult = str.Substring(index1, index2 - index1 - 2).Replace("\r\n", "\n").Split('\n');
            var lines = new List<string>();
            foreach (var line in splitResult)
            {
                var temp = line;
                StringPretreatment(ref temp);
                lines.Add(temp);
            }

            return MergeSearchResult(lines);
        }
    }
}
