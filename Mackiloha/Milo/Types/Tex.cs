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
    public class Tex : AbstractEntry, IExportable
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
                bool valid;
                Tex tex = new Tex("");

                // Guesses endianess
                ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out version, out valid);

                if (!valid) return null; // Probably do something else later

                checkVersion:
                if (version == 8 || version == 9)
                {
                    if (version == 9)
                    {
                        ar.BaseStream.Position += version << 2;
                        version = ar.ReadInt32();
                        goto checkVersion;
                    }
                    
                    // Parses tex header
                    ar.BaseStream.Position += 12; // Skips duplicate width, height, bpp info
                    tex.ExternalPath = ar.ReadString();
                    ar.BaseStream.Position += 9; // Skips unknown stuff

                    // Parses hmx image
                    tex.Image = HMXImage.FromStream(ar.BaseStream);
                }
                else
                    return null;

                return tex;
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
                case 8: // PS2
                case 9: // PS2 - Special case?
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
            dynamic json = new JObject();
            json.FileType = "Tex";
            json.ExternalPath = ExternalPath;
            json.Encoding = JsonConvert.SerializeObject(Image.Encoding, new StringEnumConverter()).Replace("\"", "");

            // Exports image as PNG
            string pngPath = $@"{FileHelper.RemoveExtension(path)}.png";
            Image.SaveAs(pngPath);
            json.Png = FileHelper.GetFileName(pngPath);

            File.WriteAllText(path, json.ToString());
        }

        public string ExternalPath { get; set; }

        public HMXImage Image { get; set; }

        public override byte[] Data => throw new NotImplementedException();

        public override string Type => "Tex";
    }
}
