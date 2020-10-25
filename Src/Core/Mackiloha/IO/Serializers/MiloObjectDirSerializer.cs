using System;
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
                dirName = FileHelper.SanitizePath(ar.ReadString());

                //ar.BaseStream.Position += 8; // Skips string count + total length
                dir.Extras.Add("Num1", ar.ReadInt32());
                dir.Extras.Add("Num2", ar.ReadInt32());
            }

            int entryCount = ar.ReadInt32();
            var entries = Enumerable.Range(0, entryCount).Select(x => new
            {
                Type = ar.ReadString(),
                Name = FileHelper.SanitizePath(ar.ReadString())
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
            else if (version == 25 && dirType == "ObjectDir")
            {
                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "WorldDir"))
            {

                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();
                float unk3 = ar.ReadSingle();
                string unk4 = ar.ReadString();
                int unk5 = ar.ReadInt32();
                int unk6 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "RndDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "SynthDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "PanelDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "BackgroundDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();
                int unk3 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "TrackPanelDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();
                int unk3 = ar.ReadInt32();
                int unk4 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "H2HTrackPanelDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();
                int unk3 = ar.ReadInt32();
                int unk4 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "VocalTrackDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "UILabelDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "UIListDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "PitchArrowDir"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
            }
            else if (version == 25 && (dirType == "WorldInstance"))
            {
                // Read past unknown stuff, big hack
                int unk1 = ar.ReadInt32();
                int unk2 = ar.ReadInt32();
                int unk3 = ar.ReadInt32();

                // Hack for project 9
                var dirEntry = new MiloObjectDirEntry() { Name = dirName };
                dirEntry.Version = ar.ReadInt32();
                dirEntry.SubVersion = ar.ReadInt32();
                dirEntry.ProjectName = ar.ReadString();

                // Skip matrices + constants
                var matCount = ar.ReadInt32(); // Usually 7
                ar.BaseStream.Position += (matCount * 48) + 9;

                // Read imported milos
                var importedMiloCount = ar.ReadInt32();
                dirEntry.ImportedMiloPaths = Enumerable.Range(0, importedMiloCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Boolean, true when sub directory?
                ar.ReadBoolean();

                // Sub directory names seem to be in reverse order of serialization...
                var subDirCount = ar.ReadInt32();
                var subDirNames = Enumerable.Range(0, subDirCount)
                    .Select(x => ar.ReadString())
                    .ToArray();

                // Read subdirectories
                foreach (var _ in subDirNames.Reverse())
                {
                    var subDir = new MiloObjectDir();
                    ReadFromStream(ar, subDir);

                    dirEntry.SubDirectories.Add(subDir);
                }

                dir.Extras.Add("DirectoryEntry", dirEntry);
                ar.BaseStream.Position += 17; // 0'd data + ADDE
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
            var dir = data as MiloObjectDir;
            aw.Write((int)Magic());

            if (Magic() >= 24)
            {
                // Write extra info
                var dirEntry = dir.Extras["DirectoryEntry"] as MiloObject;
                aw.Write((string)dirEntry.Type);
                aw.Write((string)dirEntry.Name);

                /*
                var entryNameLengths = dir.Entries.Select(x => ((string)x.Name).Length).Sum();
                aw.Write(0); // Unknown
                aw.Write(1 + ((string)dirEntry.Name).Length + dir.Entries.Count + entryNameLengths);
                */

                if (dir.Extras.TryGetValue("Num1", out var num1))
                    aw.Write((int)num1);
                if (dir.Extras.TryGetValue("Num2", out var num2))
                    aw.Write((int)num2);

                aw.Write((int)dir.Entries.Count);
            }

            foreach (var entry in dir.Entries)
            {
                aw.Write((string)entry.Type);
                aw.Write((string)entry.Name);
            }

            if (Magic() >= 24)
            {
                var dirEntryRaw = dir.Extras["DirectoryEntry"] as ISerializable;

                if (Magic() == 25
                    && dir.Type == "ObjectDir" 
                    && dirEntryRaw is MiloObjectDirEntry dirEntry)
                {
                    // Hack for project 9
                    aw.Write((int)dirEntry.Version);
                    aw.Write((int)dirEntry.SubVersion);
                    aw.Write((string)dirEntry.ProjectName);

                    // Write matrices
                    aw.Write((int)7);
                    foreach (var mat in Enumerable
                        .Range(0, 7)
                        .Select(x => Matrix4.Identity()))
                    {
                        aw.Write((float)mat.M11);
                        aw.Write((float)mat.M12);
                        aw.Write((float)mat.M13);

                        aw.Write((float)mat.M21);
                        aw.Write((float)mat.M22);
                        aw.Write((float)mat.M23);

                        aw.Write((float)mat.M31);
                        aw.Write((float)mat.M32);
                        aw.Write((float)mat.M33);

                        aw.Write((float)mat.M41);
                        aw.Write((float)mat.M42);
                        aw.Write((float)mat.M43);
                    }

                    // Constants? I hope
                    aw.Write((int)0);
                    aw.Write((bool)true);
                    aw.Write((int)0);

                    // Write imported milo paths
                    if (!(dirEntry.ImportedMiloPaths is null))
                    {
                        aw.Write((int)dirEntry.ImportedMiloPaths.Length);

                        foreach (var path in dirEntry.ImportedMiloPaths)
                            aw.Write((string)path);
                    }
                    else
                    {
                        aw.Write((int)0);
                    }

                    // Might be different depending on dir being root/nested
                    // Root: false, Nested: true
                    aw.Write((bool)(dirEntry.SubDirectories.Count <= 0)); // TODO: Use a better way to determine if nested

                    // Write sub directory names
                    aw.Write((int)dirEntry.SubDirectories.Count);
                    foreach (var subName in dirEntry
                        .SubDirectories
                        .Select(x => $"{x.Name}.milo")
                        .Reverse()) // Seems to be reverse order of serialization
                        aw.Write((string)subName);

                    // Write sub directory data
                    foreach (var subDir in dirEntry.SubDirectories)
                        WriteToStream(aw, subDir);

                    aw.BaseStream.Position += 13; // Zero'd bytes
                }
                else
                {
                    MiloSerializer.WriteToStream(aw.BaseStream, dirEntryRaw);
                }

                aw.Write(ADDE_PADDING);
            }
            else if (Magic() < 24) // GH1
            {
                /*
                var texEntries = dir.Entries.Where(x => x.Type == "Tex").ToArray();
                aw.Write((int)texEntries.Count());
                aw.Write(new byte[texEntries.Length << 2]); // TODO: Write external bitmap paths?
                */

                List<string> external;

                if (dir.Extras.ContainsKey("ExternalResources"))
                {
                    external = dir.Extras["ExternalResources"] as List<string>;
                }
                else
                {
                    // TODO: Guess external resources?
                    external = new List<string>();
                }

                aw.Write((int)external.Count());
                external.ForEach(x => aw.Write((string)x));
            }

            foreach (var entry in dir.Entries)
            {
                MiloSerializer.WriteToStream(aw.BaseStream, entry as ISerializable); // TODO: Don't cast
                aw.Write(ADDE_PADDING);
            }
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

        internal override int[] ValidMagics()
        {
            return new[] { MiloSerializer.Info.Version };
        }
    }
}
