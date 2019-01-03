using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render.Interfaces;

namespace Mackiloha.Render
{
    public class Flare : RenderObject, ITrans, IDraw
    {
        public Trans Trans { get; } = new Trans();
        public Draw Draw { get; } = new Draw();

        public MiloString Material { get; set; }
        public Sphere Origin { get; set; }

        public int Strength { get; set; }

        public override MiloString Type => "Flare";
    }
}
