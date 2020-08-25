using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boom.Data.MiloEntities
{
    public class Ark
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Platform { get; set; }
        public string Region { get; set; }
        public int ArkVersion { get; set; }

        public List<ArkEntry> Entries { get; set; }
    }
}
