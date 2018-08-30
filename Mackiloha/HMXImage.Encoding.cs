using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using ImageMagick;

namespace Mackiloha
{
    public partial class HMXImage
    {
        private static MagickImage Decode(AwesomeReader ar, ImageEncoding encoding, uint bpp, uint mipmap, uint width, uint height, uint bpl, bool ps2Texture = false)
        {
            // Image starts at bottom left corner

            switch (encoding)
            {
                case ImageEncoding.BMP:
                    return DecodeBMP(ar, bpp, width, height, bpl);
                case ImageEncoding.DXT1:
                case ImageEncoding.DXT5:
                case ImageEncoding.ATI2:
                    return DecodeDXT(ar, mipmap, width, height, encoding);
            }
            
            return null;
        }

        private static MagickImage DecodeDXT(AwesomeReader ar, uint mipmap, uint width, uint height, ImageEncoding encoding, bool x360 = true)
        {
            uint imageSize;
            string compression;

            switch (encoding)
            {
                default:
                case ImageEncoding.DXT1:
                    imageSize = (width * height) / 2; // 4bpp
                    compression = "DXT1";
                    break;
                case ImageEncoding.DXT5:
                    imageSize = width * height; // 8bpp
                    compression = "DXT5";
                    break;
                case ImageEncoding.ATI2:
                    imageSize = width * height; // 8bpp
                    compression = "ATI2";
                    break;
            }

            byte[] dds = new byte[128 + imageSize];

            // Writes DDS file
            using (MemoryStream ms = new MemoryStream(dds))
            {
                ms.Write(BuildDDSHeader(compression, width, height, imageSize, 0), 0, 128);
                ms.Write(ar.ReadBytes((int)imageSize), 0, (int)imageSize);
            }

            if (x360) SwapBytes(dds, 128);

            // Converts to bitmap
            return new MagickImage(dds);
        }

        private static void SwapBytes(byte[] data, int start = 0)
        {
            for (int i = start; i < data.Length; i += 2)
            {
                // Swaps bytes
                byte b = data[i];
                data[i] = data[i + 1];
                data[i + 1] = b;
            }
        }

        private static byte[] BuildDDSHeader(string format, uint width, uint height, uint size, uint mipMaps) // 128 bytes
        {
            byte[] dds = new byte[] //512x512 DXT5  -- 128 Bytes
                {//|-D-----D-----S---------|--Header Size (124)----|-------Flags-----------|-------Height--------|
                    0x44, 0x44, 0x53, 0x20, 0x7C, 0x00, 0x00, 0x00, 0x07, 0x10, 0x08, 0x00, 0x00, 0x02, 0x00, 0x00,
                 //|--------Width----------|-----Size or Pitch-----|                       |-------Mip Maps------|
                    0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x4E, 0x45, 0x4D, 0x4F, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x00,
                    0x04, 0x00, 0x00, 0x00, 0x44, 0x58, 0x54, 0x35, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
                };

            byte[] buffer = new byte[4];
            buffer = BitConverter.GetBytes(height);

            dds[12] = buffer[0];
            dds[13] = buffer[1];
            dds[14] = buffer[2];
            dds[15] = buffer[3];

            buffer = BitConverter.GetBytes(width);

            dds[16] = buffer[0];
            dds[17] = buffer[1];
            dds[18] = buffer[2];
            dds[19] = buffer[3];

            buffer = BitConverter.GetBytes(size);

            dds[20] = buffer[0];
            dds[21] = buffer[1];
            dds[22] = buffer[2];
            dds[23] = buffer[3];

            buffer = BitConverter.GetBytes(mipMaps);

            dds[28] = buffer[0];
            dds[29] = buffer[1];
            dds[30] = buffer[2];
            dds[31] = buffer[3];

            // Format magic
            dds[84] = (byte)format[0];
            dds[85] = (byte)format[1];
            dds[86] = (byte)format[2];
            dds[87] = (byte)format[3];
            
            return dds;
        }

        private static MagickImage DecodeBMP(AwesomeReader ar, uint bpp, uint width, uint height, uint bpl, bool ps2Texture = true)
        {
            Bitmap bmp = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);
            
            if (bpp == 4)
            {
                // 16 color palette (RGBa)
                Color[] palette = (ps2Texture) ? GetColorPalette(ar, 16) : GetColorPaletteBGRa(ar, 16); // 2^4 = 16 colors
                int index, pixel1, pixel2;
                //return bmp; // Fix later

                
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w += 2)
                    {
                        // Actually two pixels
                        index = ar.ReadByte();
                        pixel1 = (index & 0xF0) >> 4;
                        pixel2 = index & 0x0F;

                        // Pixels are in reverse order
                        bmp.SetPixel(w, h, palette[pixel2]);
                        bmp.SetPixel(w + 1, h, palette[pixel1]);
                    }
                }
            }
            else if (bpp == 8)
            {
                // 256 color palette (RGBa)
                Color[] palette = (ps2Texture) ? GetColorPalette(ar, 256) : GetColorPaletteBGRa(ar, 256); // 2^8 = 256 colors
                int index, bit3, bit4;

                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        index = ar.ReadByte();

                        if (ps2Texture)
                        {
                            // Swaps bits 3 and 4 with eachother
                            // Ex: 0110 1011 -> 0111 0011
                            bit3 = (index & 0x10) >> 1;
                            bit4 = (index & 0x08) << 1;
                            index = (0xE7 & index) | (bit4 | bit3);
                        }

                        bmp.SetPixel(w, h, palette[index]);
                    }
                }
            }
            else if (bpp == 24)
            {
                // RGB (No alpha channel)
                byte R, G, B, a = 0xFF;

                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        // Reads color
                        R = ar.ReadByte();
                        G = ar.ReadByte();
                        B = ar.ReadByte();

                        bmp.SetPixel(w, h, Color.FromArgb(a, R, G, B));
                    }
                }
            }
            else if (bpp == 32)
            {
                // RGBa
                byte R, G, B, a;

                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        // Reads color
                        R = ar.ReadByte();
                        G = ar.ReadByte();
                        B = ar.ReadByte();
                        a = ar.ReadByte();

                        if (a == 0x80) a = 0xFF;
                        else if (a > 0x80) Console.WriteLine("Alpha channel has value of {0}", a);
                        else a = (byte)(a << 1);

                        bmp.SetPixel(w, h, Color.FromArgb(a, R, G, B));
                    }
                }
            }

            return new MagickImage(bmp); // TODO: Just use magick image from the start
        }

        private static Color[] GetColorPaletteBGRa(AwesomeReader ar, uint count)
        {
            Color[] colors = new Color[count];
            byte R, G, B, a;

            for (int i = 0; i < count; i++)
            {
                // Reads color
                B = ar.ReadByte();
                G = ar.ReadByte();
                R = ar.ReadByte();
                a = ar.ReadByte();

                if (a == 0x80) a = 0xFF;
                else if (a > 0x80) Console.WriteLine("Alpha channel has value of {0}", a);
                else a = (byte)(a << 1);

                // Sets color
                colors[i] = Color.FromArgb(a, R, G, B);
            }

            return colors;
        }

        private static Color[] GetColorPalette(AwesomeReader ar, uint count, byte? alpha = null)
        {
            Color[] colors = new Color[count];
            byte R, G, B, a;

            if (!alpha.HasValue)
            {
                for (int i = 0; i < count; i++)
                {
                    // Reads color
                    R = ar.ReadByte();
                    G = ar.ReadByte();
                    B = ar.ReadByte();
                    a = ar.ReadByte();

                    if (a == 0x80) a = 0xFF;
                    else if (a > 0x80) Console.WriteLine("Alpha channel has value of {0}", a);
                    else a = (byte)(a << 1);

                    // Sets color
                    colors[i] = Color.FromArgb(a, R, G, B);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    // Reads color
                    R = ar.ReadByte();
                    G = ar.ReadByte();
                    B = ar.ReadByte();
                    a = ar.ReadByte();

                    // Sets color
                    colors[i] = Color.FromArgb(alpha.Value, R, G, B);
                }
            }

            return colors;
        }
    }
}
