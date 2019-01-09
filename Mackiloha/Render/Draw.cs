using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public interface IDraw : IRenderObject
    {
        bool Showing { get; set; }

        List<MiloString> Drawables { get; }
        Sphere Boundry { get; set; }
    }

    public class Draw : RenderObject, IDraw
    {
        // Draw
        public bool Showing { get; set; } = true;

        public List<MiloString> Drawables { get; } = new List<MiloString>();
        public Sphere Boundry { get; set; }

        public override MiloString Type => "Draw";
    }
}
