using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.System.Render
{
    public class Light : RenderObject, ITrans
    {
        public Trans Trans => new Trans();

        public Sphere Origin;
        public float KeyFrame;
    }
}
