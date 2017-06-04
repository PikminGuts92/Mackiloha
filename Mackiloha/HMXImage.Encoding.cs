using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using PanicAttack;

namespace Mackiloha
{
    public partial class HMXImage
    {
        private static Bitmap Decode(AwesomeReader ar, ImageEncoding encoding, uint bpp, uint width, uint height, uint bpl)
        {
            switch (encoding)
            {
                case ImageEncoding.BMP:
                    return DecodeBMP(ar, bpp, width, height, bpl);
            }
            
            return null;
        }

        private static Bitmap DecodeBMP(AwesomeReader ar, uint bpp, uint width, uint height, uint bpl)
        {
            if (bpp == 4)
            {
                // 16 color palette (RGBA)
            }
            else if (bpp == 8)
            {
                // 256 color palette (RGBA)
            }
            else if (bpp == 24)
            {
                // RGB (No alpha channel)
            }
            else if (bpp == 32)
            {
                // RGBA
            }

            return null;
        }
    }
}
