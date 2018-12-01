using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render.Interfaces;

namespace Mackiloha.Render
{
    public class Light : RenderObject, ITrans
    {
        public Trans Trans => new Trans();

        public Sphere Origin;
        public float KeyFrame;

        public override MiloString Type => "Light";
    }
}
