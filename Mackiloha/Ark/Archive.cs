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
    internal struct EntryOffset
    {
        long Offset;
        int Size;

        public EntryOffset(long offset, int size)
        {
            Offset = offset;
            Size = size;
        }
    }

    public class Archive
    {
        private ArkVersion _version;
        private bool _encrypted;
        private string[] _arkPaths; // 0 = HDR
        private readonly List<OffsetArkEntry> _offsetEntries;
        private readonly List<PendingArkEntry> _pendingEntries;

        private string _workingDirectory;

        private Archive()
        {
            _offsetEntries = new List<OffsetArkEntry>();
            _pendingEntries = new List<PendingArkEntry>();
        }

        public static Archive FromFile(string input)
        {
            if (input == null) throw new ArgumentNullException();
            string ext = Path.GetExtension(input).ToLower(); // TODO: Do something with this

            using (FileStream fs = File.OpenRead(input))
            {
                return ParseHeader(input, fs);
            }
        }

        private static Archive ParseHeader(string input, Stream stream)
        {
            Archive ark = new Archive();
            ark._version = ArkVersion.V3;
            ark._encrypted = false;

            using (AwesomeReader ar = new AwesomeReader(stream))
            {
                // TODO: Check version number (Assume V3 for now)
                int version = ar.ReadInt32();
                uint arkFileCount = ar.ReadUInt32();
                uint arkFileSizeCount = ar.ReadUInt32(); // Should be same as ark file count

                uint[] partSizes = new uint[arkFileSizeCount];
                ark._arkPaths = GetPartNames(input, partSizes.Length);

                // Reads ark file sizes
                for (int i = 0; i < partSizes.Length; i++)
                    partSizes[i] = ar.ReadUInt32();

                // TODO: Verify the ark parts exist and the sizes match header listing

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

            return ark;
        }

        public void WriteHeader(string path)
        {
            using (var fs = File.OpenWrite(path))
                WriteHeader(fs);
        }

        private void WriteHeader(Stream stream)
        {
            // TODO: Check for encryption (and big endian?)
            AwesomeWriter aw = new AwesomeWriter(stream, false);
            int arkCount = _arkPaths.Length - 1;

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

        public void CommitChanges()
        {
            if (!PendingChanges) return;

            //var entries = Entries;


            var remainingOffsetEntries = _offsetEntries.Except<ArkEntry>(_pendingEntries).Select(x => x as OffsetArkEntry).OrderBy(x => x.Offset);
            List<EntryOffset> gaps = new List<EntryOffset>();

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
                gaps.Add(new EntryOffset(gapOffset, gapSize));
                
                previousOffset = offsetEntry.Offset + offsetEntry.Size;
            }

            // TODO: Compare previousOffset to ark file size

            var pendingEntries = _pendingEntries.Select(x => new { Length = new FileInfo(x.LocalFilePath).Length, Entry = x }).OrderBy(x => x.Length);
            

            foreach (var gap in gaps)
            {

            }

            //var pending = this._entries.Where(x => x.Status == ArkEntryStatus.PendingChanges);
            //if (pending.Count() <= 0) return;

            // TODO: Add an output log
        }

        public Stream GetArkEntryFileStream(ArkEntry entry) => new MemoryStream(GetArkEntryBytes(entry), false); // Read-only

        private byte[] GetArkEntryBytes(ArkEntry entry)
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

        public void SetWorkingDirectory(string path)
        {
            path = Path.GetFullPath(path).Replace("\\", "/");

            // Checks access permission
            if (!FileHelper.HasAccess(path))
            {
                // Do something here
            }

            this._workingDirectory = path;
        }

        public void AddPendingEntry(PendingArkEntry pending)
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

        private ArkEntry GetArkEntry(string fullPath)
        {
            var pendingEntry = _pendingEntries.FirstOrDefault(x => string.Compare(x.FullPath, fullPath, true) == 0);
            if (pendingEntry != null) return pendingEntry;

            return _offsetEntries.FirstOrDefault(x => string.Compare(x.FullPath, fullPath, true) == 0);
        }

        private List<ArkEntry> GetMergedEntries()
        {
            var entries = new List<ArkEntry>(_pendingEntries);
            entries.AddRange(_offsetEntries.Except<ArkEntry>(_pendingEntries));
            entries.Sort((x, y) => string.Compare(x.FullPath, y.FullPath));

            return entries;
        }

        public ArkEntry this[string fullPath] => GetArkEntry(fullPath);

        public string DirectoryName => Path.GetDirectoryName(this._arkPaths[0]);
        public string FileName => Path.GetFileName(this._arkPaths[0]);
        public string FullPath => this._arkPaths[0];

        internal string ArkPath(int index) => this._arkPaths[index];
        
        public ReadOnlyCollection<ArkEntry> Entries => new ReadOnlyCollection<ArkEntry>(GetMergedEntries());
        public bool PendingChanges => _pendingEntries.Count > 0;

        public bool Encrypted => this._encrypted;
        public ArkVersion Version => this._version;
        public string WorkingDirectory => this._workingDirectory;
    }
}
