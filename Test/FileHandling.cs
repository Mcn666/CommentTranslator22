using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Test
{
    internal class FileHandling
    {
        public static void Func()
        {
            var fileName = "words";
            StringReader(fileName + ".txt", out List<string> lines);
            //StringWriter(fileName + "-副本.txt", lines);
            FenLi(fileName, lines);
        }

        static void StringReader(string fileName, out List<string> strings)
        {
            strings = [];
            if (File.Exists(fileName) == false)
            {
                return;
            }

            using (var sr = new StreamReader(fileName))
            {
                // 读取文件全部内容，然后将\r\n转换为\n，再按照\n分割行
                var lines = sr.ReadToEnd().Replace("\r\n", "\n").Split('\n');
                foreach (var s in lines)
                {
                    // 转换为小写字母，然后检查所有字符是否都在 a-z 的范围里
                    var temp = s.ToLower().Trim();
                    if (Regex.IsMatch(temp, "^[a-z]+$"))
                    {
                        strings.Add(temp);
                    }
                }
                sr.Close();
            }
        }

        static void StringWriter(string fileName, in List<string> strings)
        {
            using (var sw = new StreamWriter(fileName))
            {
                foreach(var line in strings)
                {
                    sw.WriteLine(line);
                }
                sw.Close();
            }
        }

        static void FenLi(string fileName, List<string> strings)
        {
            foreach (var s in strings)
            {
                var c = s[0];
                using (var sw = new StreamWriter($"{fileName}-{c}.txt", true))
                {
                    sw.WriteLine(s);
                    sw.Close() ;
                }
            }
        }
    }
}
