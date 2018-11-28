using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render;

namespace Mackiloha.IO
{
    public partial class MiloSerializer
    {
        private void WriteToStream(AwesomeWriter aw, HMXBitmap bitmap)
        {
            aw.Write((byte)0x01);

            aw.Write((byte)bitmap.Bpp);
            aw.Write((int)bitmap.Encoding);
            aw.Write((byte)bitmap.MipMaps);

            aw.Write((short)bitmap.Width);
            aw.Write((short)bitmap.Height);
            aw.Write((short)bitmap.BPL);

            aw.Write(new byte[19]);

            byte[] data = new byte[CalculateTextureByteSize(bitmap.Encoding, bitmap.Width, bitmap.Height, bitmap.Bpp, bitmap.MipMaps)];
            Array.Copy(bitmap.RawData, data, data.Length);
            aw.Write(data);
        }
    }
}
