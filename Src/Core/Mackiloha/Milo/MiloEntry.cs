using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mackiloha.Milo
{
    public class MiloEntry : AbstractEntry
    {
        private byte[] _data;

        /// <summary>
        /// Generic milo entry
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="type">Type</param>
        /// <param name="data">Raw bytes</param>
        /// <param name="bigEndian">Big endian?</param>
        public MiloEntry(string name, string type, byte[] data, bool bigEndian) : base(name, type, bigEndian)
        {
            _data = data;
        }

        /// <summary>
        /// Gets raw bytes
        /// </summary>
        public override byte[] Data { get { return _data; } }

        public override string ToString()
        {
            return string.Format("{0} - {1} ({2} bytes)", Type, Name, _data.Length);
        }
    }
}
