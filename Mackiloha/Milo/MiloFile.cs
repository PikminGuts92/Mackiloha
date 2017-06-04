﻿using System;
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
                        blockCompressed[i] = ar.ReadBoolean(); // "01" is uncompressed for milo_d

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
                                case BlockStructure.GZIP:
                                    block = Compression.InflateBlock(block, CompressionType.GZIP);
                                    break;
                                case BlockStructure.MILO_B:
                                    block = Compression.InflateBlock(block, CompressionType.ZLIB);
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
        public override byte[] Data { get { return null; } }
    }
}
