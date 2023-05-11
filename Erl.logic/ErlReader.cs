using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Erl.logic
{
    internal class ErlReader
    {
        public List<string> ReadErlFiles(string path)
        {
            List<string> txt = new List<string>();

            if (Regex.IsMatch(path, @"\.erl|\.ERL"))
            {
                txt = File.ReadAllLines(path).ToList();
            }
            else
            {
                ReadFromDirectory(path, txt);
            }

            return txt;
        }

        private static void ReadFromDirectory(string path, List<string> txt)
        {
            ConcurrentQueue<string> listFiles = new ConcurrentQueue<string>();
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (Regex.IsMatch(file, @"\.erl|.ERL"))
                {
                    listFiles.Enqueue(file);
                }
            }

            Dictionary<string, string[]> Files = new Dictionary<string, string[]>();
            Parallel.ForEach(listFiles, (currentFile =>
            {
                var lst = File.ReadAllLines(currentFile);

                Files.Add(currentFile, lst);
            }));

            foreach (var file in listFiles)
            {
                txt.AddRange(Files[file]);
            }
        }
    }
}
