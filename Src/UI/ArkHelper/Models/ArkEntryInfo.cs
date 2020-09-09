using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ArkHelper.Models
{
    internal class ArkEntryInfo
    {
        public string Path { get; set; }
        public string Hash { get; set; }
        public long Offset { get; set; }

        public static List<ArkEntryInfo> ReadFromCSV(string csvPath)
        {
            List<ArkEntryInfo> infoEntries = new List<ArkEntryInfo>();

            string[] lineSplit;
            using (var ar = new StreamReader(csvPath, Encoding.UTF8))
            {
                ar.ReadLine(); // Header info

                while (!ar.EndOfStream)
                {
                    lineSplit = ar.ReadLine().Split(',');

                    infoEntries.Add(new ArkEntryInfo()
                    {
                        Path = lineSplit[0].Trim(),
                        Hash = lineSplit[1].Trim(),
                        Offset = long.Parse(lineSplit[2].Trim())
                    });
                }
            }

            return infoEntries;
        }

        public static void WriteToCSV(List<ArkEntryInfo> infoEntries, string csvPath)
        {
            using (var sw = new StreamWriter(csvPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Path,Hash,Offset");

                foreach (var info in infoEntries)
                {
                    sw.WriteLine($"{info.Path},{info.Hash},{info.Offset}");
                }
            }
        }
    }
}
