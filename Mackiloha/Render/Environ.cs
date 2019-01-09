using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.Render
{
    public interface IEnviron : IRenderObject
    {
        List<List<MiloString>> Drawables { get; }
        List<float> Values { get; }
    }

    public class Environ : RenderObject, IEnviron
    {
        // Environ
        public List<List<MiloString>> Drawables { get; } = new List<List<MiloString>>();
        public List<float> Values { get; } = new List<float>();

        public override MiloString Type => "Environ";
    }
}
