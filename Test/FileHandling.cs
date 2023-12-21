using System;
using System.Collections.Generic;
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
            StringR(fileName + ".txt", out List<string> lines);
            StringW(fileName + "-副本.txt", lines);
        }

        static void StringR(string fileName, out List<string> lines)
        {
            lines = [];
            if (File.Exists(fileName) == false)
            {
                return;
            }

            using (var sr = new StreamReader(fileName))
            {
                while (true)
                {
                    var line = sr.ReadLine();
                    if (line == null)
                        break;

                    if (string.IsNullOrEmpty(line))
                        continue;

                    var splits = line.Split(' ');

                    if (splits[0].Length > 1)
                    {
                        var punctuation = false;
                        foreach (var split in splits[0])
                        {
                            if (char.IsPunctuation(split))
                            {
                                punctuation = true;
                                break;
                            }
                        }
                        if (punctuation == false)
                        {
                            var res = false;
                            foreach (var item in lines)
                            {
                                if (Equals(item, splits[0]))
                                {
                                    res = true;
                                    break;
                                }
                            }
                            if (res == false)
                            {
                                lines.Add(splits[0]);
                            }
                        }
                        
                    }
                }
                sr.Close();
            }
        }

        static void StringW(string fileName, in List<string> lines)
        {
            using (var sw = new StreamWriter(fileName))
            {
                foreach (var line in lines)
                {
                    var temp = line.ToLower();
                    if (Regex.IsMatch(temp, "[a-z]"))
                    {
                        sw.WriteLine(temp);
                    }
                }
                sw.Close();
            }
        }
    }
}
