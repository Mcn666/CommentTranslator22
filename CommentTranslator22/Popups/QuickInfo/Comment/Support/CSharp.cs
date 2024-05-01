using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace CommentTranslator22.Popups.QuickInfo.Comment.Support
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

        public static IEnumerable<string> SearchCommentScopeOne(SnapshotPoint snapshot)
        {
            // 检查鼠标所指向的这一行是否使用了第一种注释
            var currentPosition = snapshot.Position;
            var currentLineText = snapshot.GetContainingLine().Extent.GetText();
            var currentSnapshot = snapshot.GetContainingLine().Snapshot.GetText();
            var index = currentSnapshot.LastIndexOf("//", currentPosition);

            if (index == -1 || index + currentLineText.Length < currentPosition)
            {
                return null;
            }

            // 获取鼠标所指向的行号，然后按行分割快照文本
            var lineNumber = snapshot.GetContainingLineNumber();
            var splitResult = currentSnapshot.Replace("\r\n", "\n").Split('\n');
            if (splitResult.Length < 1 || splitResult.Length < lineNumber)
            {
                return null;
            }

            // 检查鼠标所指向的这一行是否属于不执行翻译的类型
            index = currentLineText.LastIndexOf("//");
            currentLineText = splitResult[lineNumber].Substring(index + 2);
            StringPretreatment(ref currentLineText);
            if (CommentTranslateInterrupt.Check(currentLineText))
            {
                return null;
            }

            var addNextLine = true;
            var addPreviousLine = true;
            var linesTextLength = currentLineText.Length;
            var lines = new List<string> { currentLineText };

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
                            linesTextLength += temp.Length;
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
                            linesTextLength += temp.Length;
                            lines.Add(temp);
                        }
                    }
                }
            }
            if (linesTextLength > TranslateClient.Instance.MaxTranslateLength)
            {
                return null;
            }
            if (lines.Count > 1)
            {
                if (lines.Last().Length < TranslateClient.Instance.MinTranslateLength)
                {
                    var last = lines.Last();
                    var temp = lines.Count - 2;
                    lines[temp] = lines[temp] + " " + last;
                    lines.RemoveAt(temp + 1);
                }
            }
            return lines;
        }

        public static IEnumerable<string> SearchCommentScopeTwo(SnapshotPoint snapshot)
        {
            var pos = snapshot.Position;
            var str = snapshot.Snapshot.GetText();

            var index1 = str.LastIndexOf("/*", pos);
            if (index1 != -1)
            {
                var temp = str.LastIndexOf("*/", pos);
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

            var splitResult = str.Substring(index1 + 2, index2 - index1 - 2).Replace("\r\n", "\n").Split('\n');
            var lines = new List<string>();
            var linesTextLength = 0;
            foreach (var line in splitResult)
            {
                var temp = line;
                StringPretreatment(ref temp);
                linesTextLength += temp.Length;
                lines.Add(temp);
            }
            if (linesTextLength > TranslateClient.Instance.MaxTranslateLength)
            {
                return null;
            }
            if (lines.Count > 1)
            {
                if (lines.Last().Length < TranslateClient.Instance.MinTranslateLength)
                {
                    var last = lines.Last();
                    var temp = lines.Count - 2;
                    lines[temp] = lines[temp] + " " + last;
                    lines.RemoveAt(temp + 1);
                }
            }
            return lines;
        }
    }
}
