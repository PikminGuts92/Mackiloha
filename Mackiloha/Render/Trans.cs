using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public class Trans : RenderObject, ISerializable
    {
        public Matrix4 Mat1 { get; set; }
        public Matrix4 Mat2 { get; set; }

        public List<MiloString> Transformables { get; } = new List<MiloString>();

        public int UnknownInt { get; set; }
        public MiloString Camera { get; set; }
        public bool UnknownBool { get; set; }

        public MiloString Transform { get; set; }

        public override MiloString Type => "Trans";
    }
}
