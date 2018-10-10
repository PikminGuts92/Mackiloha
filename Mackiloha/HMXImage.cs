using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ImageMagick;

namespace Mackiloha
{
    public enum ImageEncoding
    {
        BMP = 0x03, // PS2
        //XBX = 0x08, // Xbox OG (8bpp - color palette)
        DXT1 = 0x08,
        //GC = 0x10, // GC (8bpp)
        DXT5 = 0x18,
        //GC = 0x20, // GC (16bpp) RGB565?
        ATI2 = 0x20,
        //TPL = 0x48 // Wii (4bpp)
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

                switch (bpp)
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

                ar.BaseStream.Position += (firstByte == 1) ? 19 : 6;

                // Decodes image
                var magic = Decode(ar, encoding, bpp, mipmap, width, height, bpl, firstByte == 1);
                HMXImage image = new HMXImage(magic);
                image.Encoding = encoding;
                image.BigEndian = ar.BigEndian;

                return image;
            }
        }

        public void ImportImageFromFile(string path)
        {
            var newImage = new MagickImage(path);
            _image = newImage;
        }

        public void WriteToStream(Stream stream)
        {
            var uniqueColors = _image.TotalColors;
            int bpp, bpl, mipmap = GetMipMapCount();
            MagickFormat format;

            switch (Encoding)
            {
                default:
                case ImageEncoding.BMP:
                    if (uniqueColors <= 16)
                        bpp = 4;
                    else if (uniqueColors <= 256)
                        bpp = 8;
                    else
                        bpp = 32;
                    
                    bpl = (Width * bpp) / 8;
                    format = MagickFormat.Png; // Ignore
                    break;
                case ImageEncoding.DXT1:
                    bpp = 4;
                    bpl = (Width * bpp) / 8;
                    format = MagickFormat.Dxt1;
                    break;
                case ImageEncoding.DXT5:
                case ImageEncoding.ATI2:
                    bpp = 8;
                    bpl = (Width * bpp) / 8;
                    format = MagickFormat.Dxt5;
                    break;
            }
            
            using (AwesomeWriter aw = new AwesomeWriter(stream, BigEndian))
            {
                // Writes header
                aw.Write((byte)0x01);
                aw.Write((byte)bpp);
                aw.Write((int)Encoding);
                aw.Write((byte)mipmap);
                aw.Write((short)Width);
                aw.Write((short)Height);
                aw.Write((short)bpl);
                aw.BaseStream.Position += 19; // Zeros

                var imgBytes = GetRawBytes(_image, Width, Height, bpp, mipmap, format);
                aw.Write(imgBytes);
            }
        }

        private byte[] GetRawBytes(MagickImage image, int width, int height, int bpp, int mipMap, MagickFormat format)
        {
            var sizes = new List<int>();

            int CalcSize(int w, int h, int b) => (w * h * b) / 8;
            int CalcTotalSize(int w, int h, int b, int m)
            {
                if (m <= 0)
                    return CalcSize(w, h, b);

                return CalcSize(w, h, b) + CalcTotalSize(w >> 1, h >> 1, b, --m);
            };

            void CalcSizes(int w, int h, int b, int m)
            {
                sizes.Add(CalcSize(w, h, b));
                if (m > 0)
                    CalcSizes(w >> 1, h >> 1, b, --m);
            }
            
            CalcSizes(width, height, bpp, mipMap);
            var totalSize = sizes.Sum();
            var fullData = new byte[totalSize];
            var ddsBytes = image.ToByteArray(format);

            int offset = 128;
            int currentIdx = 0;

            foreach (var size in sizes)
            {
                Array.Copy(ddsBytes, offset + currentIdx, fullData, currentIdx, size);
                currentIdx += size;
            }
            
            SwapBytes(fullData);
            return fullData;
        }

        private int GetMipMapCount()
        {
            if (Encoding == ImageEncoding.BMP)
                return 0;

            int min = Math.Min(Width, Height);
            int mips = 0;

            while (min >= 32)
            {
                mips++;
                min >>= 1;
            }

            return mips;
        }

        public byte[] WriteToBytes()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                WriteToStream(ms);
                return ms.ToArray();
            }
        }

        public MagickImage Image
        {
            get => new MagickImage(_image);
            set => _image = value;
        }

        //public Bitmap Bitmap => new Bitmap(new MemoryStream(_image.ToByteArray(MagickFormat.Png))); // TODO: Improve this

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

        public bool BigEndian { get; set; }
        public ImageEncoding Encoding { get; set; }

        public int Width => _image.Width;

        public int Height => _image.Height;
    }
}
