using System;
using System.Collections.Generic;
using System.Text;

namespace Mackiloha.App
{
    public class AppState
    {
        private readonly IDirectory _workingDirectory;

        public AppState(string workingDirectory)
        {
            _workingDirectory = new FileSystemDirectory(workingDirectory);
        }

        public IDirectory GetWorkingDirectory() => _workingDirectory;
    }
}
