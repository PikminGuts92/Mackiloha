using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boom.Data.MiloEntities
{
    public class MiloEntry
    {
        public int Id { get; set; }

        public int MiloId { get; set; }
        public Milo Milo { get; set; }
        
        public string Name { get; set; }
        public string Type { get; set; }

        public int Size { get; set; }
        public int Magic { get; set; }
    }
}
