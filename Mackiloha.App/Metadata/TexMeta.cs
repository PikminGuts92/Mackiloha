using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.App.Metadata
{
    public enum TexEncoding
    {
        Bitmap,
        DXT1,
        DXT5,
        ATI2
    }

    public class TexMeta
    {
        public TexEncoding? Encoding { get; set; }
    }
}
