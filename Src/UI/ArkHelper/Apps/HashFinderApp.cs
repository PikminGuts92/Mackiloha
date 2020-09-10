using ArkHelper.Models;
using ArkHelper.Options;
using Mackiloha;
using Mackiloha.Ark;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArkHelper.Apps
{
    public class HashFinderApp
    {
        public void Parse(HashFinderOptions op)
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
