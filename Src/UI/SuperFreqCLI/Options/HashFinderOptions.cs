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
using SuperFreqCLI.Models;

namespace SuperFreqCLI.Options
{
    [Verb("hashfinder", HelpText = "Compute hashes for ark entries and find offsets in decrypted executable", Hidden = true)]
    public class HashFinderOptions
    {
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
                .Select(x => new ArkEntryInfo()
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

            var exeBytes = File.ReadAllBytes(op.ExePath);

            Parallel.ForEach(entryInfo, (info) =>
            {
                using (var ar = new AwesomeReader(new MemoryStream(exeBytes)))
                {
                    var hashBytes = FileHelper.GetBytes(info.Hash);
                    ar.BaseStream.Seek(0, SeekOrigin.Begin);

                    var offset = ar.FindNext(hashBytes);
                    if (offset != -1)
                        info.Offset = offset;
                }
            });

            watch.Stop();

            var protectedInfo = entryInfo
                .Where(x => x.Offset != -1)
                .OrderBy(x => x.Path)
                .ToList();

            ArkEntryInfo.WriteToCSV(protectedInfo, op.HashesPath);
            var hashCount = protectedInfo.Count();

            Console.WriteLine($"Found offsets for {hashCount} entries of out {entryInfo.Count}");
            Console.WriteLine($"Scan took {watch.Elapsed} ({watch.ElapsedMilliseconds}ms)");
        }
    }
}
