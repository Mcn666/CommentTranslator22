using CommentTranslator22.Translate;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace CommentTranslator22.Popups.QuickInfo.Comment.Support
{
    internal class CommentHelp
    {
        /// <summary>
        /// TranslateAsync 方法需要使用的部分
        /// </summary>
        /// <param name="snapshot"></param>
        /// <returns></returns>
        public static IEnumerable<string> FindComment(SnapshotPoint snapshot)
        {
            var typeName = snapshot.Snapshot.TextBuffer.ContentType.TypeName;
            switch (typeName)
            {
                case "C/C++":
                case "CSharp":
                    return CSharp.FindComment(snapshot);
                case "XML":
                case "XAML":
                    return XAML.FindComment(snapshot);
            }
            return null;
        }

        public static IEnumerable<string> FindComment(SnapshotPoint snapshot, string start, string end)
        {
            var pos = snapshot.Position;
            var str = snapshot.Snapshot.GetText();

            var index1 = str.LastIndexOf(start, pos);
            if (index1 != -1)
            {
                var temp = str.LastIndexOf(end, pos);
                if (temp != -1 && temp > index1)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            var index2 = str.IndexOf(end, pos);
            if (index2 != -1)
            {
                var temp = str.IndexOf(start, pos);
                if (temp != -1 && temp < index2)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            index1 += start.Length;
            var lines = str.Substring(index1, index2 - index1).Replace("\r\n", "\n").Split('\n');
            var lineList = new List<string>();
            var lengthText = 0;
            foreach (var line in lines)
            {
                var temp = line;
                StringPretreatment(ref temp);
                if (CommentTranslateInterrupt.Check(temp))
                {
                    continue;
                }
                lengthText += temp.Length;
                lineList.Add(temp);
            }
            if (lengthText > TranslationClient.Instance.MaxTranslateLength)
            {
                return null;
            }

            return StringPretreatment(ref lineList);
        }

        public static IEnumerable<string> StringPretreatment(ref List<string> ls)
        {
            if (ls.Count > 1)
            {
                if (ls.Last().Length < TranslationClient.Instance.MinTranslateLength)
                {
                    var temp = ls.Count - 2;
                    if (ls[temp].Length > TranslationClient.Instance.MinTranslateLength)
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
