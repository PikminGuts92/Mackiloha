using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha
{
    public interface IMiloObject : ISerializable
    {
        MiloString Name { get; set; }
        MiloString Type { get; }
    }

    public abstract class MiloObject : IMiloObject
    {        
        public MiloString Name { get; set; }
        public abstract MiloString Type { get; }

        public override string ToString()
            => Name != "" ? $"{Type}: {Name}" : (string)Type;
    }
}
