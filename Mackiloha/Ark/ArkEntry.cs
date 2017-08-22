using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mackiloha.Ark
{
    public class ArkEntry
    {
        private readonly ArkFile _ark;
        private readonly string _filePath;
        private readonly string _directoryPath;

        internal ArkEntry(ArkFile ark, long offset, string filePath, string directoryPath, uint size, uint inflatedSize)
        {
            this._ark = ark;
            this._filePath = filePath;
            this._directoryPath = directoryPath;

            this.Offset = offset;
            this.Size = size;
            this.InflatedSize = inflatedSize;
        }
        
        public long Offset { get; } // Should this be public?
        public string DirectoryPath => this._directoryPath;
        public string FilePath => this._filePath;
        public uint Size { get; }
        public uint InflatedSize { get; } // 0 = Already inflated

        public string FullPath
        {
            get
            {
                return string.IsNullOrEmpty(this._directoryPath) ? this._filePath : $"{this._directoryPath}/{this._filePath}";
            }
        }
    }
}
