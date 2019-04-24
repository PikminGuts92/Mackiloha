using System;
using System.Collections.Generic;
using System.Text;
using Mackiloha.IO;

namespace Mackiloha.App
{
    public class AppState
    {
        private readonly IDirectory _workingDirectory;
        private SystemInfo _systemInfo;

        public AppState(string workingDirectory)
        {
            _workingDirectory = new FileSystemDirectory(workingDirectory);
            _systemInfo.Version = 10; // GH1
        }

        public void UpdateSystemInfo(SystemInfo info)
        {
            // TODO: Add version verification?
            _systemInfo = info;
        }

        public IDirectory GetWorkingDirectory() => _workingDirectory;
        public SystemInfo SystemInfo => _systemInfo;
    }
}
