using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.System.Render
{
    public class Environ : RenderObject
    {
        public List<List<MiloString>> Drawables { get; } = new List<List<MiloString>>();
        public List<float> Values { get; } = new List<float>();
    }
}
