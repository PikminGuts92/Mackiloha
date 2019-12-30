using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Mackiloha;
using Mackiloha.Ark;

namespace SuperFreqCLI.Options
{
    [Verb("hashfinder", HelpText = "Compute hashes for ark entries and find offsets in decrypted executable", Hidden = true)]
    public class HashFinderOptions
    {
        private class EntryInfo
        {
            public string Path { get; set; }
            public string Hash { get; set; }
            public long Offset { get; set; }
        }

        [Value(0, Required = true, MetaName = "arkPath", HelpText = "Path to ark (hdr file)" )]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "exePath", HelpText = "Path to decrypted executable")]
        public string ExePath { get; set; }

        [Option('h', "hashPath", HelpText = "Path to file containing hash offsets", Required = true)]
        public string HashesPath { get; set; }

        public static void Parse(HashFinderOptions op)
        {
            var watch = Stopwatch.StartNew();

            var ark = ArkFile.FromFile(op.InputPath);
            var exts = new[] { "dtb", "elf", "mid" };

            var arkEntries = ark
                .Entries
                .Where(x => exts.Contains(x.Extension))
                .ToList();

            var entryInfo = arkEntries
                .Select(x => new EntryInfo()
                {
                    Path = x.FullPath,
                    Hash = "",
                    Offset = -1
                })
                .ToList();

            var i = 0;
            foreach (var dtb in arkEntries)
            {
                using (var dtbStream = ark.GetArkEntryFileStream(dtb))
                {
                    entryInfo[i].Hash = Crypt.SHA1Hash(dtbStream);
                    i++;
                }
            }

            byte[] GetBytes(string hex)
            {
                var bytes = new byte[hex.Length >> 1];
                
                for (int i = 0; i < hex.Length; i += 2)
                {
                    bytes[i >> 1] = Convert.ToByte(hex.Substring(i, 2), 16);
                }

                return bytes;
            }

            var exeBytes = File.ReadAllBytes(op.ExePath);

            Parallel.ForEach(entryInfo, (info) =>
            {
                using (var ar = new AwesomeReader(new MemoryStream(exeBytes)))
                {
                    var hashBytes = GetBytes(info.Hash);
                    ar.BaseStream.Seek(0, SeekOrigin.Begin);

                    var offset = ar.FindNext(hashBytes);
                    if (offset != -1)
                        info.Offset = offset;
                }
            });

            watch.Stop();

            using (var sw = new StreamWriter(op.HashesPath, false, Encoding.UTF8))
            {
                sw.WriteLine("Path,Hash,Offset");

                foreach (var info in entryInfo
                    .Where(x => x.Offset != -1)
                    .OrderBy(x => x.Path))
                {
                    sw.WriteLine($"{info.Path},{info.Hash},{info.Offset}");
                }
            }

            var hashCount = entryInfo
                .Where(x => x.Offset != -1)
                .Count();

            Console.WriteLine($"Found offsets for {hashCount} entries of out {entryInfo.Count}");
            Console.WriteLine($"Scan took {watch.Elapsed} ({watch.ElapsedMilliseconds}ms)");
        }
    }
}
