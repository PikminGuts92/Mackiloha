using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha.Milo
{
    public partial class MiloFile
    {
        private static byte[] ADDE_PADDING = { 0xAD, 0xDE, 0xAD, 0xDE }; // Used to pad files

        private static MiloFile ParseDirectory(AwesomeReader ar, BlockStructure structure, uint offset)
        {
            bool origBigEndian = ar.BigEndian; // Used to preserve orig stream
            MiloFile milo;
            MiloVersion version;
            bool valid;
            string dirName, dirType;
            string[] entryNames, entryTypes;

            // Guesses endianess
            ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out version, out valid);
            if (!valid) return null; // Maybe do something else later

            ParseEntryNames(ar, version, out dirName, out dirType, out entryNames, out entryTypes);
            milo = new MiloFile(dirName, dirType, ar.BigEndian);
            milo._structure = structure;
            milo._offset = offset;
            milo._version = version;

            // TODO: Add component parser (Difficult)
            if (version == MiloVersion.V10)
                milo._externalResources = new List<string>(GetExternalResources(ar));
            else if (version == MiloVersion.V24)
            {
                /*
                if (dirName == "alterna1") // Hacky fix, please remove!
                {
                    ar.BaseStream.Position += 117;
                    Trans tran = new Trans(dirName);

                    // Reads view matrices
                    tran.Mat1 = Matrix.FromStream(ar);
                    ar.BaseStream.Position += 4;
                    tran.Mat2 = Matrix.FromStream(ar);
                    ar.BaseStream.Position += 4;

                    milo.Entries.Add(tran);
                }
                else if (dirName == "dancer1") // Hacky fix, please remove!
                {
                    ar.BaseStream.Position += 31;
                    Trans tran = new Trans(dirName);

                    // Reads view matrices
                    tran.Mat1 = Matrix.FromStream(ar);
                    ar.BaseStream.Position += 4;
                    tran.Mat2 = Matrix.FromStream(ar);
                    ar.BaseStream.Position += 4;

                    milo.Entries.Add(tran);
                }*/

                // Skips unknown stuff for now
                ar.FindNext(ADDE_PADDING);
                ar.BaseStream.Position += 4;
            }
            
            // Reads each file
            for (int i = 0; i < entryNames.Length; i++)
            {
                long start = ar.BaseStream.Position;
                int size = (int)(ar.FindNext(ADDE_PADDING));
                byte[] bytes;

                // Reads raw file bytes
                ar.BaseStream.Position = start;
                bytes = ar.ReadBytes(size);
                ar.BaseStream.Position += 4; // Jumps ADDE padding

                switch (entryTypes[i])
                {
                    case "Tex":
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            AbstractEntry entry = Tex.FromStream(ms);
                            if (entry == null) goto defaultCase;

                            entry.Name = entryNames[i];
                            milo.Entries.Add(entry);
                        }
                        break;
                    case "Mesh":
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            AbstractEntry entry = Mesh.FromStream(ms);
                            if (entry == null) goto defaultCase;

                            entry.Name = entryNames[i];
                            milo.Entries.Add(entry);
                        }
                        break;
                    case "View":
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            AbstractEntry entry = View.FromStream(ms);
                            if (entry == null) goto defaultCase;

                            entry.Name = entryNames[i];
                            milo.Entries.Add(entry);
                        }
                        break;
                    case "Group":
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            AbstractEntry entry = View.FromStreamAsGroup(ms);
                            if (entry == null) goto defaultCase;

                            entry.Name = entryNames[i];
                            milo.Entries.Add(entry);
                        }
                        break;
                    case "Mat":
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            AbstractEntry entry = Mat.FromStream(ms);
                            if (entry == null) goto defaultCase;

                            entry.Name = entryNames[i];
                            milo.Entries.Add(entry);
                        }
                        break;
                    case "Trans":
                        using (MemoryStream ms = new MemoryStream(bytes))
                        {
                            AbstractEntry entry = Trans.FromStream(ms);
                            if (entry == null) goto defaultCase;

                            entry.Name = entryNames[i];
                            milo.Entries.Add(entry);
                        }
                        break;
                    default:
                        defaultCase:
                        milo.Entries.Add(new MiloEntry(entryNames[i], entryTypes[i], bytes, milo.BigEndian));
                        break;
                }

                

                /* TODO: Implement milo files as entries
                if (type[i] == "ObjectDir" || type[i] == "MoveDir")
                {
                    // Directory embedded as an entry
                    // Skips over redundant directory info
                    ar.BaseStream.Position += 4;
                    dir.Entries.Add(MiloFile.FromStream(ar));
                }
                else
                {
                    // Regular entry
                    ar.BaseStream.Position = start;
                    dir.Entries.Add(new MEntry(name[i], type[i], ar.ReadBytes(size)));
                    ar.BaseStream.Position += 4;
                } */
            }

            ar.BigEndian = origBigEndian;
            return milo;
        }

        private static void ParseEntryNames(AwesomeReader ar, MiloVersion version, out string dirName, out string dirType, out string[] names, out string[] types)
        {
            dirName = dirType = ""; // Only used on versions 24+
            int count;
            
            if ((int)version >= 24)
            {
                // Parse directory name + type
                dirType = ar.ReadString();
                dirName = ar.ReadString();
                ar.BaseStream.Position += 8; // Skips weird counts
            }

            count = ar.ReadInt32();
            names = new string[count];
            types = new string[count];

            for (int i = 0; i < count; i++)
            {
                // Reads entry name + type
                types[i] = ar.ReadString();
                names[i] = ar.ReadString();
            }
        }

        private static string[] GetExternalResources(AwesomeReader ar)
        {
            string[] res = new string[ar.ReadUInt32()];

            // Mostly zero'd
            for (int i = 0; i < res.Length; i++)
            {
                uint charCount = ar.ReadUInt32();

                // Reads string if not some outrageous number
                if (charCount < 0xFFFF)
                {
                    ar.BaseStream.Position -= 4;
                    res[i] = ar.ReadString();
                }
            }

            return res;
        }

        private static bool DetermineEndianess(byte[] head, out MiloVersion version, out bool valid)
        {
            bool bigEndian = false;
            version = (MiloVersion)BitConverter.ToInt32(head, 0);
            valid = IsVersionValid(version);

            checkVersion:
            if (!valid && !bigEndian)
            {
                bigEndian = !bigEndian;
                Array.Reverse(head);
                version = (MiloVersion)BitConverter.ToInt32(head, 0);
                valid = IsVersionValid(version);

                goto checkVersion;
            }
            
            return bigEndian;
        }

        private static bool IsVersionValid(MiloVersion version)
        {
            switch (version)
            {
                case MiloVersion.V6:  // FreQ
                case MiloVersion.V10: // Amp/KR/GH1
                case MiloVersion.V24: // GH2(PS2)
                case MiloVersion.V25: // RB/GH2(X360)
                case MiloVersion.V28: // RB3
                case MiloVersion.V32: // RBB/DC3
                    return true;
                default:
                    return false;
            }
        }
    }
}
