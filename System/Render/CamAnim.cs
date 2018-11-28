using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.System.Render.Interfaces;

namespace Mackiloha.System.Render
{
    public class CamAnim : RenderObject, IAnim
    {
        public Anim Anim => new Anim();

        MiloString Camera { get; set; }
        MiloString Animation { get; set; }
    }
}
