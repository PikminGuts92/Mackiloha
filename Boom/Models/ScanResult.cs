using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boom.Models
{
    public class ScanResult
    {
        public int TotalArkEntries { get; set; }
        public int TotalMilos { get; set; }
        public int TotalMiloEntries { get; set; }
        public long TimeElapsed { get; set; }
    }
}
