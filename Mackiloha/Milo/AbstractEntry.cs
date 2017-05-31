using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mackiloha.Milo
{
    public abstract class AbstractEntry
    {
        /// <summary>
        /// Abstract milo entry
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type</param>
        /// <param name="bigEndian">Big endian?</param>
        public AbstractEntry(string name, string type, bool bigEndian = true)
        {
            Name = name;
            Type = type;
            BigEndian = bigEndian;
        }

        public string Name { get; set; }
        public virtual string Type { get; }
        public abstract byte[] Data { get; }
        public bool BigEndian { get; set; }
    }
}
