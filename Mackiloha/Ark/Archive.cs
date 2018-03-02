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
    public class Archive
    {
        private ArkVersion _version;
        private bool _encrypted;
        private string[] _arkPaths; // 0 = HDR
        private readonly List<ArkEntry> _offsetEntries;
        private readonly List<PendingArkEntry> _pendingEntries;

        private string _workingDirectory;

        private Archive()
        {
            _offsetEntries = new List<ArkEntry>();
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

        private void CommitChanges()
        {
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

        private ArkEntry GetArkEntry(string fullPath)
        {
            var pendingEntry = _pendingEntries.FirstOrDefault(x => string.Compare(x.FullPath, fullPath, true) == 0);
            if (pendingEntry != null) return pendingEntry;

            return _offsetEntries.FirstOrDefault(x => string.Compare(x.FullPath, fullPath, true) == 0);
        }

        private List<ArkEntry> GetMergedEntries()
        {
            var pending = _pendingEntries.Except(_offsetEntries);
            return _offsetEntries.Except(pending).OrderBy(x => x.FullPath).ToList();
        }

        public ArkEntry this[string fullPath] => GetArkEntry(fullPath);

        public string DirectoryName => Path.GetDirectoryName(this._arkPaths[0]);
        public string FileName => Path.GetFileName(this._arkPaths[0]);
        public string FullPath => this._arkPaths[0];

        internal string ArkPath(int index) => this._arkPaths[index];
        
        public ReadOnlyCollection<ArkEntry> Entries => new ReadOnlyCollection<ArkEntry>(GetMergedEntries());

        public bool Encrypted => this._encrypted;
        public ArkVersion Version => this._version;
        public string WorkingDirectory => this._workingDirectory;
    }
}
