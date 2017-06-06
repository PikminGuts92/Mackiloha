using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PanicAttack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Mackiloha.Milo
{
    public partial class MiloFile : AbstractEntry, IExportable
    {
        private BlockStructure _structure;
        private uint _offset;
        private MiloVersion _version;
        private const uint MAX_BLOCK_SIZE = 0x8000; // 2^15

        public MiloFile() : this("", "")
        {

        }

        public MiloFile(string name, string type, bool bigEndian = true) : base(name, type, bigEndian)
        {
            _structure = BlockStructure.MILO_A;
            _offset = 2064;

            Entries = new List<AbstractEntry>();
        }

        public static MiloFile FromFile(string input)
        {
            using (FileStream fs = File.OpenRead(input))
            {
                return FromStream(fs);
            }
        }

        public static MiloFile FromStream(Stream input)
        {
            BlockStructure structure;
            uint offset = 0;

            MemoryStream ms = new MemoryStream(); // Raw milo data
            using (AwesomeReader ar = new AwesomeReader(input))
            {
                long startingOffset = ar.BaseStream.Position; // You might not be starting at 0x0.
                uint firstFour = ar.ReadUInt32();

                // Checks compression type
                switch ((BlockStructure)firstFour)
                {
                    case BlockStructure.MILO_A: // "AFDEBECA"
                    case BlockStructure.MILO_B: // "AFDEBECB"
                    case BlockStructure.MILO_C: // "AFDEBECC"
                    case BlockStructure.MILO_D: // "AFDEBECD"
                        structure = (BlockStructure)firstFour;
                        break;
                    default:
                        return null;
                }

                if (structure == BlockStructure.MILO_A || structure == BlockStructure.MILO_B
                    || structure == BlockStructure.MILO_C || structure == BlockStructure.MILO_D)
                {
                    offset = ar.ReadUInt32();

                    int blockCount = ar.ReadInt32();
                    ar.ReadInt32(); // Largest Block - Not needed

                    // Reads blocks sizes
                    int[] blockSize = new int[blockCount];
                    bool[] blockCompressed = new bool[blockCount]; // Some RB3 blocks aren't compressed
                    for (int i = 0; i < blockCount; i++)
                    {
                        blockSize[i] = ar.ReadInt24();
                        blockCompressed[i] = ar.ReadBoolean(); // "01" is uncompressed for MILO_D

                        if (structure == BlockStructure.MILO_D && blockCompressed[i])
                        {
                            blockCompressed[i] = !blockCompressed[i];
                        }
                        else if (structure == BlockStructure.MILO_A) blockCompressed[i] = false;
                        else blockCompressed[i] = true; // Type B + C + D
                    }

                    // Jumps to first block offset
                    ar.BaseStream.Position = startingOffset + offset;

                    for (int i = 0; i < blockCount; i++)
                    {
                        byte[] block = ar.ReadBytes(blockSize[i]);

                        // Decompresses block
                        if (block.Length > 0 && blockCompressed[i])
                        {
                            switch(structure)
                            {
                                case BlockStructure.MILO_B:
                                    block = Compression.InflateBlock(block, CompressionType.ZLIB);
                                    break;
                                case BlockStructure.MILO_C:
                                    block = Compression.InflateBlock(block, CompressionType.GZIP);
                                    break;
                                default: // MILO_D
                                    block = Compression.InflateBlock(block, CompressionType.ZLIB, 4);
                                    break;
                            }
                        }

                        // Writes block data to memory stream
                        ms.Write(block, 0, block.Length);
                    }
                }
            }

            MiloFile milo;
            ms.Seek(0, SeekOrigin.Begin);

            using (AwesomeReader ar = new AwesomeReader(ms))
            {
                // Parses milo directory
                milo = ParseDirectory(ar, structure, offset);
            }
            
            ms.Close();
            
            return milo;
        }

        public void ToFile(string outputPath)
        {
            FileHelper.CreateDirectoryIfNotExists(outputPath);

            using (FileStream fs = File.Create(outputPath))
            {
                ToStream(fs);
            }
        }

        public void ToStream(Stream output)
        {
            using (AwesomeWriter aw = new AwesomeWriter(output, false))
            {
                WriteToStream(aw, 0);
            }
        }

        private long WriteToStream(AwesomeWriter aw, int depth)
        {
            // TODO: Implement recursion and multi-directory writing
            long startOffset = aw.BaseStream.Position;
            long endOffset = 0;
            byte[] data = this.Data;

            // Writes just raw data if no block structure
            if (depth == 0 && _structure == BlockStructure.NONE)
            {
                aw.Write(data);
                return aw.BaseStream.Position - startOffset;
            }
            else if (depth == 0 && _structure == BlockStructure.GZIP)
            {
                aw.Write(Compression.DeflateBlock(data, CompressionType.GZIP));
                return aw.BaseStream.Position - startOffset;
            }
            
            // Milo block structure
            using (AwesomeReader ar = new AwesomeReader(new MemoryStream(data)))
            {
                List<int> blockSizes = new List<int>();
                int currentBlockSize = 0, largetBlockSize = 0;
                
                // Calculates uncompressed block sizes
                while (ar.BaseStream.Position < ar.BaseStream.Length)
                {
                    // This assumes at least embedded file entry - Fix later?
                    long nextAdde = ar.FindNext(ADDE_PADDING);
                    ar.BaseStream.Position += 4;
                    currentBlockSize += (int)nextAdde + 4;

                    if (currentBlockSize >= MAX_BLOCK_SIZE || ar.BaseStream.Position >= ar.BaseStream.Position)
                    {
                        if (currentBlockSize > largetBlockSize) largetBlockSize = currentBlockSize; // Sets larget block size

                        blockSizes.Add(currentBlockSize);
                        currentBlockSize = 0;
                    }
                }

                // Writes header (16 bytes)
                aw.Write((int)_structure);
                aw.Write(_offset);
                aw.Write(blockSizes.Count);
                aw.Write(largetBlockSize);
                aw.BaseStream.Seek(startOffset + _offset, SeekOrigin.Begin);

                ar.BaseStream.Seek(0, SeekOrigin.Begin);

                // Compresses blocks
                for (int i = 0; i < blockSizes.Count; i++)
                {
                    byte[] block = ar.ReadBytes(blockSizes[i]);

                    switch(_structure)
                    {
                        case BlockStructure.MILO_B:
                            block = Compression.DeflateBlock(block, CompressionType.ZLIB);
                            break;
                        case BlockStructure.MILO_C: // Gzip
                            block = Compression.DeflateBlock(block, CompressionType.GZIP);
                            break;
                        case BlockStructure.MILO_D:
                            byte[] size = BitConverter.GetBytes(block.Length);
                            byte[] temp = Compression.DeflateBlock(block, CompressionType.ZLIB);

                            block = new byte[temp.Length + 4];
                            Array.Copy(size, 0, block, 0, size.Length);
                            Array.Copy(temp, 0, block, size.Length, block.Length - size.Length);
                            break;
                    }

                    // Updates block size
                    blockSizes[i] = block.Length;

                    // Writes block
                    aw.Write(block);
                }

                // Updates end offset
                endOffset = aw.BaseStream.Position;

                // Writes block sizes in header
                aw.BaseStream.Seek(startOffset + 16, SeekOrigin.Begin);
                foreach(int blockSize in blockSizes)
                {
                    aw.Write(blockSize);
                }
            }
            
            return endOffset - startOffset;
        }

        public void Import(string path)
        {
            //throw new NotImplementedException();
        }

        public void Export(string path)
        {
            string extractPath = $@"{FileHelper.RemoveExtension(path)}.extracted";

            dynamic json = new JObject();
            json.FileType = "MiloFile";
            json.Structure = JsonConvert.SerializeObject(_structure, new StringEnumConverter()).Replace("\"", "");
            json.BlockOffset = _offset;
            json.Version = _version;
            
            if ((uint)_version > 10)
            {
                json.DirectoryName = Name;
                json.DirectoryType = Type;
            }

            json.ExtractDirectory = FileHelper.GetFileName(extractPath);

            // Writes entries
            JArray array = new JArray();
            foreach(AbstractEntry entry in Entries)
            {
                string entryPath = $@"Entries\{entry.Type}\{entry.Name}";

                // Adds to json file
                dynamic jsonEntry = new JObject();
                jsonEntry.Name = entry.Name;
                jsonEntry.Type = entry.Type;
                jsonEntry.ExtractPath = entryPath;
                
                // Checks if entry is IExportable
                if (entry is IExportable)
                {
                    string fullEntryPath = $@"{extractPath}\{entryPath}.json";
                    FileHelper.CreateDirectoryIfNotExists(fullEntryPath);

                    // Exports entry
                    IExportable export = entry as IExportable;
                    export.Export(fullEntryPath);
                    jsonEntry.Exported = true;
                }
                else
                {
                    // Writes raw bytes
                    string fullEntryPath = $@"{extractPath}\{entryPath}";
                    FileHelper.CreateDirectoryIfNotExists(fullEntryPath);
                    File.WriteAllBytes(fullEntryPath, entry.Data);
                }

                // Adds entry
                array.Add(jsonEntry);
            }

            json.Entries = array;

            File.WriteAllText(path, json.ToString());
        }

        /// <summary>
        /// Gets or sets block structure
        /// </summary>
        public BlockStructure Structure
        {
            get
            {
                return _structure;
            }
            set
            {
                switch (value)
                {
                    case BlockStructure.NONE:
                    case BlockStructure.GZIP:
                    case BlockStructure.MILO_A:
                    case BlockStructure.MILO_B:
                    case BlockStructure.MILO_C:
                    case BlockStructure.MILO_D:
                        _structure = value;
                        return;
                }
            }
        }

        /// <summary>
        /// Gets or sets milo directory version
        /// </summary>
        public MiloVersion Version
        {
            get
            {
                return _version;
            }
            set
            {
                if (IsVersionValid(value)) _version = value;
            }
        }

        public List<AbstractEntry> Entries { get; }
        public override byte[] Data => CreateData();

        private byte[] CreateData()
        {
            using (AwesomeWriter aw = new AwesomeWriter(new MemoryStream(), BigEndian))
            {
                aw.Write((int)_version);
                // TODO: Implement directory type + name writing

                aw.Write(Entries.Count);

                // Writes entry type + name
                foreach(AbstractEntry entry in Entries)
                {
                    aw.Write(entry.Type);
                    aw.Write(entry.Name);
                }

                if (_version == MiloVersion.V10) aw.Write((int)0);

                // Writes data from each entry
                foreach(AbstractEntry entry in Entries)
                {
                    aw.Write(entry.Data);
                    aw.Write(ADDE_PADDING);
                }

                return ((MemoryStream)(aw.BaseStream)).ToArray();
            }
        }
    }
}
