using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha
{
    public abstract class MiloObject
    {
        public MiloString Name { get; set; }
        public virtual MiloString Type { get; internal set; } = "Object";
    }
}
