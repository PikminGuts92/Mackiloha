﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mackiloha.IO.Serializers
{
    public class MiloObjectDirSerializer : AbstractSerializer
    {
        private static readonly byte[] ADDE_PADDING = { 0xAD, 0xDE, 0xAD, 0xDE }; // Used to pad files

        public MiloObjectDirSerializer(MiloSerializer miloSerializer) : base(miloSerializer) { }

        public override void ReadFromStream(AwesomeReader ar, ISerializable data)
        {
            var dir = data as MiloObjectDir;
            int version = ReadMagic(ar, data);
            string dirType = null, dirName = null;

            dir.Extras.Clear(); // Clears for good measure
            
            if (version >= 24)
            {
                // Parses directory type/name
                dirType = ar.ReadString();
                dirName = ar.ReadString();
                ar.BaseStream.Position += 8; // Skips string count + total length
            }

            int entryCount = ar.ReadInt32();
            var entries = Enumerable.Range(0, entryCount).Select(x => new
            {
                Type = ar.ReadString(),
                Name = ar.ReadString()
            }).ToArray();

            if (version == 10)
            {
                // Parses external resource paths?
                entryCount = ar.ReadInt32();

                // Note: Entry can be empty
                var external = Enumerable.Range(0, entryCount)
                    .Select(x => ar.ReadString())
                    .ToList();

                dir.Extras.Add("ExternalResources", external);
            }
            else if (version >= 24)
            {
                // GH2 and above

                // Reads data as a byte array
                var entrySize = GuessEntrySize(ar);
                var entryBytes = new MiloObjectBytes(dirType) { Name = dirName };
                entryBytes.Data = ar.ReadBytes((int)entrySize);

                dir.Extras.Add("DirectoryEntry", entryBytes);
                ar.BaseStream.Position += 4;
            }


            foreach (var entry in entries)
            {
                var entryOffset = ar.BaseStream.Position;
                // TODO: De-serialize entries

                //try
                //{
                //    var miloEntry = ReadFromStream(ar.BaseStream, entry.Type);
                //    miloEntry.Name = entry.Name;

                //    dir.Entries.Add(miloEntry);
                //    ar.BaseStream.Position += 4; // Skips padding
                //    continue;
                //}
                //catch (Exception ex)
                //{
                //    // Catch exception and log?
                //    ar.Basestream.Position = entryOffset; // Return to start
                //}
                
                // Reads data as a byte array
                var entrySize = GuessEntrySize(ar);
                var entryBytes = new MiloObjectBytes(entry.Type) { Name = entry.Name };
                entryBytes.Data = ar.ReadBytes((int)entrySize);

                dir.Entries.Add(entryBytes);
                ar.BaseStream.Position += 4;
            }
        }

        public override void WriteToStream(AwesomeWriter aw, ISerializable data)
        {
            throw new NotImplementedException();
        }

        private long GuessEntrySize(AwesomeReader ar)
        {
            var entryOffset = ar.BaseStream.Position;
            int magic;

            do
            {
                int size = (int)ar.FindNext(ADDE_PADDING);
                if (size == -1)
                {
                    ar.BaseStream.Seek(0, SeekOrigin.End);
                    break; // End of file reached!
                }

                ar.BaseStream.Position += 4; // Skips padding

                if (ar.BaseStream.Position >= ar.BaseStream.Length)
                {
                    // EOF reached
                    break;
                }

                // Checks magic because ADDE padding can also be found in some Tex files as pixel data
                // This should reduce false positives
                magic = ar.ReadInt32();
                ar.BaseStream.Position -= 4;

            } while (magic < 0 || magic > 0xFF);

            // Calculates size and returns to start of stream
            var entrySize = ar.BaseStream.Position - (entryOffset + 4);
            ar.BaseStream.Position = entryOffset;

            return entrySize;
        }
        
        public override bool IsOfType(ISerializable data) => data is MiloObjectDir;

        public override int Magic()
        {
            switch (MiloSerializer.Info.Version)
            {
                case 6:
                    return -1;
                case 10: // GH1
                case 24: // GH2
                case 25: // RB1
                    return MiloSerializer.Info.Version;
                case 28: // RB3
                case 32: // Blitz
                default:
                    return -1;
            }
        }
    }
}