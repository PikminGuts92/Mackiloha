using Mackiloha;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Mackiloha.IO;
using Mackiloha.Milo2;
using P9SongTool.Exceptions;
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
        protected readonly string[] Characters = new[] { "john", "george", "paul", "ringo"  };

        public void Parse(Milo2ProjectOptions op)
        {
            var appState = AppState.FromFile(op.InputPath);
            appState.UpdateSystemInfo(GetSystemInfo(op));

            var milo = appState.OpenMiloFile(op.InputPath);

            // Create output directory
            if (!Directory.Exists(op.OutputPath))
                Directory.CreateDirectory(op.OutputPath);

            var entries = GetEntries(milo);
            ExtractLipsync(entries, op.OutputPath);
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

        protected List<MiloObject> GetEntries(MiloObjectDir miloDir)
        {
            var entries = new List<MiloObject>();
            GetEntries(miloDir, entries);
            return entries;
        }

        protected void GetEntries(MiloObjectDir miloDir, List<MiloObject> entries)
        {
            if (miloDir.Type != "ObjectDir")
                throw new UnsupportedMiloException($"Directory type of \"{miloDir.Type}\" found, expected \"ObjectDir\" expected");

            // Traverse sub directories
            if (!(miloDir.Extras["DirectoryEntry"] is MiloObjectDirEntry dirEntry))
                throw new UnsupportedMiloException("Could not parse directory entry");

            foreach (var subDir in dirEntry.SubDirectories)
            {
                GetEntries(subDir, entries);
            }

            // Add entries in current directory
            entries.AddRange(miloDir.Entries);
        }

        protected void ExtractLipsync(List<MiloObject> entries, string dirPath)
        {
            var lipsyncEntries = entries
                .Where(x => x.Type == "CharLipSync")
                .ToList();

            foreach (var lipsync in lipsyncEntries)
            {
                var lipFilePath = Path.Combine(dirPath, lipsync.Name);

                var lipBytes = lipsync as MiloObjectBytes; // Should always be this
                File.WriteAllBytes(lipFilePath, lipBytes.Data);
            }
        }
    }
}
