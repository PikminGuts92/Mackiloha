using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public struct LightEvent
    {
        public Sphere Origin;
        public float KeyFrame;
    }

    public interface ILightAnim : IRenderObject
    {
        MiloString Light { get; set; }
        List<LightEvent> Events { get; }

        MiloString LightAnimation { get; set; }
    }

    public class LightAnim : RenderObject, ILightAnim, IAnim
    {
        internal Anim Anim { get; } = new Anim();

        // Anim
        public List<AnimEntry> AnimEntries => Anim.AnimEntries;
        public List<MiloString> Animatables => Anim.Animatables;

        // LightAnim
        public MiloString Light { get; set; }
        public List<LightEvent> Events { get; } = new List<LightEvent>();

        public MiloString LightAnimation { get; set; }

        public override MiloString Type => "LightAnim";
    }
}
