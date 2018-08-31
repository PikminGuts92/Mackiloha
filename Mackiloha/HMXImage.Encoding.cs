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
                    return DecodeDXT(ar, bpp, width, height, encoding);
            }
            
            return null;
        }

        private static MagickImage DecodeDXT(AwesomeReader ar, uint bpp, uint width, uint height, ImageEncoding encoding, bool x360 = true)
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

            if (encoding == ImageEncoding.DXT1)
            {
                const int PIXELS_PER_BLOCK = 16;
                int blockSize = (PIXELS_PER_BLOCK * (int)bpp) / 8;

                byte[] data = ar.ReadBytes((int)imageSize);
                if (x360) SwapBytes(data);

                Color FromRGB565(int c) => Color.FromArgb(
                    0xFF,
                    (c & 0b1111_1000_0000_0000) >> 8,
                    (c & 0b0000_0111_1110_0000) >> 3,
                    (c & 0b0000_0000_0001_1111) << 3);

                Color MultiplyColor(Color c, float mult) => Color.FromArgb(
                    (int)(c.A * mult),
                    (int)(c.R * mult),
                    (int)(c.G * mult),
                    (int)(c.B * mult));

                Color AddColors(Color a, Color b) => Color.FromArgb(
                    a.A + b.A,
                    a.R + b.R,
                    a.G + b.G,
                    a.B + b.B);

                int idx = 0;
                var colors = new Color[4];
                var pixels = new Color[16];

                int w, h;
                var imageBytes = new byte[width * height * 4]; // 32-bit color
                
                for (int y = 0; y < (height >> 2); y++)
                {
                    for (int x = 0; x < (width >> 2); x++)
                    {
                        // Colors - 4 bytes
                        colors[0] = FromRGB565(data[idx    ] | data[idx + 1] << 8);
                        colors[1] = FromRGB565(data[idx + 2] | data[idx + 3] << 8);
                        colors[2] = AddColors(MultiplyColor(colors[0], 0.66f), MultiplyColor(colors[1], 0.33f));
                        colors[3] = AddColors(MultiplyColor(colors[0], 0.33f), MultiplyColor(colors[1], 0.66f));
                        //colors[2] = AddColors(MultiplyColor(colors[0], 0.5f), MultiplyColor(colors[1], 0.5f));
                        //colors[3] = Color.FromArgb(0);
                        
                        // Indices - 4 bytes (16 pixels)
                        pixels[ 0] = colors[(data[idx + 4] & 0b00_00_00_11)     ]; // Row 1
                        pixels[ 1] = colors[(data[idx + 4] & 0b00_00_11_00) >> 2];
                        pixels[ 2] = colors[(data[idx + 4] & 0b00_11_00_00) >> 4];
                        pixels[ 3] = colors[(data[idx + 4] & 0b11_00_00_00) >> 6];

                        pixels[ 4] = colors[(data[idx + 5] & 0b00_00_00_11)     ]; // Row 2
                        pixels[ 5] = colors[(data[idx + 5] & 0b00_00_11_00) >> 2];
                        pixels[ 6] = colors[(data[idx + 5] & 0b00_11_00_00) >> 4];
                        pixels[ 7] = colors[(data[idx + 5] & 0b11_00_00_00) >> 6];

                        pixels[ 8] = colors[(data[idx + 6] & 0b00_00_00_11)     ]; // Row 3
                        pixels[ 9] = colors[(data[idx + 6] & 0b00_00_11_00) >> 2];
                        pixels[10] = colors[(data[idx + 6] & 0b00_11_00_00) >> 4];
                        pixels[11] = colors[(data[idx + 6] & 0b11_00_00_00) >> 6];

                        pixels[12] = colors[(data[idx + 7] & 0b00_00_00_11)     ]; // Row 4
                        pixels[13] = colors[(data[idx + 7] & 0b00_00_11_00) >> 2];
                        pixels[14] = colors[(data[idx + 7] & 0b00_11_00_00) >> 4];
                        pixels[15] = colors[(data[idx + 7] & 0b11_00_00_00) >> 6];
                        
                        w = x << 2;
                        h = y << 2;
                        
                        int linerOffset(int oY, int oX) => (oY * ((int)width << 2)) + (oX << 2);

                        void SetPixel(int pY, int pX, Color c)
                        {
                            int off = linerOffset(pX, pY);
                            imageBytes[off    ] = c.R;
                            imageBytes[off + 1] = c.G;
                            imageBytes[off + 2] = c.B;
                            imageBytes[off + 3] = c.A;
                        }

                        SetPixel(w    , h, pixels[0]);
                        SetPixel(w + 1, h, pixels[1]);
                        SetPixel(w + 2, h, pixels[2]);
                        SetPixel(w + 3, h, pixels[3]);

                        SetPixel(w    , h + 1, pixels[4]);
                        SetPixel(w + 1, h + 1, pixels[5]);
                        SetPixel(w + 2, h + 1, pixels[6]);
                        SetPixel(w + 3, h + 1, pixels[7]);

                        SetPixel(w    , h + 2, pixels[8]);
                        SetPixel(w + 1, h + 2, pixels[9]);
                        SetPixel(w + 2, h + 2, pixels[10]);
                        SetPixel(w + 3, h + 2, pixels[11]);

                        SetPixel(w    , h + 3, pixels[12]);
                        SetPixel(w + 1, h + 3, pixels[13]);
                        SetPixel(w + 2, h + 3, pixels[14]);
                        SetPixel(w + 3, h + 3, pixels[15]);
                        
                        idx += blockSize;
                    }
                }
                
                return new MagickImage(imageBytes, new PixelStorageSettings((int)width, (int)height, StorageType.Char, PixelMapping.RGBA));
            }
            else if (encoding == ImageEncoding.ATI2)
            {
                const int PIXELS_PER_BLOCK = 16;
                int blockSize = (PIXELS_PER_BLOCK * (int)bpp) / 8;

                byte[] data = ar.ReadBytes((int)imageSize);
                if (x360) SwapBytes(data);

                int idx = 0;
                
                var reds = new float[8];
                var greens = new float[8];
                
                var pixels = new Color[16];

                int w, h;
                var imageBytes = new byte[width * height * 4]; // 32-bit color

                void CalculateColors(float[] colors)
                {
                    var c0 = colors[0];
                    var c1 = colors[1];

                    // BC5_UNORM
                    if (c0 > c1)
                    {
                        // 6 interpolated color values
                        colors[2] = (6 * c0 + 1 * c1) / 7.0f;
                        colors[3] = (5 * c0 + 2 * c1) / 7.0f;
                        colors[4] = (4 * c0 + 3 * c1) / 7.0f;
                        colors[5] = (3 * c0 + 4 * c1) / 7.0f;
                        colors[6] = (2 * c0 + 5 * c1) / 7.0f;
                        colors[7] = (1 * c0 + 6 * c1) / 7.0f;
                    }
                    else
                    {
                        // 4 interpolated color values
                        colors[2] = (4 * c0 + 1 * c1) / 5.0f;
                        colors[3] = (3 * c0 + 2 * c1) / 5.0f;
                        colors[4] = (2 * c0 + 3 * c1) / 5.0f;
                        colors[5] = (1 * c0 + 4 * c1) / 5.0f;
                        colors[6] = 0.0f;
                        colors[7] = 1.0f;
                    }
                }

                int[] GetIndices(int v1, int v2) =>
                    new int[] {
                        (v1 & 0b000_000_000_000_000_000_000_111),
                        (v1 & 0b000_000_000_000_000_000_111_000) >>  3,
                        (v1 & 0b000_000_000_000_000_111_000_000) >>  6,
                        (v1 & 0b000_000_000_000_111_000_000_000) >>  9,
                        (v1 & 0b000_000_000_111_000_000_000_000) >> 12,
                        (v1 & 0b000_000_111_000_000_000_000_000) >> 15,
                        (v1 & 0b000_111_000_000_000_000_000_000) >> 18,
                        (v1 & 0b111_000_000_000_000_000_000_000) >> 21,
                        (v2 & 0b000_000_000_000_000_000_000_111),
                        (v2 & 0b000_000_000_000_000_000_111_000) >>  3,
                        (v2 & 0b000_000_000_000_000_111_000_000) >>  6,
                        (v2 & 0b000_000_000_000_111_000_000_000) >>  9,
                        (v2 & 0b000_000_000_111_000_000_000_000) >> 12,
                        (v2 & 0b000_000_111_000_000_000_000_000) >> 15,
                        (v2 & 0b000_111_000_000_000_000_000_000) >> 18,
                        (v2 & 0b111_000_000_000_000_000_000_000) >> 21
                    };
                
                for (int y = 0; y < (height >> 2); y++)
                {
                    for (int x = 0; x < (width >> 2); x++)
                    {

                        // Reds - 2 bytes
                        reds[0] = data[idx] / (float)byte.MaxValue;
                        reds[1] = data[idx + 1] / (float)byte.MaxValue;
                        CalculateColors(reds);
                        
                        // Greens - 2 bytes
                        greens[0] = data[idx + 8] / (float)byte.MaxValue;
                        greens[1] = data[idx + 9] / (float)byte.MaxValue;
                        CalculateColors(greens);

                        // Indicies
                        var ir0 = GetIndices(data[idx +  2] | (data[idx +  3] << 8) | (data[idx +  4] << 16),
                                             data[idx +  5] | (data[idx +  6] << 8) | (data[idx +  7] << 16));
                        var ig0 = GetIndices(data[idx + 10] | (data[idx + 11] << 8) | (data[idx + 12] << 16),
                                             data[idx + 13] | (data[idx + 14] << 8) | (data[idx + 15] << 16));

                        Color GetColor(int i) => Color.FromArgb(
                            0xFF,
                            (byte)(reds[ir0[i]] * byte.MaxValue),
                            (byte)(greens[ig0[i]] * byte.MaxValue),
                            0x00
                            );
                        
                        pixels[0] = GetColor(0);
                        pixels[1] = GetColor(1);
                        pixels[2] = GetColor(2);
                        pixels[3] = GetColor(3);
                        pixels[4] = GetColor(4);
                        pixels[5] = GetColor(5);
                        pixels[6] = GetColor(6);
                        pixels[7] = GetColor(7);

                        pixels[ 8] = GetColor( 8);
                        pixels[ 9] = GetColor( 9);
                        pixels[10] = GetColor(10);
                        pixels[11] = GetColor(11);
                        pixels[12] = GetColor(12);
                        pixels[13] = GetColor(13);
                        pixels[14] = GetColor(14);
                        pixels[15] = GetColor(15);
                        
                        w = x << 2;
                        h = y << 2;

                        int linerOffset(int oY, int oX) => (oY * ((int)width << 2)) + (oX << 2);

                        void SetPixel(int pY, int pX, Color c)
                        {
                            int off = linerOffset(pX, pY);
                            imageBytes[off] = c.R;
                            imageBytes[off + 1] = c.G;
                            imageBytes[off + 2] = c.B;
                            imageBytes[off + 3] = c.A;
                        }

                        SetPixel(w    , h, pixels[0]);
                        SetPixel(w + 1, h, pixels[1]);
                        SetPixel(w + 2, h, pixels[2]);
                        SetPixel(w + 3, h, pixels[3]);

                        SetPixel(w    , h + 1, pixels[4]);
                        SetPixel(w + 1, h + 1, pixels[5]);
                        SetPixel(w + 2, h + 1, pixels[6]);
                        SetPixel(w + 3, h + 1, pixels[7]);

                        SetPixel(w    , h + 2, pixels[8]);
                        SetPixel(w + 1, h + 2, pixels[9]);
                        SetPixel(w + 2, h + 2, pixels[10]);
                        SetPixel(w + 3, h + 2, pixels[11]);

                        SetPixel(w    , h + 3, pixels[12]);
                        SetPixel(w + 1, h + 3, pixels[13]);
                        SetPixel(w + 2, h + 3, pixels[14]);
                        SetPixel(w + 3, h + 3, pixels[15]);

                        idx += blockSize;
                    }
                }

                return new MagickImage(imageBytes, new PixelStorageSettings((int)width, (int)height, StorageType.Char, PixelMapping.RGBA));
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
