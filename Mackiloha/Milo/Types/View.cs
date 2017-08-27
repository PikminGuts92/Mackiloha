using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha.Milo
{
    public enum ViewVersion : int
    {
        GH1 = 7
    }

    public class View : AbstractEntry
    {
        private ViewVersion _version;
        private Matrix _mat1, _mat2;

        public View(string name, bool bigEndian = true) : base(name, "View", bigEndian)
        {

        }

        public static View FromFile(string input)
        {
            using (FileStream fs = File.OpenRead(input))
            {
                return FromStream(fs);
            }
        }

        public static View FromStream(Stream input)
        {
            View view = new View("");

            using (AwesomeReader ar = new AwesomeReader(input))
            {
                bool valid;

                // Guesses endianess
                ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out view._version, out valid);
                if (!valid) return null;

                // Skips 12 zero'd bytes + 8 constant
                ar.BaseStream.Position += 16;

                // Reads in matrix tables
                view._mat1 = Matrix.FromStream(ar);
                view._mat2 = Matrix.FromStream(ar);

                // Reads sub mesh strings
                if (view._version == ViewVersion.GH1)
                {
                    string[] submeshes = new string[ar.ReadUInt32()];
                    for (int i = 0; i < submeshes.Length; i++) submeshes[i] = ar.ReadString();
                }

                // Skips unknown stuff
                ar.BaseStream.Position += 9;
                ar.ReadString(); // Reads view

                // Skipping these other mesh strings
                ar.BaseStream.Position += 5;
                uint meshCount = ar.ReadUInt32();
                for (int i = 0; i < meshCount; i++) ar.ReadString();
                ar.BaseStream.Position += 16; // Four floats - Quaternions?

                ar.ReadString(); // View (again)
                ar.ReadInt32();
                ar.ReadInt32();
            }

            return view;
        }

        private static bool DetermineEndianess(byte[] head, out ViewVersion version, out bool valid)
        {
            bool bigEndian = false;
            version = (ViewVersion)BitConverter.ToInt32(head, 0);
            valid = IsVersionValid(version);

            checkVersion:
            if (!valid && !bigEndian)
            {
                bigEndian = !bigEndian;
                Array.Reverse(head);
                version = (ViewVersion)BitConverter.ToInt32(head, 0);
                valid = IsVersionValid(version);

                goto checkVersion;
            }

            return bigEndian;
        }

        private static bool IsVersionValid(ViewVersion version)
        {
            switch (version)
            {
                case ViewVersion.GH1: // PS2 - GH1
                    return true;
                default:
                    return false;
            }
        }

        public override byte[] Data => throw new NotImplementedException();
    }
}
