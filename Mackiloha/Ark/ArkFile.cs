using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // ReadOnlyCollection
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace Mackiloha.Ark
{
    internal class EntryOffset
    {
        public long Offset;
        public int Size;

        public EntryOffset(long offset, int size)
        {
            Offset = offset;
            Size = size;
        }
    }

    public class ArkFile : Archive
    {
        private ArkVersion _version;
        private bool _encrypted;
        private string[] _arkPaths; // 0 = HDR
        private readonly List<OffsetArkEntry> _offsetEntries;

        private const int MAX_HDR_SIZE = 20 * 0x100000; // 20MB
        private const int DEFAULT_KEY = 0x295E2D5E;
        
        private ArkFile() : base()
        {
            _offsetEntries = new List<OffsetArkEntry>();
        }

        public static ArkFile FromFile(string input)
        {
            if (input == null) throw new ArgumentNullException();
            string ext = Path.GetExtension(input).ToLower(); // TODO: Do something with this

            MemoryStream ms = new MemoryStream();

            using (FileStream fs = File.OpenRead(input))
            {
                if (fs.Length > MAX_HDR_SIZE)
                    throw new Exception("HDR file is larger than 20MB");

                fs.CopyTo(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ParseHeader(input, ms);
        }

        private static ArkFile ParseHeader(string input, Stream stream)
        {
            ArkFile ark = new ArkFile();
            ark._encrypted = false;

            using (AwesomeReader ar = new AwesomeReader(stream))
            {
                // Checks version
                int version = ar.ReadInt32();
                
                if (Enum.IsDefined(typeof(ArkVersion), version))
                    ark._version = (ArkVersion)version;
                else
                {
                    // Decrypt stream and re-checks version
                    Crypt.DTBCrypt(ar.BaseStream, version, true);
                    version = ar.ReadInt32();
                    ark._encrypted = true;

                    if (!Enum.IsDefined(typeof(ArkVersion), version))
                    {
                        version = (int)((uint)version ^ 0xFFFFFFFF);

                        // Check one last time
                        if (!Enum.IsDefined(typeof(ArkVersion), version))
                            throw new Exception($"Ark version of \'{version}\' is unsupported");
                        
                        long start = ar.BaseStream.Position;
                        int b;

                        // 0xFF xor rest of stream
                        while ((b = stream.ReadByte()) > -1)
                        {
                            stream.Seek(-1, SeekOrigin.Current);
                            stream.WriteByte((byte)(b ^ 0xFF));
                        }

                        ar.BaseStream.Seek(start, SeekOrigin.Begin);
                    }

                    ark._version = (ArkVersion)version;
                }

                if (version >= 6)
                {
                    // TODO: Save 16-byte hashes
                    uint hashCount = ar.ReadUInt32();
                    ar.BaseStream.Position += hashCount << 4;
                }

                uint arkFileCount = ar.ReadUInt32();
                uint arkFileSizeCount = ar.ReadUInt32(); // Should be same as ark file count

                ulong[] partSizes = new ulong[arkFileSizeCount];
                
                // Reads ark file sizes
                if (version != 4)
                    for (int i = 0; i < partSizes.Length; i++)
                        partSizes[i] = ar.ReadUInt32();
                else
                    // Version 4 uses 64-bit sizes
                    for (int i = 0; i < partSizes.Length; i++)
                        partSizes[i] = ar.ReadUInt64();

                // TODO: Verify the ark parts exist and the sizes match header listing
                if (version >= 5)
                {
                    // Read ark names from hdr
                    uint arkPathsCount = ar.ReadUInt32();
                    ark._arkPaths = new string[arkPathsCount + 1];

                    string directory = Path.GetDirectoryName(input).Replace("\\", "/");
                    ark._arkPaths[0] = input.Replace("\\", "/");
                    
                    for (int i = 0; i < arkPathsCount; i++)
                    {
                        ark._arkPaths[i+1] = $"{directory}/{ar.ReadString()}";
                    }
                }
                else
                    // Make a good guess
                    ark._arkPaths = GetPartNames(input, partSizes.Length);

                if (version >= 6 && version <= 9)
                {
                    // TODO: Save hashes?
                    uint hash2Count = ar.ReadUInt32();
                    ar.BaseStream.Position += hash2Count << 2;
                }

                if (version >= 7)
                {
                    // TODO: Save file collection paths
                    uint fileCollectionCount = ar.ReadUInt32();
                    for (int i = 0; i < fileCollectionCount; i++)
                    {
                        uint fileCount = ar.ReadUInt32();
                        
                        for (int j = 0; j < fileCount; j++)
                        {
                            ar.ReadString();
                        }
                    }
                }

                if (version <= 7)
                {
                    Dictionary<int, string> strings = new Dictionary<int, string>(); // Index, value
                    uint sTableSize = ar.ReadUInt32();
                    int offset = 0;
                    long startPosition = ar.BaseStream.Position;

                    // Reads all strings in table
                    while (offset < sTableSize)
                    {
                        string s = ar.ReadNullString();
                        strings.Add(offset, s);

                        offset = (int)(ar.BaseStream.Position - startPosition);
                    }

                    // Reads string index entries
                    int[] stringIndex = new int[ar.ReadUInt32()];

                    for (int i = 0; i < stringIndex.Length; i++)
                        stringIndex[i] = ar.ReadInt32();

                    // Reads entries
                    uint entryCount = ar.ReadUInt32();

                    if (version >= 4)   
                        for (int i = 0; i < entryCount; i++)
                        {
                            long entryOffset = ar.ReadInt64();
                            string filePath = strings[stringIndex[ar.ReadInt32()]];
                            string direPath = strings[stringIndex[ar.ReadInt32()]];
                            uint size = ar.ReadUInt32();
                            uint inflatedSize = ar.ReadUInt32();

                            // TODO: Do some calculation to figure out which ark path to use
                            ark._offsetEntries.Add(new OffsetArkEntry(entryOffset, filePath, direPath, size, inflatedSize, 1));
                        }
                    else
                        for (int i = 0; i < entryCount; i++)
                        {
                            uint entryOffset = ar.ReadUInt32();
                            string filePath = strings[stringIndex[ar.ReadInt32()]];
                            string direPath = strings[stringIndex[ar.ReadInt32()]];
                            uint size = ar.ReadUInt32();
                            uint inflatedSize = ar.ReadUInt32();

                            // TODO: Do some calculation to figure out which ark path to use
                            ark._offsetEntries.Add(new OffsetArkEntry(entryOffset, filePath, direPath, size, inflatedSize, 1));
                        }
                }
                else
                {
                    uint entryCount = ar.ReadUInt32();

                    // Reads file entries
                    if (version <= 9)
                        for (int i = 0; i < entryCount; i++)
                        {
                            long entryOffset = ar.ReadInt64();
                            string fullPath = ar.ReadString();
                            uint flags = ar.ReadUInt32();
                            uint size = ar.ReadUInt32();
                            uint hash = ar.ReadUInt32();

                            int lastIdx = fullPath.LastIndexOf('/');
                            string filePath = (lastIdx < 0) ? fullPath : fullPath.Remove(0, lastIdx + 1);
                            string direPath = (lastIdx < 0) ? "" : fullPath.Substring(0, lastIdx);

                            // TODO: Do some calculation to figure out which ark path to use
                            ark._offsetEntries.Add(new OffsetArkEntry(entryOffset, filePath, direPath, size, 0, 1));
                        }
                    else
                        for (int i = 0; i < entryCount; i++)
                        {
                            long entryOffset = ar.ReadInt64();
                            string fullPath = ar.ReadString();
                            uint flags = ar.ReadUInt32();
                            uint size = ar.ReadUInt32();

                            int lastIdx = fullPath.LastIndexOf('/');
                            string filePath = (lastIdx < 0) ? fullPath : fullPath.Remove(0, lastIdx + 1);
                            string direPath = (lastIdx < 0) ? "" : fullPath.Substring(0, lastIdx);

                            // TODO: Do some calculation to figure out which ark path to use
                            ark._offsetEntries.Add(new OffsetArkEntry(entryOffset, filePath, direPath, size, 0, 1));
                        }

                    // Reads other entries - Path hashes?
                    // TODO: Save these values
                    uint entryCount2 = ar.ReadUInt32();
                    ar.BaseStream.Position += entryCount2 << 2;
                }
            }

            return ark;
        }

        public void WriteHeader(string path)
        {
            using (var fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                WriteHeader(fs);
        }

        private void WriteHeader(Stream stream)
        {
            AwesomeWriter aw = new AwesomeWriter(stream, false);

            // Writes key if encrypted
            if (_encrypted) aw.Write((int)DEFAULT_KEY);
            long hdrStart = aw.BaseStream.Position;

            // Gets lengths of ark files
            var arkSizes = _arkPaths.Skip(1).Select(x => new FileInfo(x).Length).ToArray();

            aw.Write((int)Version);
            aw.Write(arkSizes.Length);
            aw.Write(arkSizes.Length);

            // Writes ark sizes
            foreach (var size in arkSizes)
                aw.Write((uint)size);

            // Creates and writes string blob
            var entries = _offsetEntries.OrderBy(x => x.Offset).ToList();
            byte[] blob = CreateBlob(out var strings, entries);
            aw.Write((uint)blob.Length);
            aw.Write(blob);

            // Write string offset table
            int[] stringOffsets = new int[(entries.Count * 2) + 200];
            aw.Write(stringOffsets.Length);
            
            int CalculateHash(string str, int tableSize)
            {
                int hash = 0;
                foreach (char c in str)
                {
                    hash = (hash * 0x7F) + c;
                    hash -= ((hash / tableSize) * tableSize);
                }
                return hash;
            }

            Dictionary<string, int> tableOffsets = new Dictionary<string, int>();
            foreach (var str in strings)
            {
                int hash = CalculateHash(str.Key, stringOffsets.Length);
                
                // Prevents duplicate hashes
                while (stringOffsets[hash] != 0)
                {
                    hash++;
                    if (hash >= stringOffsets.Length) hash = 0;
                }

                stringOffsets[hash] = str.Value;

                // Sets previous hash offset
                tableOffsets.Add(str.Key, hash);
            }

            // Writes offsets
            foreach (var offset in stringOffsets)
                aw.Write((uint)offset);

            // Sort by hash index
            entries.Sort((x, y) =>
            {
                int xValue = tableOffsets[x.Directory];
                int yValue = tableOffsets[y.Directory];

                // The directories are the same
                if (xValue == yValue)
                {
                    // So compare file names
                    xValue = tableOffsets[x.FileName];
                    yValue = tableOffsets[y.FileName];
                }

                return xValue - yValue;
            });

            aw.Write((uint)entries.Count);
            foreach(var entry in entries)
            {
                aw.Write((uint)entry.Offset);
                aw.Write((uint)tableOffsets[entry.FileName]);
                aw.Write((uint)tableOffsets[entry.Directory]);
                aw.Write((uint)entry.Size);
                aw.Write((uint)entry.InflatedSize);
            }

            if (_encrypted)
            {
                // Encrypts HDR file
                aw.BaseStream.Seek(hdrStart, SeekOrigin.Begin);
                Crypt.DTBCrypt(aw.BaseStream, DEFAULT_KEY, true);
            }
        }

        private byte[] CreateBlob(out Dictionary<string, int> offsets, List<OffsetArkEntry> entries)
        {
            offsets = new Dictionary<string, int>();
            byte[] nullByte = { 0x00 };
            
            using (MemoryStream ms = new MemoryStream())
            {
                // Adds null byte
                offsets.Add("", 0);
                ms.Write(nullByte, 0, nullByte.Length);

                foreach (var entry in entries)
                {
                    // Writes directory name
                    if (!offsets.ContainsKey(entry.Directory))
                    {
                        offsets.Add(entry.Directory, (int)ms.Position);

                        byte[] data = Encoding.ASCII.GetBytes(entry.Directory);
                        ms.Write(data, 0, data.Length);
                        ms.Write(nullByte, 0, nullByte.Length);
                    }

                    // Writes file name
                    if (!offsets.ContainsKey(entry.FileName))
                    {
                        offsets.Add(entry.FileName, (int)ms.Position);

                        byte[] data = Encoding.ASCII.GetBytes(entry.FileName);
                        ms.Write(data, 0, data.Length);
                        ms.Write(nullByte, 0, nullByte.Length);
                    }
                }

                return ms.ToArray();
            }
        }

        public override void CommitChanges()
        {
            if (!PendingChanges) return;
            
            var remainingOffsetEntries = _offsetEntries.Except<ArkEntry>(_pendingEntries).Select(x => x as OffsetArkEntry).OrderBy(x => x.Offset).ToList();

            List<EntryOffset> GetGaps()
            {
                List<EntryOffset> offsetGaps = new List<EntryOffset>();
                long previousOffset = 0;

                foreach (var offsetEntry in remainingOffsetEntries)
                {
                    if (offsetEntry.Offset - previousOffset == 0)
                    {
                        // No gap, continues
                        previousOffset = offsetEntry.Offset + offsetEntry.Size;
                        continue;
                    }

                    // Adds gap to list
                    long gapOffset = previousOffset;
                    int gapSize = (int)(offsetEntry.Offset - previousOffset);
                    offsetGaps.Add(new EntryOffset(gapOffset, gapSize));

                    previousOffset = offsetEntry.Offset + offsetEntry.Size;
                }

                return offsetGaps;
            }

            void CopyToArchive(string arkFile, long arkOffset, string entryFile)
            {
                // TODO: Extract out of this
                using (FileStream fsArk = File.OpenWrite(arkFile))
                {
                    fsArk.Seek(arkOffset, SeekOrigin.Begin);

                    using (FileStream fsEntry = File.OpenRead(entryFile))
                    {
                        fsEntry.CopyTo(fsArk);
                    }
                }
            }

            // TODO: Compare previousOffset to ark file size
            List<EntryOffset> gaps = GetGaps();
            var pendingEntries = _pendingEntries.Select(x => new { Length = new FileInfo(x.LocalFilePath).Length, Entry = x }).OrderBy(x => x.Length);
            
            foreach (var pending in pendingEntries)
            {
                // Looks at smallest gaps first, selects first fit
                var bestFit = gaps.OrderBy(x => x.Size).FirstOrDefault(x => x.Size >= pending.Length);
                
                if (bestFit == null)
                {
                    // Adds to end of last archive file
                    var lastEntry = remainingOffsetEntries.OrderByDescending(x => x.Offset).FirstOrDefault();
                    long offset = (lastEntry != null) ? lastEntry.Offset + lastEntry.Size : 0;

                    // Copies entry to ark file (TODO: Calculate arkPath beforehand)
                    CopyToArchive(_arkPaths[1], offset, pending.Entry.LocalFilePath);

                    // Adds ark offset entry
                    remainingOffsetEntries.Add(new OffsetArkEntry(offset, pending.Entry.FileName, pending.Entry.Directory, (uint)pending.Length, 0, 1));
                }
                else
                {
                    // Copies entry to ark file (TODO: Calculate arkPath beforehand)
                    CopyToArchive(_arkPaths[1], bestFit.Offset, pending.Entry.LocalFilePath);

                    // Adds ark offset entry
                    remainingOffsetEntries.Add(new OffsetArkEntry(bestFit.Offset, pending.Entry.FileName, pending.Entry.Directory, (uint)pending.Length, 0, 1));

                    // Updates gap entry
                    if (bestFit.Size == pending.Length)
                    {
                        // Remove gap
                        gaps.Remove(bestFit);
                    }
                    else
                    {
                        // Updates values
                        bestFit.Offset += pending.Length;
                        bestFit.Size -= (int)pending.Length;
                    }
                }
            }

            // Updates archive entries
            _pendingEntries.Clear();
            _offsetEntries.Clear();
            _offsetEntries.AddRange(remainingOffsetEntries);

            // Re-writes header file
            WriteHeader(_arkPaths[0]);

            // TODO: Add an output log
        }

        protected override byte[] GetArkEntryBytes(ArkEntry entry)
        {
            if (entry is OffsetArkEntry)
            {
                var offEntry = entry as OffsetArkEntry;

                string arkPath = ArkPath(offEntry.Part);
                byte[] data = new byte[offEntry.Size];

                using (FileStream fs = File.OpenRead(arkPath))
                {
                    fs.Seek(offEntry.Offset, SeekOrigin.Begin);
                    fs.Read(data, 0, data.Length);
                }

                return data; // Not very efficient or super smart at the moment
            }
            else
                // TODO: Implement reading from non-archive file on disk
                throw new Exception();
        }

        private static string[] GetPartNames(string hdrPath, int count)
        {
            string directory = Path.GetDirectoryName(hdrPath).Replace("\\", "/");
            string fileName = Path.GetFileNameWithoutExtension(hdrPath);
            string extension = Path.GetExtension(hdrPath).All(c => c == '.' || char.IsUpper(c)) ? ".ARK" : ".ark";

            string[] arkPaths = new string[count + 1];
            arkPaths[0] = hdrPath.Replace("\\", "/");

            for (int i = 0; i < count; i++)
                arkPaths[i+1] = $"{directory}/{fileName}_{i}{extension}";

            return arkPaths;
        }
        
        public override void AddPendingEntry(PendingArkEntry pending)
        {
            // TODO: Check if local file path exists?
            var entry = GetArkEntry(pending.FullPath);

            if (entry == null || entry is OffsetArkEntry)
            {
                // Adds new pending entry
                _pendingEntries.Add(new PendingArkEntry(pending));
            }
            else if (entry is PendingArkEntry)
            {
                // Updates pending entry
                _pendingEntries.Remove(pending);
                _pendingEntries.Add(new PendingArkEntry(pending));
            }
        }

        protected override ArkEntry GetArkEntry(string fullPath)
        {
            var pendingEntry = _pendingEntries.FirstOrDefault(x => string.Compare(x.FullPath, fullPath, true) == 0);
            if (pendingEntry != null) return pendingEntry;

            return _offsetEntries.FirstOrDefault(x => string.Compare(x.FullPath, fullPath, true) == 0);
        }

        protected override List<ArkEntry> GetMergedEntries()
        {
            var entries = new List<ArkEntry>(_pendingEntries);
            entries.AddRange(_offsetEntries.Except<ArkEntry>(_pendingEntries));
            entries.Sort((x, y) => string.Compare(x.FullPath, y.FullPath));

            return entries;
        }
        
        public string DirectoryName => Path.GetDirectoryName(this._arkPaths[0]);
        public override string FileName => Path.GetFileName(this._arkPaths[0]);
        public override string FullPath => this._arkPaths[0];

        internal string ArkPath(int index) => this._arkPaths[index];
        
        public bool Encrypted => this._encrypted;
        public ArkVersion Version => this._version;
    }
}
