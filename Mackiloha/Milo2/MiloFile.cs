using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mackiloha.Milo2
{
    public class MiloFile
    {
        private const uint MAX_BLOCK_SIZE = 0x8000; // 2^15
        private const uint ADDE_PADDING = 0xADDEADDE;

        private BlockStructure _structure;
        private uint _offset;
        private MiloVersion _version;
        private MiloEntry _directoryEntry;
        
        public MiloFile()
        {
            _structure = BlockStructure.MILO_B;
            _offset = 2064;
            _version = MiloVersion.V24;
            Entries = new List<IMiloEntry>();
        }

        public MiloFile(MiloEntry directory) : this()
        {
            _directoryEntry = directory;
        }

        public static MiloFile ReadFromFile(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                return ReadFromStream(new AwesomeReader(fs, false));
            }
        }

        public static MiloFile ReadFromStream(AwesomeReader ar)
        {
            long startingOffset = ar.BaseStream.Position; // You might not be starting at 0x0
            var structureType = (BlockStructure)ar.ReadUInt32();

            if (structureType != BlockStructure.MILO_A)
                throw new Exception("Unsupported milo compression");

            int offset = ar.ReadInt32(); // Start of blocks
            int blockCount = ar.ReadInt32();
            int maxBlockSize = ar.ReadInt32(); // Skips max uncompressed size

            // Reads block sizes
            int totalSize = Enumerable.Range(0, blockCount).Select(x => ar.ReadInt32()).Sum();

            // Jumps to first block offset
            ar.BaseStream.Position = startingOffset + offset;

            using (var ms = new MemoryStream())
            {
                // Copies raw milo data
                ar.BaseStream.CopyTo(ms, totalSize);
                ms.Seek(0, SeekOrigin.Begin);
                return ParseDirectory(new AwesomeReader(ms, true));
            }
        }

        private static MiloFile ParseDirectory(AwesomeReader ar)
        {
            int version = ar.ReadInt32();

            if (!(version == 25 || version == 28)) // RBN1 and RBN2 milos
                throw new Exception($"Milo directory version of {version} is not supported");

            string dirType = ar.ReadString(), dirName = ar.ReadString();
            
            ar.BaseStream.Position += 8; // Skips string count + total length

            // Reads entry types/names
            int count = ar.ReadInt32();
            string[] types = new string[count];
            string[] names = new string[count];

            for (int i = 0; i < count; i++)
            {
                types[i] = ar.ReadString();
                names[i] = ar.ReadString();
            }

            // Skips unknown data
            var next = FindNext(ar, ADDE_PADDING);

            MiloFile milo = new MiloFile()
            {
                _version = (MiloVersion)version,
                _directoryEntry = new MiloEntry(dirName, dirType, ar.ReadBytes(next))
            };

            ar.BaseStream.Seek(4, SeekOrigin.Current);

            // Reads each file
            for (int i = 0; i < names.Length; i++)
            {
                long start = ar.BaseStream.Position;
                int size = FindNext(ar, ADDE_PADDING);
                byte[] data = ar.ReadBytes(size);
                ar.BaseStream.Position += 4;

                milo.Entries.Add(new MiloEntry(names[i], types[i], data));
            }

            return milo;
        }

        private static int FindNext(AwesomeReader ar, uint magic)
        {
            long start = ar.BaseStream.Position, currentPosition = ar.BaseStream.Position;
            uint currentMagic = 0;

            while (magic != currentMagic)
            {
                if (ar.BaseStream.Position == ar.BaseStream.Length)
                {
                    // Couldn't find it
                    ar.BaseStream.Seek(start, SeekOrigin.Begin);
                    return -1;
                }

                currentMagic = (uint)((currentMagic << 8) | ar.ReadByte());
                currentPosition++;
            }

            ar.BaseStream.Seek(start, SeekOrigin.Begin);
            return (int)((currentPosition - 4) - start);
        }

        public List<IMiloEntry> Entries { get; }
    }
}
