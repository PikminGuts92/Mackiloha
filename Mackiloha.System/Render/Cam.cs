using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.System.Render
{
    public class Cam : RenderObject, ITrans
    {
        public Trans Trans => new Trans();

        public Matrix4 Mat { get; set; }
    }
}
