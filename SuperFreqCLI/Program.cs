using System;
using CommandLine;
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

                })
                .WithNotParsed(errors => { });
        }
    }
}
