using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mackiloha.Ark
{
    public abstract class FileEntry
    {
        private readonly static Regex _directoryRegex = new Regex(@"^[_\-a-zA-Z0-9]|([/][_\-a-zA-Z0-9]+)*$");
        private readonly static Regex _fileRegex = new Regex(@"^[_\-a-zA-Z0-9]+[.]?[_\-a-zA-Z0-9]*$");

        public string FileName { get; }
        public string DirectoryName { get; }

        private bool IsValidPath(string text, bool directory = false)
        {
            if (directory)
                return _directoryRegex.IsMatch(text) || (text == string.Empty);

            return _fileRegex.IsMatch(text);
        }
    }
}
