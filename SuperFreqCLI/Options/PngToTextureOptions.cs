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
    [Verb("png2tex", HelpText = "Converts HMX texture to png", Hidden = true)]
    internal class PngToTextureOptions : GameOptions
    {
        [Value(0, Required = true, MetaName = "pngPath", HelpText = "Path to input png")]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "texPath", HelpText = "Path to output texture")]
        public string OutputPath { get; set; }

        public static void Parse(PngToTextureOptions op)
        {
            op.UpdateOptions();

            var appState = new AppState(Path.GetDirectoryName(op.InputPath));
            appState.UpdateSystemInfo(op.GetSystemInfo());

            var bitmap = TextureExtensions.BitmapFromImage(op.InputPath, appState.SystemInfo);
            var serializer = appState.GetSerializer();
            serializer.WriteToFile(op.OutputPath, bitmap);
        }
    }
}
