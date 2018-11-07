using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.System.Render
{
    public class Tex : RenderObject
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Bpp { get; set; }

        public MiloString ExternalPath { get; set; }
        public bool UseExternal { get; set; }

        public HMXBitmap Bitmap { get; set; }
    }
}
