using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Mackiloha.IO;
using ImageMagick;

namespace Mackiloha.Wpf.Extensions
{
    public static class TextureExtensions
    {
        public static byte[] ToRGBA(this HMXBitmap bitmap, SystemInfo info)
        {
            switch (bitmap.Encoding)
            {
                case 3:
                    var image = DecodeBitmap(bitmap.RawData, bitmap.Width, bitmap.Height, bitmap.MipMaps, bitmap.Bpp);

                    // Converts BGRa -> RGBa if needed
                    if (info.Platform == Platform.XBOX
                        || info.Platform == Platform.X360)
                        SwapRBColors(image);

                    return image;
                case 8:
                case 24:
                    var tempData = new byte[bitmap.RawData.Length];
                    Array.Copy(bitmap.RawData, tempData, tempData.Length);

                    if (info.Platform == Platform.X360)
                        SwapBytes(tempData);

                    return DecodeDXT1(tempData, bitmap.Width, bitmap.Height, bitmap.MipMaps, bitmap.Encoding == 24);
                default:
                    return null;
            }
        }

        private static void SwapRBColors(byte[] image)
        {
            byte temp;

            for (int i = 0; i < image.Length; i += 16)
            {
                // Pixel 1
                temp          = image[i     ];
                image[i     ] = image[i +  2];
                image[i +  2] = temp;

                // Pixel 2
                temp          = image[i +  4];
                image[i +  4] = image[i +  6];
                image[i +  6] = temp;

                // Pixel 3
                temp          = image[i +  8];
                image[i +  8] = image[i + 10];
                image[i + 10] = temp;

                // Pixel 4
                temp         =  image[i + 12];
                image[i + 12] = image[i + 14];
                image[i + 14] = temp;
            }
        }

        private static void SwapBytes(byte[] image)
        {
            byte temp;

            for (int i = 0; i < image.Length; i += 2)
            {
                temp = image[i];
                image[i    ] = image[i + 1];
                image[i + 1] = temp;
            }
        }

        private static byte[] DecodeBitmap(byte[] raw, int width, int height, int mips, int bpp)
        {
            byte[] image = new byte[width * height * 4]; // 32 bpp

            // TODO: Take into account mip maps
            if (bpp == 32)
            {
                Array.Copy(raw, image, image.Length);
                return image;
            }
            else if (bpp == 24)
            {
                int r = 0;
                for (int i = 0; i < image.Length; i += 16)
                {
                    // Pixel 1
                    image[i    ]  = raw[r     ];
                    image[i + 1]  = raw[r +  1];
                    image[i + 2]  = raw[r +  2];
                    image[i + 3]  = 0xFF;
                    // Pixel 2
                    image[i + 4]  = raw[r +  3];
                    image[i + 5]  = raw[r +  4];
                    image[i + 6]  = raw[r +  5];
                    image[i + 7]  = 0xFF;
                    // Pixel 3
                    image[i +  8] = raw[r +  6];
                    image[i +  9] = raw[r +  7];
                    image[i + 10] = raw[r +  8];
                    image[i + 11] = 0xFF;
                    // Pixel 4
                    image[i + 12] = raw[r +  9];
                    image[i + 13] = raw[r + 10];
                    image[i + 14] = raw[r + 11];
                    image[i + 15] = 0xFF;
                    r += 12;
                }
                return image;
            }
            byte[] palette = new byte[1 << (bpp + 2)];
            Array.Copy(raw, palette, palette.Length);
            var o = palette.Length; // Pixel start offset
            // Updates alpha channels (7-bit -> 8-bit)
            byte a;
            for (int p = 0; p < palette.Length; p += 4)
            {
                // Set to max (0xFF) if 128 or multiply by 2
                a = palette[p + 3];
                palette[p + 3] = ((a & 0x80) != 0) ? (byte)0xFF : (byte)(a << 1);
            }
            if (bpp == 4)
            {
                int r = 0, p1, p2, p3, p4;
                for (int i = 0; i < image.Length; i += 16)
                {
                    // Palette offsets
                    p1 = (raw[ o + r    ] & 0x0F) << 2;
                    p2 = (raw[ o + r    ] & 0xF0) >> 2;
                    p3 = (raw[ o + r + 1] & 0x0F) << 2;
                    p4 = (raw[ o + r + 1] & 0xF0) >> 2;
                    // Pixel 1
                    image[i     ] = palette[p1    ];
                    image[i +  1] = palette[p1 + 1];
                    image[i +  2] = palette[p1 + 2];
                    image[i +  3] = palette[p1 + 3];
                    // Pixel 2
                    image[i +  4] = palette[p2    ];
                    image[i +  5] = palette[p2 + 1];
                    image[i +  6] = palette[p2 + 2];
                    image[i +  7] = palette[p2 + 3];
                    // Pixel 3
                    image[i +  8] = palette[p3    ];
                    image[i +  9] = palette[p3 + 1];
                    image[i + 10] = palette[p3 + 2];
                    image[i + 11] = palette[p3 + 3];
                    // Pixel 4
                    image[i + 12] = palette[p4    ];
                    image[i + 13] = palette[p4 + 1];
                    image[i + 14] = palette[p4 + 2];
                    image[i + 15] = palette[p4 + 3];
                    r += 2;
                }
            }
            else if (bpp == 8)
            {
                int r = 0, p1, p2, p3, p4;
                for (int i = 0; i < image.Length; i += 16)
                {
                    // Palette offsets
                    // Swaps bits 3 and 4 with eachother
                    // Ex: 0110 1011 -> 0111 0011
                    p1 = ((0xE7 & raw[o + r    ]) | ((0x08 & raw[o + r    ]) << 1) | ((0x10 & raw[o + r    ]) >> 1)) << 2;
                    p2 = ((0xE7 & raw[o + r + 1]) | ((0x08 & raw[o + r + 1]) << 1) | ((0x10 & raw[o + r + 1]) >> 1)) << 2;
                    p3 = ((0xE7 & raw[o + r + 2]) | ((0x08 & raw[o + r + 2]) << 1) | ((0x10 & raw[o + r + 2]) >> 1)) << 2;
                    p4 = ((0xE7 & raw[o + r + 3]) | ((0x08 & raw[o + r + 3]) << 1) | ((0x10 & raw[o + r + 3]) >> 1)) << 2;
                    // Pixel 1
                    image[i     ] = palette[p1    ];
                    image[i +  1] = palette[p1 + 1];
                    image[i +  2] = palette[p1 + 2];
                    image[i +  3] = palette[p1 + 3];
                    // Pixel 2
                    image[i +  4] = palette[p2    ];
                    image[i +  5] = palette[p2 + 1];
                    image[i +  6] = palette[p2 + 2];
                    image[i +  7] = palette[p2 + 3];
                    // Pixel 3
                    image[i +  8] = palette[p3    ];
                    image[i +  9] = palette[p3 + 1];
                    image[i + 10] = palette[p3 + 2];
                    image[i + 11] = palette[p3 + 3];
                    // Pixel 4
                    image[i + 12] = palette[p4    ];
                    image[i + 13] = palette[p4 + 1];
                    image[i + 14] = palette[p4 + 2];
                    image[i + 15] = palette[p4 + 3];
                    r += 4;
                }
            }
            
            return image;
        }

        private static byte[] DecodeDXT1(byte[] raw, int width, int height, int mips, bool dxt5)
        {
            byte[] image = new byte[width * height * 4]; // 32 bpp

            // TODO: Make parameter instead
            int blockX = width >> 2;
            int blockY = height >> 2;
            int blockSize = 8; // (16 pixels * 4bpp) / 8
            
            int[] colors = new int[4];
            int[] pixelIndices = new int[16];
            int[] pixels = new int[16];

            byte[] colorRgba = new byte[16];

            (byte R, byte G, byte B, byte A)[] colors2 = new (byte, byte, byte, byte)[4];
            
            int i = 0, x, y;
            ushort packed0, packed1;

            for (int by = 0; by < blockY; by++)
            {
                for (int bx = 0; bx < blockX; bx++)
                {
                    if (dxt5)
                        i += blockSize;

                    packed0 = (ushort)(raw[i    ] | raw[i + 1] << 8);
                    packed1 = (ushort)(raw[i + 2] | raw[i + 3] << 8);


                    colors[0] = ReadRGBAFromRGB565(packed0);
                    colors[1] = ReadRGBAFromRGB565(packed1);

                    colors2[0] = RGBAFromRGB565(packed0);
                    colors2[1] = RGBAFromRGB565(packed1);

                    if (!dxt5 && packed0 <= packed1)
                    {
                        colors[2] = AddRGBAColors(MultiplyRGBAColors(colors[0], 0.5f), MultiplyRGBAColors(colors[1], 0.5f));
                        colors[3] = 0;

                        colors2[2] = ((byte)((colors2[0].R + colors2[1].R) / 2), (byte)((colors2[0].G + colors2[1].G) / 2), (byte)((colors2[0].B + colors2[1].B) / 2), 0xFF);

                        //colors2[2] = ((byte)(colors2[0].R * 0.5 + colors2[1].R * 0.5), (byte)(colors2[0].G * 0.5 + colors2[1].G * 0.5), (byte)(colors2[0].B * 0.5 + colors2[1].B * 0.5), (byte)(colors2[0].A * 0.5 + colors2[1].A * 0.5));
                        colors2[3] = (0, 0, 0, 0);
                    }
                    else
                    {
                        colors[2] = AddRGBAColors(MultiplyRGBAColors(colors[0], 0.66f), MultiplyRGBAColors(colors[1], 0.33f));
                        colors[3] = AddRGBAColors(MultiplyRGBAColors(colors[0], 0.33f), MultiplyRGBAColors(colors[1], 0.66f));

                        colors2[2] = ((byte)((colors2[0].R * 2 + colors2[1].R) / 3), (byte)((colors2[0].G * 2 + colors2[1].G) / 3), (byte)((colors2[0].B * 2 + colors2[1].B) / 3), 0xFF);
                        colors2[3] = ((byte)((colors2[1].R * 2 + colors2[0].R) / 3), (byte)((colors2[1].G * 2 + colors2[0].G) / 3), (byte)((colors2[1].B * 2 + colors2[0].B) / 3), 0xFF);

                        //colors2[2] = ((byte)((colors2[0].R * 0.66 + colors2[1].R * 0.33)), (byte)((colors2[0].G * 0.66 + colors2[1].G * 0.33)), (byte)((colors2[0].B * 0.66 + colors2[1].B * 0.33)), (byte)((colors2[0].A * 0.66 + colors2[1].A * 0.33)));
                        //colors2[3] = ((byte)((colors2[1].R * 0.66 + colors2[0].R * 0.33)), (byte)((colors2[1].G * 0.66 + colors2[0].G * 0.33)), (byte)((colors2[1].B * 0.66 + colors2[0].B * 0.33)), (byte)((colors2[1].A * 0.66 + colors2[0].A * 0.33)));
                    }

                    /*
                    // Indices - 4 bytes (16 pixels)
                    pixels[ 0] = colors[(raw[i + 4] & 0b00_00_00_11)     ]; // Row 1
                    pixels[ 1] = colors[(raw[i + 4] & 0b00_00_11_00) >> 2];
                    pixels[ 2] = colors[(raw[i + 4] & 0b00_11_00_00) >> 4];
                    pixels[ 3] = colors[(raw[i + 4] & 0b11_00_00_00) >> 6];

                    pixels[ 4] = colors[(raw[i + 5] & 0b00_00_00_11)     ]; // Row 2
                    pixels[ 5] = colors[(raw[i + 5] & 0b00_00_11_00) >> 2];
                    pixels[ 6] = colors[(raw[i + 5] & 0b00_11_00_00) >> 4];
                    pixels[ 7] = colors[(raw[i + 5] & 0b11_00_00_00) >> 6];

                    pixels[ 8] = colors[(raw[i + 6] & 0b00_00_00_11)     ]; // Row 3
                    pixels[ 9] = colors[(raw[i + 6] & 0b00_00_11_00) >> 2];
                    pixels[10] = colors[(raw[i + 6] & 0b00_11_00_00) >> 4];
                    pixels[11] = colors[(raw[i + 6] & 0b11_00_00_00) >> 6];

                    pixels[12] = colors[(raw[i + 7] & 0b00_00_00_11)     ]; // Row 4
                    pixels[13] = colors[(raw[i + 7] & 0b00_00_11_00) >> 2];
                    pixels[14] = colors[(raw[i + 7] & 0b00_11_00_00) >> 4];
                    pixels[15] = colors[(raw[i + 7] & 0b11_00_00_00) >> 6];
                    */

                    /*
                    colorRgba[ 0] = (byte)((colors[0] & 0xFF_00_00_00) >> 24);
                    colorRgba[ 1] = (byte)((colors[0] & 0x00_FF_00_00) >> 16);
                    colorRgba[ 2] = (byte)((colors[0] & 0x00_00_FF_00) >>  8);
                    colorRgba[ 3] = (byte)((colors[0] & 0x00_00_00_FF));

                    colorRgba[ 4] = (byte)((colors[1] & 0xFF_00_00_00) >> 24);
                    colorRgba[ 5] = (byte)((colors[1] & 0x00_FF_00_00) >> 16);
                    colorRgba[ 6] = (byte)((colors[1] & 0x00_00_FF_00) >>  8);
                    colorRgba[ 7] = (byte)((colors[1] & 0x00_00_00_FF));

                    colorRgba[ 8] = (byte)((colors[2] & 0xFF_00_00_00) >> 24);
                    colorRgba[ 9] = (byte)((colors[2] & 0x00_FF_00_00) >> 16);
                    colorRgba[10] = (byte)((colors[2] & 0x00_00_FF_00) >>  8);
                    colorRgba[11] = (byte)((colors[2] & 0x00_00_00_FF));

                    colorRgba[12] = (byte)((colors[3] & 0xFF_00_00_00) >> 24);
                    colorRgba[13] = (byte)((colors[3] & 0x00_FF_00_00) >> 16);
                    colorRgba[14] = (byte)((colors[3] & 0x00_00_FF_00) >>  8);
                    colorRgba[15] = (byte)((colors[3] & 0x00_00_00_FF));
                    */

                    colorRgba[ 0] = colors2[0].R;
                    colorRgba[ 1] = colors2[0].G;
                    colorRgba[ 2] = colors2[0].B;
                    colorRgba[ 3] = colors2[0].A;

                    colorRgba[ 4] = colors2[1].R;
                    colorRgba[ 5] = colors2[1].G;
                    colorRgba[ 6] = colors2[1].B;
                    colorRgba[ 7] = colors2[1].A;

                    colorRgba[ 8] = colors2[2].R;
                    colorRgba[ 9] = colors2[2].G;
                    colorRgba[10] = colors2[2].B;
                    colorRgba[11] = colors2[2].A;

                    colorRgba[12] = colors2[3].R;
                    colorRgba[13] = colors2[3].G;
                    colorRgba[14] = colors2[3].B;
                    colorRgba[15] = colors2[3].A;

                    // Indices - 4 bytes (16 pixels)
                    pixelIndices[ 0] = (raw[i + 4] & 0b00_00_00_11)     ; // Row 1
                    pixelIndices[ 1] = (raw[i + 4] & 0b00_00_11_00) >> 2;
                    pixelIndices[ 2] = (raw[i + 4] & 0b00_11_00_00) >> 4;
                    pixelIndices[ 3] = (raw[i + 4] & 0b11_00_00_00) >> 6;

                    pixelIndices[ 4] = (raw[i + 5] & 0b00_00_00_11)     ; // Row 2
                    pixelIndices[ 5] = (raw[i + 5] & 0b00_00_11_00) >> 2;
                    pixelIndices[ 6] = (raw[i + 5] & 0b00_11_00_00) >> 4;
                    pixelIndices[ 7] = (raw[i + 5] & 0b11_00_00_00) >> 6;

                    pixelIndices[ 8] = (raw[i + 6] & 0b00_00_00_11)     ; // Row 3
                    pixelIndices[ 9] = (raw[i + 6] & 0b00_00_11_00) >> 2;
                    pixelIndices[10] = (raw[i + 6] & 0b00_11_00_00) >> 4;
                    pixelIndices[11] = (raw[i + 6] & 0b11_00_00_00) >> 6;

                    pixelIndices[12] = (raw[i + 7] & 0b00_00_00_11)     ; // Row 4
                    pixelIndices[13] = (raw[i + 7] & 0b00_00_11_00) >> 2;
                    pixelIndices[14] = (raw[i + 7] & 0b00_11_00_00) >> 4;
                    pixelIndices[15] = (raw[i + 7] & 0b11_00_00_00) >> 6;

                    x = bx << 2;
                    y = by << 2;

                    Array.Copy(colorRgba, pixelIndices[ 0] << 2, image, LinearOffset(x    , y    , width), 4);
                    Array.Copy(colorRgba, pixelIndices[ 1] << 2, image, LinearOffset(x + 1, y    , width), 4);
                    Array.Copy(colorRgba, pixelIndices[ 2] << 2, image, LinearOffset(x + 2, y    , width), 4);
                    Array.Copy(colorRgba, pixelIndices[ 3] << 2, image, LinearOffset(x + 3, y    , width), 4);

                    Array.Copy(colorRgba, pixelIndices[ 4] << 2, image, LinearOffset(x    , y + 1, width), 4);
                    Array.Copy(colorRgba, pixelIndices[ 5] << 2, image, LinearOffset(x + 1, y + 1, width), 4);
                    Array.Copy(colorRgba, pixelIndices[ 6] << 2, image, LinearOffset(x + 2, y + 1, width), 4);
                    Array.Copy(colorRgba, pixelIndices[ 7] << 2, image, LinearOffset(x + 3, y + 1, width), 4);

                    Array.Copy(colorRgba, pixelIndices[ 8] << 2, image, LinearOffset(x    , y + 2, width), 4);
                    Array.Copy(colorRgba, pixelIndices[ 9] << 2, image, LinearOffset(x + 1, y + 2, width), 4);
                    Array.Copy(colorRgba, pixelIndices[10] << 2, image, LinearOffset(x + 2, y + 2, width), 4);
                    Array.Copy(colorRgba, pixelIndices[11] << 2, image, LinearOffset(x + 3, y + 2, width), 4);

                    Array.Copy(colorRgba, pixelIndices[12] << 2, image, LinearOffset(x    , y + 3, width), 4);
                    Array.Copy(colorRgba, pixelIndices[13] << 2, image, LinearOffset(x + 1, y + 3, width), 4);
                    Array.Copy(colorRgba, pixelIndices[14] << 2, image, LinearOffset(x + 2, y + 3, width), 4);
                    Array.Copy(colorRgba, pixelIndices[15] << 2, image, LinearOffset(x + 3, y + 3, width), 4);

                    i += blockSize;
                }
            }

            return image;
        }

        private static int LinearOffset(int x, int y, int w) => (y * (w << 2)) + (x << 2);

        private static (byte, byte, byte, byte) RGBAFromRGB565(ushort c)
        {
            return ((byte)((((c & 0b1111_1000_0000_0000) << 16) | ((c & 0b1110_0000_0000_0000) << 11)) >> 24),
                    (byte)((((c & 0b0000_0111_1110_0000) << 13) | ((c & 0b0000_0110_0000_0000) <<  7)) >> 16),
                    (byte)((((c & 0b0000_0000_0001_1111) << 11) | ((c & 0b0000_0000_0001_1100) <<  6)) >>  8),
                    0xFF);
        }

        private static int ReadRGBAFromRGB565(ushort c)
        {
            return ((c & 0b1111_1000_0000_0000) << 16) | ((c & 0b1110_0000_0000_0000) << 11)
                |  ((c & 0b0000_0111_1110_0000) << 13) | ((c & 0b0000_0110_0000_0000) <<  7)
                |  ((c & 0b0000_0000_0001_1111) << 11) | ((c & 0b0000_0000_0001_1100) <<  6)
                | 0xFF;
        }
        
        private static int AddRGBAColors(int c1, int c2)
        {
            return (int)((((c1 & 0xFF_00_00_00) + (c2 & 0xFF_00_00_00)) & 0xFF_00_00_00)
                      |  (((c1 & 0x00_FF_00_00) + (c2 & 0x00_FF_00_00)) & 0x00_FF_00_00)
                      |  (((c1 & 0x00_00_FF_00) + (c2 & 0x00_00_FF_00)) & 0x00_00_FF_00)
                      //|  (((c1 & 0x00_00_00_FF) + (c2 & 0x00_00_00_FF)) & 0x00_00_00_FF));
                      | 0xFF);
        }

        private static int MultiplyRGBAColors(int c, float m)
        {
            return (int)(((byte)(((c & 0xFF_00_00_00) >> 24) * m) << 24 & 0xFF_00_00_00)
                      |  ((byte)(((c & 0x00_FF_00_00) >> 16) * m) << 16 & 0x00_FF_00_00)
                      |  ((byte)(((c & 0x00_00_FF_00) >>  8) * m) <<  8 & 0x00_00_FF_00)
                      //|  ((int)((c & 0x00_00_00_FF) * m) & 0x00_00_00_FF));
                      | 0xFF);
        }
        
        private static void ReadRGBAFromRGB565(byte[] rgba, int c)
        {
            rgba[0] = (byte)((c & 0b1111_1000_0000_0000) >> 8);
            rgba[1] = (byte)((c & 0b0000_0111_1110_0000) >> 3);
            rgba[2] = (byte)((c & 0b0000_0000_0001_1111) << 3);
            rgba[3] = 0xFF;
        }

        private static void AddRGBAColors(byte[] combined, byte[] c1, byte[] c2)
        {
            combined[0] = (byte)(c1[0] + c2[0]);
            combined[1] = (byte)(c1[1] + c2[1]);
            combined[2] = (byte)(c1[2] + c2[2]);
            combined[3] = (byte)(c1[3] + c2[3]);
        }

        public static BitmapSource ToBitmapSource(this HMXBitmap bitmap, SystemInfo info)
        {
            var rgba = bitmap.ToRGBA(info);

            return new MagickImage(rgba, new PixelStorageSettings(bitmap.Width, bitmap.Height, StorageType.Char, PixelMapping.RGBA))
                .ToBitmapSource();
        }

        public static void SaveAs(this HMXBitmap bitmap, SystemInfo info, string path)
        {
            var rgba = bitmap.ToRGBA(info);

            new MagickImage(rgba, new PixelStorageSettings(bitmap.Width, bitmap.Height, StorageType.Char, PixelMapping.RGBA))
                .Write(path);
        }
    }
}
