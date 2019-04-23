using System;
using System.IO;
using CommandLine;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using SuperFreqCLI.Options;

namespace SuperFreqCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Dir2MiloOptions, Milo2DirOptions>(args)
                .WithParsed<Dir2MiloOptions>(OperatingSystem => { })
                .WithParsed<Milo2DirOptions>(op =>
                {
                    var appState = new AppState(Path.GetDirectoryName(op.InputPath));
                    appState.ExtractMiloContents(op.InputPath, op.OutputPath, true);
                })
                .WithNotParsed(errors => { });
        }
    }
}
