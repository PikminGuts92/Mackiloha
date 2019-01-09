using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public interface IFlare : IRenderObject
    {

    }

    public class Flare : RenderObject, IFlare, ITrans, IDraw
    {
        internal Trans Trans { get; } = new Trans();
        internal Draw Draw { get; } = new Draw();

        // Trans
        public Matrix4 Mat1 { get => Trans.Mat1; set => Trans.Mat1 = value; }
        public Matrix4 Mat2 { get => Trans.Mat2; set => Trans.Mat2 = value; }

        public List<MiloString> Transformables => Trans.Transformables;

        public int UnknownInt { get => Trans.UnknownInt; set => Trans.UnknownInt = value; }
        public MiloString Camera { get => Trans.Camera; set => Trans.Camera = value; }
        public bool UnknownBool { get => Trans.UnknownBool; set => Trans.UnknownBool = value; }

        public MiloString Transform { get => Trans.Transform; set => Trans.Transform = value; }

        // Draw
        public bool Showing { get => Draw.Showing; set => Draw.Showing = value; }

        public List<MiloString> Drawables => Draw.Drawables;
        public Sphere Boundry { get => Draw.Boundry; set => Draw.Boundry = value; }

        // Flare
        public MiloString Material { get; set; }
        public Sphere Origin { get; set; }

        public int Strength { get; set; }

        public override MiloString Type => "Flare";
    }
}
