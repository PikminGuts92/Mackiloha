using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.System.Render
{
    public class Trans : RenderObject
    {
        public Matrix4 Mat1 { get; set; }
        public Matrix4 Mat2 { get; set; }

        public List<MiloString> Transformables { get; } = new List<MiloString>();
        public MiloString Camera { get; set; }
        public MiloString Transform { get; set; }
    }
}
