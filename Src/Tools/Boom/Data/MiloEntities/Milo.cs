using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boom.Data.MiloEntities
{
    public class Milo
    {
        public int Id { get; set; }

        public int ArkEntryId { get; set; }
        public ArkEntry ArkEntry { get; set; }

        public int Version { get; set; }
        public int TotalSize { get; set; }

        // Directory info
        public string Name { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
        public int Magic { get; set; }

        public List<MiloEntry> Entries { get; set; }
    }
}
