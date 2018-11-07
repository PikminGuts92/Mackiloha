using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.System
{
    public class HMXBitmap
    {
        public int Bpp { get; set; }
        public int Encoding { get; set; }
        public int MipMaps { get; set; }
        
        public int Width { get; set; }
        public int Height { get; set; }
        public int BPL { get; set; }

        public byte[] RawData { get; set; }
    }
}
