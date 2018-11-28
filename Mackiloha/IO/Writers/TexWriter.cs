using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render;

namespace Mackiloha.IO
{
    public partial class MiloSerializer
    {
        private void WriteToStream(AwesomeWriter aw, Tex tex)
        {
            // TODO: Add version check
            aw.Write((int)0x08);

            aw.Write((int)tex.Width);
            aw.Write((int)tex.Height);
            aw.Write((int)tex.Bpp);

            aw.Write(tex.ExternalPath);
            aw.Write((float)-8.0);
            aw.Write((int)0x01);
            
            if (tex.UseExternal && tex.Bitmap != null)
            {
                aw.Write(true);
                WriteToStream(aw, tex.Bitmap);
            }
            else
            {
                aw.Write(false);
            }
        }
    }
}
