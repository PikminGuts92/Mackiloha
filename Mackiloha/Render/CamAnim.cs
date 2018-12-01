using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.Render.Interfaces;

namespace Mackiloha.Render
{
    public class CamAnim : RenderObject, IAnim
    {
        public Anim Anim => new Anim();

        MiloString Camera { get; set; }
        MiloString Animation { get; set; }

        public override MiloString Type => "CamAnim";
    }
}
