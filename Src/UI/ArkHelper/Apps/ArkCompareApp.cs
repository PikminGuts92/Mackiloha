using ArkHelper.Options;
using Mackiloha;
using Mackiloha.Ark;

namespace ArkHelper.Apps
{
    public class ArkCompareApp
    {
        public void Parse(ArkCompareOptions op)
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
                    Hash = Crypt.SHA1Hash(ark2.GetArkEntryFileStream(x))
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

            var newFiles = string.Join('\n', ark1UniqueEntries
                .Select(x => x.Name));

            // TODO: Create formatted console output
        }
    }
}
