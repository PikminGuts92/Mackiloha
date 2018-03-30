using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha.Milo
{
    public class Mat : AbstractEntry, IExportable
    {
        public Mat(string name, bool bigEndian = true) : base(name, "", bigEndian)
        {

        }

        public static Mat FromFile(string input)
        {
            using (FileStream fs = File.OpenRead(input))
            {
                return FromStream(fs);
            }
        }
        public static Mat FromStream(Stream input)
        {
            using (AwesomeReader ar = new AwesomeReader(input))
            {
                int version;
                bool valid;
                Mat mat = new Mat("");

                // Guesses endianess
                ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out version, out valid);
                if (!valid) return null; // Probably do something else later
                
                if (version < 27)
                {
                    int textureCount = ar.ReadInt32(); // Usually between 0-2

                    for (int i = 0; i < textureCount; i++)
                    {
                        // INT32 - Unknown (2)
                        // INT32 - Unknown (0, 5)
                        // MATRIX
                        //      1.0 0.0 0.0 0.0
                        //      1.0 0.0 0.0 0.0
                        //      1.0 0.0 0.0 0.0
                        // INT32 - Either 0 or 1
                        // \____ 60 bytes ____/
                        ar.BaseStream.Position += 60;
                        mat.Textures.Add(ar.ReadString());
                    }
                }
                else
                {
                    // Just reads first texture
                    ar.BaseStream.Position += 93;
                    string name = ar.ReadString();

                    if (!string.IsNullOrEmpty(name))
                        mat.Textures.Add(name);
                }

                // INT32 - Always 3
                // FLOAT - Not sure // Four floats - Values between 0 and 1 (Transparency?)
                // FLOAT -   |
                // FLOAT -   |
                // FLOAT -  /
                // BYTES(13)

                return mat;
            }
        }

        private static bool DetermineEndianess(byte[] head, out int version, out bool valid)
        {
            bool bigEndian = false;
            version = BitConverter.ToInt32(head, 0);
            valid = IsVersionValid(version);

            checkVersion:
            if (!valid && !bigEndian)
            {
                bigEndian = !bigEndian;
                Array.Reverse(head);
                version = BitConverter.ToInt32(head, 0);
                valid = IsVersionValid(version);

                goto checkVersion;
            }

            return bigEndian;
        }

        private static bool IsVersionValid(int version)
        {
            switch (version)
            {
                case 21: // PS2 - GH1
                case 27: // PS2 - GH2
                    return true;
                default:
                    return false;
            }
        }

        public void Import(string path)
        {
            throw new NotImplementedException();
        }

        public void Export(string path)
        {
            throw new NotImplementedException();
        }

        public List<string> Textures { get; } = new List<string>();

        public override byte[] Data => throw new NotImplementedException();

        public override string Type => "Mat";
    }
}
