using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public class Draw : RenderObject
    {
        public bool Showing { get; set; } = true;

        public List<MiloString> Drawables { get; } = new List<MiloString>();
        public Sphere Boundry { get; set; }
    }
}
