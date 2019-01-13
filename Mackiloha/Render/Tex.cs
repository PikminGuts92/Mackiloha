using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public interface ITex : IRenderObject
    {
        int Width { get; set; }
        int Height { get; set; }
        int Bpp { get; set; }

        float IndexF { get; set; }
        int Index { get; set; }

        string ExternalPath { get; set; }
        bool UseExternal { get; set; }

        HMXBitmap Bitmap { get; set; }
    }

    public class Tex : RenderObject, ITex
    {
        // Tex
        public int Width { get; set; }
        public int Height { get; set; }
        public int Bpp { get; set; }

        public float IndexF { get; set; }
        public int Index { get; set; } = 1;

        public string ExternalPath { get; set; }
        public bool UseExternal { get; set; }

        public HMXBitmap Bitmap { get; set; }

        public override string Type => "Tex";
    }
}
