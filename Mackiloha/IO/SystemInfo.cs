using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.IO
{
    public class SystemInfo
    {
        public string Title { get; set; }
        public Region Region { get; set; }
        public Platform Platform { get; set; }
        public bool BigEndian { get; set; } 
    }
}
