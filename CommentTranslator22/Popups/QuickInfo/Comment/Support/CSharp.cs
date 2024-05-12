using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Text;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace CommentTranslator22.Popups.QuickInfo.Comment.Support
{
    internal class CSharp
    {
        public static IEnumerable<string> SearechComment(SnapshotPoint snapshot)
        {
            return SearchCommentScopeOne(snapshot)
                ?? SearchCommentScopeTwo(snapshot);
        }

        static IEnumerable<string> SearchCommentScopeOne(SnapshotPoint snapshot)
        {
            // 先获得文件快照，再按顺序替换行尾符号，最后按行尾符号分割字符串
            var snapshotBuffer = snapshot.Snapshot.GetText();
            var splitResult = snapshotBuffer.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            if (splitResult == null || splitResult.Any() == false)
            {
                return null;
            }

            var lineNumber = snapshot.GetContainingLineNumber();
            var currentPosition = snapshot.Position;
            var currentLineText = splitResult[lineNumber];

            var index = currentLineText.LastIndexOf("//");
            if (index == -1)
            {
                return null;
            }
            if (snapshot.Snapshot.LineCount > lineNumber)
            {
                for (var i = 0; i < lineNumber; i++)
                {
                    // 从 Snapshot 的 Lines 中获取每一行的长度，但要注意不是使用 Length，而是使用 LengthIncludingLineBreak
                    // LengthIncludingLineBreak 这个包含了行尾符号的长度
                    var temp = snapshot.Snapshot.Lines.ElementAt(i);
                    currentPosition -= temp.LengthIncludingLineBreak;
                }
            }
            if (currentPosition < index)
            {
                return null;
            }

            // 检查鼠标所指向的这一行是否属于不执行翻译的类型
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

            return ResultDispose(ref lines);
        }

        static IEnumerable<string> SearchCommentScopeTwo(SnapshotPoint snapshot)
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

            return ResultDispose(ref lines);
        }

        static IEnumerable<string> ResultDispose(ref List<string> ls)
        {
            if (ls.Count > 1)
            {
                if (ls.Last().Length < TranslateClient.Instance.MinTranslateLength)
                {
                    var temp = ls.Count - 2;
                    if (ls[temp].Length > TranslateClient.Instance.MinTranslateLength)
                    {
                        ls[temp] = ls[temp] + " " + ls.Last();
                        ls.RemoveAt(temp + 1);
                    }
                }
            }
            return ls;
        }

        public static void StringPretreatment(ref string str)
        {
            var count = 0;
            var temp = str.TrimStart();
            foreach (var s in temp)
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
    }
}
