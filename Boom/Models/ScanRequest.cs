using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Boom.Models
{
    public class ScanRequest
    {
        public string InputPath { get; set; }
        public string OutputPath { get; set; }
        public string GameTitle { get; set; }
        public string Platform { get; set; }
        public string Region { get; set; }
    }
}
