using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render.Interfaces;

namespace Mackiloha.Render
{
    public class View : RenderObject, IAnim, IDraw, ITrans
    {
        public Anim Anim => new Anim();
        public Trans Trans => new Trans();
        public Draw Draw => new Draw();

        public MiloString MainView { get; set; }
    }
}
