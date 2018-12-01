using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render.Interfaces;

namespace Mackiloha.Render
{
    public class Cam : RenderObject, ITrans
    {
        public Trans Trans => new Trans();

        public Matrix4 Mat { get; set; }

        public override MiloString Type => "Cam";
    }
}
