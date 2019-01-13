using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public interface IView : IRenderObject
    {
        string MainView { get; set; }
    }

    public class View : RenderObject, IView, IAnim, ITrans, IDraw
    {
        internal Anim Anim { get; } = new Anim();
        internal Trans Trans { get; } = new Trans();
        internal Draw Draw { get; } = new Draw();

        // Anim
        public List<AnimEntry> AnimEntries => Anim.AnimEntries;
        public List<string> Animatables => Anim.Animatables;

        // Trans
        public Matrix4 Mat1 { get => Trans.Mat1; set => Trans.Mat1 = value; }
        public Matrix4 Mat2 { get => Trans.Mat2; set => Trans.Mat2 = value; }

        public List<string> Transformables => Trans.Transformables;

        public int UnknownInt { get => Trans.UnknownInt; set => Trans.UnknownInt = value; }
        public string Camera { get => Trans.Camera; set => Trans.Camera = value; }
        public bool UnknownBool { get => Trans.UnknownBool; set => Trans.UnknownBool = value; }

        public string Transform { get => Trans.Transform; set => Trans.Transform = value; }

        // Draw
        public bool Showing { get => Draw.Showing; set => Draw.Showing = value; }

        public List<string> Drawables => Draw.Drawables;
        public Sphere Boundry { get => Draw.Boundry; set => Draw.Boundry = value; }

        // View
        public string MainView { get; set; }

        public override string Type => "View";
    }
}
