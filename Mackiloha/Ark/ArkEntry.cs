using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mackiloha.Ark
{
    public abstract class ArkEntry
    {
        private readonly static Regex _directoryRegex = new Regex(@"^[_\-a-zA-Z0-9]|([/][_\-a-zA-Z0-9]+)*$"); // TODO: Consider .. and . directories
        private readonly static Regex _fileRegex = new Regex(@"^[_\-a-zA-Z0-9]+[.]?[_\-a-zA-Z0-9]*$");

        public ArkEntry(string fileName, string directory)
        {
            FileName = fileName;
            Directory = directory;
        }

        public string FileName { get; }
        public string Directory { get; }

        private bool IsValidPath(string text, bool directory = false)
        {
            if (directory)
                return _directoryRegex.IsMatch(text) || (text == string.Empty);

            return _fileRegex.IsMatch(text);
        }

        public string FullPath => string.IsNullOrEmpty(Directory) ? FileName : $"{Directory}/{FileName}";

        public override string ToString() => $"{FullPath}";
    }
}
