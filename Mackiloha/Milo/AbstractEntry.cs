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

        /// <summary>
        /// Gets name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets type
        /// </summary>
        public virtual string Type { get; }
        /// <summary>
        /// Gets raw bytes
        /// </summary>
        public abstract byte[] Data { get; }
        /// <summary>
        /// Gets or sets endianess
        /// </summary>
        public bool BigEndian { get; set; }
    }
}
