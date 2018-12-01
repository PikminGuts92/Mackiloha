using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha
{
    public class MiloObjectBytes : MiloObject, ISerializable
    {
        private readonly MiloString _type;

        public MiloObjectBytes(string type) : base()
        {
            _type = type;
        }
        
        public byte[] Data { get; set; }

        public override MiloString Type => _type;
    }
}
