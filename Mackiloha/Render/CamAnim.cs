using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public interface ICamAnim : IRenderObject
    {
        MiloString Camera { get; set; }
        MiloString Animation { get; set; }
    }

    public class CamAnim : RenderObject, ICamAnim, IAnim
    {
        internal Anim Anim { get; } = new Anim();

        // Anim
        public List<AnimEntry> AnimEntries => Anim.AnimEntries;
        public List<MiloString> Animatables => Anim.Animatables;

        // CamAnim
        public MiloString Camera { get; set; }
        public MiloString Animation { get; set; }

        public override MiloString Type => "CamAnim";
    }
}
