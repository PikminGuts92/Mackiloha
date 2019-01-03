using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render.Interfaces;

namespace Mackiloha.Render
{
    public class View : RenderObject, IAnim, IDraw, ITrans, ISerializable
    {
        public Anim Anim { get; } = new Anim();
        public Trans Trans { get; } = new Trans();
        public Draw Draw { get; } = new Draw();

        public MiloString MainView { get; set; }

        public override MiloString Type => "View";
    }
}
