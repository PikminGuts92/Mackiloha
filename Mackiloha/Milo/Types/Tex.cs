using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
                bool valid, useExternal = true;
                Tex tex = new Tex("");

                // Guesses endianess
                ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out version, out valid);
                if (!valid) return null; // Probably do something else later
                
                // Parses tex header
                ar.BaseStream.Position += 12; // Skips duplicate width, height, bpp info
                tex.ExternalPath = ar.ReadString(); // Relative path

                if (version != 5)
                {
                    ar.BaseStream.Position += 8; // Skips unknown stuff
                    useExternal = ar.ReadBoolean();
                }
                else
                    ar.BaseStream.Position += 5; // Amp doesn't embed textures?

                // Parses hmx image
                if (!useExternal) tex.Image = HMXImage.FromStream(ar.BaseStream);
               
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
                case 5: // PS2 - Amp
                case 8: // PS2 - GH1
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
            json.FileType = Type;
            json.ExternalPath = ExternalPath;

            if (Image != null)
            {
                json.Encoding = JsonConvert.SerializeObject(Image.Encoding, new StringEnumConverter()).Replace("\"", "");

                // Exports image as PNG
                string pngPath = $@"{FileHelper.RemoveExtension(path)}.png";
                Image.SaveAs(pngPath);
                json.Png = FileHelper.GetFileName(pngPath);
            }

            File.WriteAllText(path, json.ToString());
        }

        public string ExternalPath { get; set; }

        public HMXImage Image { get; set; }

        public override byte[] Data => throw new NotImplementedException();

        public override string Type => "Tex";
    }
}
