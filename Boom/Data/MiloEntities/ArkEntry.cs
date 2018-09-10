using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boom.Data.MiloEntities
{
    public class ArkEntry
    {
        public int Id { get; set; }

        public int ArkId { get; set; }
        public Ark Ark { get; set; }

        public string Path { get; set; }
        public int Part { get; set; }
        public long Offset { get; set; }
        public int Size { get; set; }
        public int InflatedSize { get; set; }
    }
}
