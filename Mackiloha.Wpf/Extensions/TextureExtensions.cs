using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageMagick;

namespace Mackiloha.Wpf.Extensions
{
    public static class TextureExtensions
    {
        public static byte[] ToRGBA(this HMXBitmap bitmap)
        {
            if (bitmap.Encoding != 3)
                return null; // TODO: Actually use system info

            byte[] image = new byte[bitmap.Width * bitmap.Height * 4]; // 32 bpp
            byte[] raw = bitmap.RawData;
            // TODO: Take into account mip maps
            if (bitmap.Bpp == 32)
            {
                Array.Copy(raw, image, image.Length);
                return image;
            }
            else if (bitmap.Bpp == 24)
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
            byte[] palette = new byte[1 << (bitmap.Bpp + 2)];
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
            if (bitmap.Bpp == 4)
            {
                int r = 0, p1, p2, p3, p4;
                for (int i = 0; i < image.Length; i += 16)
                {
                    // Palette offsets
                    p1 = (raw[ o + r    ] & 0xF0) >> 4;
                    p2 =  raw[ o + r    ];
                    p3 = (raw[ o + r + 1] & 0xF0) >> 4;
                    p4 =  raw[ o + r + 1];
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
            else if (bitmap.Bpp == 8)
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

        public static BitmapSource ToBitmapSource(this HMXBitmap bitmap)
        {
            var rgba = bitmap.ToRGBA();

            return new MagickImage(rgba, new PixelStorageSettings(bitmap.Width, bitmap.Height, StorageType.Char, PixelMapping.RGBA))
                .ToBitmapSource();
        }
    }
}
