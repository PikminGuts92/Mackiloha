using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public interface ITrans : IRenderObject
    {
        Matrix4 Mat1 { get; set; }
        Matrix4 Mat2 { get; set; }

        List<MiloString> Transformables { get; }

        int UnknownInt { get; set; }
        MiloString Camera { get; set; }
        bool UnknownBool { get; set; }

        MiloString Transform { get; set; }
    }

    public class Trans : RenderObject, ITrans
    {
        // Trans
        public Matrix4 Mat1 { get; set; } = Matrix4.Identity();
        public Matrix4 Mat2 { get; set; } = Matrix4.Identity();

        public List<MiloString> Transformables { get; } = new List<MiloString>();

        public int UnknownInt { get; set; }
        public MiloString Camera { get; set; }
        public bool UnknownBool { get; set; }

        public MiloString Transform { get; set; }

        public override MiloString Type => "Trans";
    }
}
