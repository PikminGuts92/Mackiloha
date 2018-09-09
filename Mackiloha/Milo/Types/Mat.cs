using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha.Milo
{
    public class Mat : AbstractEntry
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
                
                if (version < 25)
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

                        // TODO: Set per texture, not material
                        mat.Mode1 = ar.ReadInt32();
                        mat.Mode2 = ar.ReadInt32();

                        ar.BaseStream.Position += 52;
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
                // BYTES(15)

                if (version == 21) // GH1
                {
                    ar.BaseStream.Position += 4;
                    mat.BaseColorR = ar.ReadSingle();
                    mat.BaseColorG = ar.ReadSingle();
                    mat.BaseColorB = ar.ReadSingle();
                    mat.BaseColorA = ar.ReadSingle();

                    ar.BaseStream.Position += 9; // Skips unknown junk
                    mat.BlendFactor = ar.ReadInt32();
                }

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
                case 25: // PS2 - GH2 (OPM demo)
                case 27: // PS2 - GH2
                    return true;
                default:
                    return false;
            }
        }

        public List<string> Textures { get; } = new List<string>();

        public int Mode1 { get; set; } = 2;
        public int Mode2 { get; set; } = 0; // 2 == reflective?

        public float BaseColorR { get; set; } = 1.0f;
        public float BaseColorG { get; set; } = 1.0f;
        public float BaseColorB { get; set; } = 1.0f;
        public float BaseColorA { get; set; } = 1.0f;
        public int BlendFactor { get; set; } = 1; // TODO: Switch to enum

        public override byte[] Data => throw new NotImplementedException();

        public override string Type => "Mat";
    }
}
