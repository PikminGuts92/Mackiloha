using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.System;
using Mackiloha.System.Render;

namespace Mackiloha.IO
{
    public partial class HMXBitmapWriter
    {
        private void WriteToStream(AwesomeWriter aw, HMXBitmap bitmap)
        {
            aw.Write((byte)0x01);

            aw.Write((byte)bitmap.Bpp);
            aw.Write((int)bitmap.Encoding);
            aw.Write((byte)bitmap.MipMaps);
        }
    }
}
