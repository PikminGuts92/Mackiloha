using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Mackiloha.Ark
{
    public class ArkEntry
    {
        private readonly Archive _ark;
        private readonly string _filePath;
        private readonly string _directoryPath;
        private readonly int _part;

        private string _tempFile;
        private ArkEntryStatus _status;

        internal ArkEntry(Archive ark, long offset, string filePath, string directoryPath, uint size, uint inflatedSize, int part)
        {
            this._ark = ark;
            this._filePath = filePath;
            this._directoryPath = directoryPath;
            this._part = part;

            this.Offset = offset;
            this.Size = size;
            this.InflatedSize = inflatedSize;

            this._tempFile = null;
            this._status = ArkEntryStatus.Original;
        }

        public bool Import(string path, bool overwritePending = false)
        {
            // TODO: Also check if ark temp directory is set
            if (!File.Exists(path)) return false;

            // Creates temp file
            string tempPath = (this._tempFile != null) ? this._tempFile : $"{this._ark.WorkingDirectory}/{Guid.NewGuid()}"; 
            File.Copy(path, tempPath);

            this._status = ArkEntryStatus.PendingChanges;
            return true;
        }
        
        public byte[] GetBytes()
        {
            string arkPath = this._ark.ArkPath(this._part);
            byte[] data = new byte[this.Size];

            using (FileStream fs = File.OpenRead(arkPath))
            {
                fs.Seek(this.Offset, SeekOrigin.Begin);
                fs.Read(data, 0, data.Length);
            }

            return data; // Not very efficient or super smart at the moment
        }

        public Stream GetStream() => new MemoryStream(this.GetBytes(), false); // Read-only

        public long Offset { get; } // Should this be public?
        public string DirectoryName => this._directoryPath;
        public string FileName => this._filePath;
        public uint Size { get; }
        public uint InflatedSize { get; } // 0 = Already inflated

        public string FullPath => string.IsNullOrEmpty(this._directoryPath) ? this._filePath : $"{this._directoryPath}/{this._filePath}";
        public ArkEntryStatus Status => this._status;

        public override string ToString() => $"{FullPath} ({Size} bytes)";
    }
}
