using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public struct AnimEntry
    {
        public MiloString Name;
        public float F1;
        public float F2;
    }

    public interface IAnim : IRenderObject
    {
        List<AnimEntry> AnimEntries { get; }
        List<MiloString> Animatables { get; }
    }

    public class Anim : RenderObject, IAnim
    {
        // Anim
        public List<AnimEntry> AnimEntries { get; } = new List<AnimEntry>();
        public List<MiloString> Animatables { get; } = new List<MiloString>();

        public override MiloString Type => "Anim";
    }
}
