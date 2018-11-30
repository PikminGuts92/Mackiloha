using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha
{
    public class MiloObjectBytes : MiloObject, ISerializable
    {
        public MiloObjectBytes(string type) : base()
        {
            Type = type;
        }
        
        public byte[] Data { get; set; }
    }
}
