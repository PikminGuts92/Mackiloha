using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using PanicAttack;

namespace Mackiloha
{
    public partial class HMXImage
    {
        private static Bitmap Decode(AwesomeReader ar, ImageEncoding encoding, uint bpp, uint width, uint height, uint bpl)
        {
            // Image starts at bottom left corner

            switch (encoding)
            {
                case ImageEncoding.BMP:
                    return DecodeBMP(ar, bpp, width, height, bpl);
            }
            
            return null;
        }

        private static Bitmap DecodeBMP(AwesomeReader ar, uint bpp, uint width, uint height, uint bpl)
        {
            Bitmap bmp = new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb);

            if (bpp == 4)
            {
                // 16 color palette (RGBa)
                Color[] palette = GetColorPalette(ar, 16, 0xFF); // 2^4 = 16 colors
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
                Color[] palette = GetColorPalette(ar, 256, 0xFF); // 2^8 = 256 colors
                int index, bit3, bit4;

                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        index = ar.ReadByte();

                        // Swaps bits 3 and 4 with eachother
                        // Ex: 0110 1011 -> 0111 0011
                        bit3 = (index & 0x10) >> 1;
                        bit4 = (index & 0x08) << 1;
                        index = (0xE7 & index) | (bit4 | bit3);

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

                        bmp.SetPixel(w, h, Color.FromArgb(a, R, G, B));
                    }
                }
            }

            return bmp;
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
