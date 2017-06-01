using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PanicAttack;

namespace Mackiloha.Milo
{
    public partial class MiloFile
    {
        private static MiloFile ParseDirectory(AwesomeReader ar, BlockStructure structure, uint offset)
        {
            bool origBigEndian = ar.BigEndian; // Used to preserve orig stream
            MiloFile milo;
            MiloVersion version;
            bool valid;
            string dirName, dirType;
            List<string> entryNames, entryTypes;

            // Guesses endianess
            ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out version, out valid);
            if (!valid) return null; // Maybe do something else later

            ParseEntryNames(ar, version, out dirName, out dirType, out entryNames, out entryTypes);
            milo = new MiloFile(dirName, dirType, ar.BigEndian);
            milo._structure = structure;
            milo._offset = offset;
            // TODO: Add component parser (Difficult)
            // TODO: Add entry parser
            
            return milo;
        }

        private static void ParseEntryNames(AwesomeReader ar, MiloVersion version, out string dirName, out string dirType, out List<string> names, out List<string> types)
        {
            dirName = dirType = ""; // Only used on versions 24+
            names = new List<string>();
            types = new List<string>();
            int count;
            
            if ((int)version >= 24)
            {
                // Parse directory name + type
                dirType = ar.ReadString();
                dirName = ar.ReadString();
                ar.BaseStream.Position += 8; // Skips weird counts
            }

            count = ar.ReadInt32();

            while (count >= 0)
            {
                // Reads entry name + type
                types.Add(ar.ReadString());
                names.Add(ar.ReadString());

                count--;
            }
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
