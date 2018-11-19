using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.System;
using Mackiloha.System.Render;

namespace Mackiloha.IO
{
    public partial class MiloSerializer
    {
        private void ReadFromStream(AwesomeReader ar, Tex tex)
        {
            // TODO: Add version check
            if (ar.ReadInt32() != 0x08)
                throw new Exception($"TexReader: Expected 0x08 at offset 0");

            tex.Width = ar.ReadInt32();
            tex.Height = ar.ReadInt32();
            tex.Bpp = ar.ReadInt32();

            tex.ExternalPath = ar.ReadString();

            if (ar.ReadSingle() != -8.0f)
                throw new Exception("TexReader: Expected -8.0");

            if (ar.ReadInt32() != 0x01)
                throw new Exception($"TexReader: Expected 0x01");

            tex.UseExternal = ar.ReadBoolean();

            tex.Bitmap = new HMXBitmap();
            ReadFromStream(ar, tex.Bitmap as HMXBitmap);
        }
    }
}
