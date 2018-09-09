using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha.Milo
{
    public class Tex : AbstractEntry
    {
        public Tex(string name, bool bigEndian = true) : base(name, "", bigEndian)
        {

        }

        public static Tex FromFile(string input)
        {
            using (FileStream fs = File.OpenRead(input))
            {
                return FromStream(fs);
            }
        }

        public static Tex FromStream(Stream input)
        {
            using (AwesomeReader ar = new AwesomeReader(input))
            {
                int version;
                bool valid, useExternal = true;
                Tex tex = new Tex("");

                // Guesses endianess
                ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out version, out valid);
                if (!valid) return null; // Probably do something else later

                int idk = ar.ReadInt32();

                // Skips duplicate width, height, bpp info
                if (version < 10)
                    ar.BaseStream.Position += 8;
                else if (idk == 0)
                    ar.BaseStream.Position += 17;
                else
                    ar.BaseStream.Position += 21;
                
                tex.ExternalPath = ar.ReadString(); // Relative path

                if (version != 5)
                {
                    ar.BaseStream.Position += 8; // Skips unknown stuff
                    useExternal = ar.ReadBoolean();
                }
                else
                    ar.BaseStream.Position += 5; // Amp doesn't embed textures?

                // Parses hmx image
                //if (!useExternal)
                tex.Image = HMXImage.FromStream(ar.BaseStream);
                tex.BigEndian = ar.BigEndian;
               
                return tex;
            }
        }

        public void WriteToStream(Stream stream)
        {
            using (AwesomeWriter aw = new AwesomeWriter(stream, BigEndian))
            {
                aw.Write((int)10); // TODO: Save version
                aw.Write((int)1);

                aw.BaseStream.Position += 9;
                aw.Write((int)Image.Width);
                aw.Write((int)Image.Height);
                aw.Write((int)((Image.Encoding == ImageEncoding.DXT1) ? 4 : 8));

                //aw.Write("NO_EXTERNAL_PATH");
                aw.Write(ExternalPath);
                aw.Write((float)-8.0f);
                aw.Write((int)1);
                aw.Write((byte)0x01); // Use embedded

                aw.Write(Image.WriteToBytes());
            }
        }

        public byte[] WriteToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WriteToStream(ms);
                return ms.ToArray();
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
                case 5: // PS2 - Amp
                case 8: // PS2 - GH1
                case 10: // PS2 - GH2
                    return true;
                default:
                    return false;
            }
        }

        public string ExternalPath { get; set; }
        public bool BigEndian { get; set; }

        public HMXImage Image { get; set; }

        public override byte[] Data => throw new NotImplementedException();

        public override string Type => "Tex";
    }
}
