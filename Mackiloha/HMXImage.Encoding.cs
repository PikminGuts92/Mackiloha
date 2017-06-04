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
                Color[] palette = GetColorPalette(ar, 16); // 2^4 = 16 colors

                // Each byte is 2 pixels
                // Requires bit swapping (Ex: 0110 1011 -> 1011 0110)

                /*byte index;

                
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        index = ar.ReadByte();
                        bmp.SetPixel(w, h, palette[index]);
                    }
                }*/
            }
            else if (bpp == 8)
            {
                // 256 color palette (RGBa)
                Color[] palette = GetColorPalette(ar, 256); // 2^8 = 256 colors
                byte index;

                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        index = ar.ReadByte();
                        bmp.SetPixel(w, h, palette[index]);
                    }
                }
            }
            else if (bpp == 24)
            {
                // RGB (No alpha channel)
            }
            else if (bpp == 32)
            {
                // RGBa
            }

            return null;
        }

        private static Color[] GetColorPalette(AwesomeReader ar, uint count)
        {
            Color[] colors = new Color[count];
            byte R, G, B, a;
            
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

            return colors;
        }
    }
}
