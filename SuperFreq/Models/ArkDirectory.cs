using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mackiloha.Ark;

namespace SuperFreq.Models
{
    public class ArkDirectory : INode
    {
        private readonly Archive _archive;
        private readonly string _path;


        public ArkDirectory(Archive archive, string path)
        {
            _archive = archive;
            _path = path;

            Name = Path.GetFileName(path);
        }

        public string Name { get; set; }
        public bool IsSelected { get; set; } = false;
        public bool IsExpanded { get; set; } = false;

        public List<INode> Children => throw new NotImplementedException();
    }
}
