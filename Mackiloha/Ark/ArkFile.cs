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
    public class ArkFile : IEnumerable<ArkEntry>
    {
        private ArkVersion _version;
        private string _hdrPath;
        private bool _encrypted;
        private string[] _parts;
        private ArkEntry[] _entries;

        private ArkFile() { }

        public static ArkFile FromFile(string input)
        {
            if (input == null) throw new ArgumentNullException();
            string ext = Path.GetExtension(input).ToLower(); // TODO: Do something with this

            using (FileStream fs = File.OpenRead(input))
            {
                return ParseHeader(input, fs);
            }
        }

        private static ArkFile ParseHeader(string input, Stream stream)
        {
            ArkFile ark = new ArkFile();
            ark._version = ArkVersion.V3;
            ark._hdrPath = input;
            ark._encrypted = false;

            using (AwesomeReader ar = new AwesomeReader(stream))
            {
                // TODO: Check version number (Assume V3 for now)
                int version = ar.ReadInt32();
                uint arkFileCount = ar.ReadUInt32();
                uint arkFileSizeCount = ar.ReadUInt32(); // Should be same as ark file count

                uint[] partSizes = new uint[arkFileSizeCount];
                ark._parts = GetPartNames(ark._hdrPath, partSizes.Length);

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
                ark._entries = new ArkEntry[ar.ReadUInt32()];

                for (int i = 0; i < ark._entries.Length; i++)
                {
                    uint entryOffset = ar.ReadUInt32();
                    string filePath = strings[stringIndex[ar.ReadInt32()]];
                    string direPath = strings[stringIndex[ar.ReadInt32()]];
                    uint size = ar.ReadUInt32();
                    uint inflatedSize = ar.ReadUInt32();

                    ark._entries[i] = new ArkEntry(ark, entryOffset, filePath, direPath, size, inflatedSize);
                }
            }

            return ark;
        }

        private static string[] GetPartNames(string hdrPath, int count)
        {
            string directory = Path.GetDirectoryName(hdrPath).Replace("\\", "/");
            string fileName = Path.GetFileNameWithoutExtension(hdrPath);
            string extension = Path.GetExtension(hdrPath);

            string[] arkPaths = new string[count];

            for (int i = 0; i < count; i++)
                arkPaths[i] = $"{directory}/{fileName}_{i}{extension}";

            return arkPaths;
        }

        public ArkEntry this[string fullPath] => this._entries.FirstOrDefault(x => string.Compare(x.FullPath, fullPath, true) == 0);

        public IEnumerator<ArkEntry> GetEnumerator()
        {
            return ((IEnumerable<ArkEntry>)_entries).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ArkEntry>)_entries).GetEnumerator();
        }

        public ReadOnlyCollection<ArkEntry> Entries => new ReadOnlyCollection<ArkEntry>(this._entries);

        public bool Encrypted => this._encrypted;
        public ArkVersion Version => this._version;
    }
}
