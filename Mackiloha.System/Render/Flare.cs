using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.System.Render
{
    public class Flare : RenderObject, ITrans, IDraw
    {
        public Trans Trans => new Trans();
        public Draw Draw => new Draw();

        public MiloString Material { get; set; }
        public Sphere Origin { get; set; }

        public int Strength { get; set; }
    }
}
