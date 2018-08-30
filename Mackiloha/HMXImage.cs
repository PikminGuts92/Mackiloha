using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ImageMagick;

namespace Mackiloha
{
    public enum ImageEncoding
    {
        BMP = 0x03, // PS2
        DXT1 = 0x08,
        DXT5 = 0x18,
        ATI2 = 0x20
    }

    public partial class HMXImage
    {
        // HMX: "Powerful and relatively insensitive nitroamine high explosive."
        private MagickImage _image;

        private HMXImage(MagickImage image)
        {
            _image = image;
        }

        private HMXImage(int width, int height)
        {
            _image = new MagickImage(MagickColor.FromRgb(0, 0, 0), width, height);
        }

        public static HMXImage FromFile(string input)
        {
            using (FileStream fs = File.OpenRead(input))
            {
                return FromStream(fs);
            }
        }

        public static HMXImage FromStream(Stream input)
        {
            /* Header size = 32 bytes
             * ======================
             * BYTE  - Always 1
             * BYTE  - Bits Per Pixel - Can be 4, 8, 24, or 32 (But usually either 4 or 8)
             * INT32 - Image Format/Encoding
             * BYTE  - MipMaps count
             * INT16 - Width
             * INT16 - Height
             * INT16 - Bytes Per Line
             * BYTES - 19 bytes of zero'd data
             */

            /* Header size = 16 bytes
             * ======================
             * BYTE  - Always 0
             * BYTE  - Bits Per Pixel - Can be 4, 8, 24, or 32 (But usually either 4 or 8)
             * BYTE  - Always 0 (Image Format/Encoding)
             * BYTE  - Awlays 0 MipMaps count
             * INT16 - Width
             * INT16 - Height
             * INT16 - Bytes Per Line
             * BYTES - 6 bytes of zero'd data
             */
            if (input.Position == input.Length) return null; // End of stream;

            using (AwesomeReader ar = new AwesomeReader(input))
            {
                ImageEncoding encoding;
                bool valid;
                uint bpp, width, height, bpl, mipmap;

                byte firstByte = ar.ReadByte();

                if (firstByte != 0 && firstByte != 1)
                    return null;
                
                bpp = ar.ReadByte();

                switch(bpp)
                {
                    case 4:
                    case 8:
                    case 24:
                    case 32:
                        break;
                    default:
                        return null; // Probably should do something else
                }

                if (firstByte == 1)
                {
                    // Guesses endianess
                    ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out encoding, out valid);
                    if (!valid) return null; // Maybe do something else later

                    // Reads rest of header
                    mipmap = ar.ReadByte(); // Mipmap count
                }
                else
                {
                    // Xbox OG texture
                    encoding = ImageEncoding.BMP;
                    ar.BaseStream.Position += 2;
                    mipmap = 0;
                }

                width = ar.ReadUInt16();
                height = ar.ReadUInt16();
                bpl = ar.ReadUInt16();
                
                ar.BaseStream.Position += (firstByte == 1) ?  19 : 6;

                // Decodes image
                var magic = Decode(ar, encoding, bpp, mipmap, width, height, bpl, firstByte == 1);
                HMXImage image = new HMXImage(magic);
                image.Encoding = encoding;

                return image;
            }
        }

        public MagickImage Image => new MagickImage(_image);

        public Bitmap Bitmap => _image.ToBitmap(); // TODO: Improve this
        
        public IntPtr Hbitmap => _image.ToBitmap().GetHbitmap();

        public void SaveAs(string path)
        {
            _image.Write(path, MagickFormat.Png);
        }

        private static bool DetermineEndianess(byte[] head, out ImageEncoding encoding, out bool valid)
        {
            bool bigEndian = false;
            encoding = (ImageEncoding)BitConverter.ToInt32(head, 0);
            valid = IsVersionValid(encoding);

            checkVersion:
            if (!valid && !bigEndian)
            {
                bigEndian = !bigEndian;
                Array.Reverse(head);
                encoding = (ImageEncoding)BitConverter.ToInt32(head, 0);
                valid = IsVersionValid(encoding);

                goto checkVersion;
            }

            return bigEndian;
        }

        private static bool IsVersionValid(ImageEncoding encoding)
        {
            switch (encoding)
            {
                case ImageEncoding.BMP:
                case ImageEncoding.DXT1:
                case ImageEncoding.DXT5:
                case ImageEncoding.ATI2:
                    return true;
                default:
                    return false;
            }
        }

        public ImageEncoding Encoding { get; set; }

        public int Width => _image.Width;

        public int Height => _image.Height;
    }
}
