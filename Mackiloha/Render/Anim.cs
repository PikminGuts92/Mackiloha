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

    public class Anim : RenderObject, ISerializable
    {
        public List<AnimEntry> Entries { get; } = new List<AnimEntry>();
        public List<MiloString> Animatables { get; } = new List<MiloString>();

        public override MiloString Type => "Anim";
    }
}
