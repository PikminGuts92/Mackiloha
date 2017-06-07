﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PanicAttack;

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
        Bitmap _bmp;

        private HMXImage(Bitmap bmp)
        {
            _bmp = bmp;
        }

        private HMXImage(int width, int height)
        {
            _bmp = new Bitmap(width, height);
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

            using (AwesomeReader ar = new AwesomeReader(input))
            {
                ImageEncoding encoding;
                bool valid;
                uint bpp, width, height, bpl;

                if (ar.ReadByte() != 0x01) return null; // Should always be 1!
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

                // Guesses endianess
                ar.BigEndian = DetermineEndianess(ar.ReadBytes(4), out encoding, out valid);
                if (!valid) return null; // Maybe do something else later

                // Reads rest of header
                ar.ReadByte(); // Mipmap count
                width = ar.ReadUInt16();
                height = ar.ReadUInt16();
                bpl = ar.ReadUInt16();
                ar.BaseStream.Position += 19;

                // Decodes image
                Bitmap bmp = Decode(ar, encoding, bpp, width, height, bpl);
                HMXImage image = new HMXImage(bmp);
                image.Encoding = encoding;

                return image;
            }
        }

        public void SaveAs(string path)
        {
            _bmp.Save(path, ImageFormat.Png);
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

        public int Width => _bmp.Width;

        public int Height => _bmp.Height;
    }
}