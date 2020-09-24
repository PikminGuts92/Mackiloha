using Mackiloha;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Mackiloha.IO;
using Mackiloha.Milo2;
using P9SongTool.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace P9SongTool.Apps
{
    public class Milo2ProjectApp
    {
        public void Parse(Milo2ProjectOptions op)
        {
            var appState = AppState.FromFile(op.InputPath);
            appState.UpdateSystemInfo(GetSystemInfo(op));

            var milo = appState.OpenMiloFile(op.InputPath);
        }

        protected SystemInfo GetSystemInfo(Milo2ProjectOptions op)
            => new SystemInfo()
            {
                Version = 25,
                BigEndian = true,
                Platform = op.InputPath
                    .ToLower()
                    .EndsWith("_ps3")
                    ? Platform.PS3
                    : Platform.X360
            };
    }
}
