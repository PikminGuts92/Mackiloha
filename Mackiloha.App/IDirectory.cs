using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Mackiloha.App
{
    public interface IDirectory
    {
        string FullPath { get; }
        string Name { get; }
        string[] GetFiles();
        IDirectory[] GetSubDirectories();
        bool IsLeaf();
        Stream GetStreamForFile(string fileName);
    }
}
