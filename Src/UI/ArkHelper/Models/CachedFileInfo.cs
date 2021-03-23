using System;
using System.Collections.Generic;
using System.Text;

namespace ArkHelper.Models
{
    public class CachedFileInfo
    {
        public string SourcePath { get; set; }
        public string InternalPath { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
