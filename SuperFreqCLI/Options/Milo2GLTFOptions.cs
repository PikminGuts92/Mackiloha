using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;
using Mackiloha;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Mackiloha.IO;

namespace SuperFreqCLI.Options
{
    [Verb("milo2gltf", HelpText = "Converts milo scene to GLTF", Hidden = true)]
    internal class Milo2GLTFOptions
    {
        [Value(0, Required = true, MetaName = "miloPath", HelpText = "Path to input milo archive")]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "gltfPath", HelpText = "Path to output gltf file")]
        public string OutputPath { get; set; }

        public static void Parse(Milo2GLTFOptions op)
        {
            var appState = new AppState(Path.GetDirectoryName(op.InputPath));
            var info = new SystemInfo()
            {
                Version = 10,
                Platform = Platform.PS2,
                BigEndian = false
            };

            appState.UpdateSystemInfo(info);
            
            var milo = appState.OpenMiloFile(op.InputPath);
            milo.ExportToGLTF(op.OutputPath, appState);
        }
    }
}
