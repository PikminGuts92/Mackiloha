using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mackiloha.Milo2
{
    public class MiloEntry : IMiloEntry
    {
        public MiloEntry(string name, string type, byte[] data)
        {
            Name = name;
            Type = type;
            Data = data;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public byte[] Data { get; set; }
    }
}
