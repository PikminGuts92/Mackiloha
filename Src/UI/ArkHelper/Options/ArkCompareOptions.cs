using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using Mackiloha;
using Mackiloha.Ark;

namespace ArkHelper.Options
{
    [Verb("arkcompare", HelpText = "Compares differences between two arks", Hidden = true)]
    public class ArkCompareOptions
    {
        [Value(0, Required = true, MetaName = "arkPath1", HelpText = "Path to ark 1 (hdr file)")]
        public string ArkPath1 { get; set; }

        [Value(1, Required = true, MetaName = "arkPath2", HelpText = "Path to ark 2 (hdr file)")]
        public string ArkPath2 { get; set; }

        public static void Parse(ArkCompareOptions op)
        {
            var ark1 = ArkFile.FromFile(op.ArkPath1);
            var ark2 = ArkFile.FromFile(op.ArkPath2);

            var ark1Entries = ark1.Entries
                .Select(x => x as OffsetArkEntry)
                .Select(x => new
                {
                    Name = x.FullPath,
                    x.Size,
                    Hash = Crypt.SHA1Hash(ark1.GetArkEntryFileStream(x))
                })
                .ToList();

            var ark2Entries = ark2.Entries
                .Select(x => x as OffsetArkEntry)
                .Select(x => new
                {
                    Name = x.FullPath,
                    x.Size,
                    Hash = Crypt.SHA1Hash(ark1.GetArkEntryFileStream(x))
                })
                .ToList();

            var sharedEntries = ark1Entries
                .Intersect(ark2Entries)
                .ToList();

            var ark1UniqueEntries = ark1Entries
                .Except(sharedEntries)
                .ToList();

            var ark2UniqueEntries = ark2Entries
                .Except(sharedEntries)
                .ToList();

            // TODO: Create formatted console output
        }
    }
}
