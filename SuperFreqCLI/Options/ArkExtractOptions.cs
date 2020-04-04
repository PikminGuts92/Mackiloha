using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CliWrap;
using CommandLine;
using Mackiloha;
using Mackiloha.Ark;
using Mackiloha.DTB;
using SuperFreqCLI.Exceptions;

namespace SuperFreqCLI.Options
{
    [Verb("arkextract", HelpText = "Extracts files from milo arks", Hidden = true)]
    public class ArkExtractOptions
    {
        [Value(0, Required = true, MetaName = "arkPath", HelpText = "Path to ark (hdr file)")]
        public string InputPath { get; set; }

        [Value(1, Required = true, MetaName = "dirPath", HelpText = "Path to output directory")]
        public string OutputPath { get; set; }

        [Option('s', "convertScripts", HelpText = "Convert dtb scripts to dta")]
        public bool ConvertScripts { get; set; }

        [Option('m', "extractMilos", HelpText = "Extract milo archives")]
        public bool ExtractMilos { get; set; }

        [Option('a', "extractAll", HelpText = "Extract everything")]
        public bool ExtractAll { get; set; }

        private static void WriteOutput(string text)
            => Console.WriteLine(text);

        private static void ConvertNewDtbToOld(string newDtbPath, string oldDtbPath, bool fme = false)
        {
            var encoding = fme ? DTBEncoding.FME : DTBEncoding.RBVR;

            var dtb = DTBFile.FromFile(newDtbPath, encoding);
            dtb.Encoding = DTBEncoding.Classic;
            dtb.SaveToFile(oldDtbPath);
        }

        private static string CreateDTAFile(string dtbPath, string tempDir, bool newEncryption, int arkVersion, string dtaPath = null)
        {
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            var decDtbPath = Path.Combine(tempDir, Path.GetRandomFileName());
            dtaPath = dtaPath ?? Path.Combine(tempDir, Path.GetRandomFileName());

            var dtaDir = Path.GetDirectoryName(dtaPath);
            if (!Directory.Exists(dtaDir))
                Directory.CreateDirectory(dtaDir);

            if (arkVersion < 7)
            {
                // Decrypt dtb
                Cli.Wrap("dtab")
                    .SetArguments(new[]
                    {
                        newEncryption ? "-d" : "-D",
                        dtbPath,
                        decDtbPath
                    })
                    .Execute();
            }
            else
            {
                // New dtb style, convert to old format to use dtab
                //  and assume not encrypted
                ConvertNewDtbToOld(dtbPath, decDtbPath, arkVersion < 9);
            }

            // Convert to dta (plaintext)
            var result = Cli.Wrap("dtab")
                .SetArguments(new[]
                {
                    "-a",
                    decDtbPath,
                    dtaPath
                })
                .EnableExitCodeValidation(false)
                .SetStandardOutputCallback(WriteOutput)
                .SetStandardErrorCallback(WriteOutput)
                .Execute();

            if (result.ExitCode != 0)
                throw new DTBParseException($"dtab.exe was unable to parse file from \'{decDtbPath}\'");

            return dtaPath;
        }

        private static string CombinePath(string basePath, string path)
        {
            // Consistent slash
            basePath = (basePath ?? "").Replace("/", "\\");
            path = (path ?? "").Replace("/", "\\");

            path = ReplaceDotsInPath(path);
            return Path.Combine(basePath, path);
        }

        private static string ReplaceDotsInPath(string path)
        {
            var dotRegex = new Regex(@"[.]+[\/\\]");

            if (dotRegex.IsMatch(path))
            {
                // Replaces dotdot path
                path = dotRegex.Replace(path, x => $"({x.Value.Substring(0, x.Value.Length - 1)}){x.Value.Last()}");
            }

            return path;
        }

        private static string ExtractEntry(ArkFile ark, ArkEntry entry, string filePath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            using (var fs = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                using (var stream = ark.GetArkEntryFileStream(entry))
                {
                    stream.CopyTo(fs);
                }
            }

            return filePath;
        }

        public static void Parse(ArkExtractOptions op)
        {
            var scriptRegex = new Regex("(?i).((dtb)|(dta)|(([A-Z]+)(_dta_)([A-Z0-9]+)))$");
            var scriptForgeRegex = new Regex("(?i)(_dta_)([A-Z0-9]+)$");

            var dtaRegex = new Regex("(?i).dta$");
            var miloRegex = new Regex("(?i).milo(_[A-Z0-9]+)?$");

            var genPathedFile = new Regex(@"(?i)(([^\/\\]+[\/\\])*)(gen[\/\\])([^\/\\]+)$");

            var ark = ArkFile.FromFile(op.InputPath);
            var arkVersion = (int)ark.Version;

            var scriptsToConvert = ark.Entries
                .Where(x => op.ConvertScripts
                    && scriptRegex.IsMatch(x.FullPath))
                .ToList();

            var milosToExtract = ark.Entries
                .Where(x => op.ExtractMilos
                    && miloRegex.IsMatch(x.FullPath))
                .ToList();

            var entriesToExtract = ark.Entries
                .Where(x => op.ExtractAll)
                .Except(scriptsToConvert)
                .Except(milosToExtract)
                .ToList();

            foreach (var arkEntry in entriesToExtract)
            {
                var filePath = ExtractEntry(ark, arkEntry, CombinePath(op.OutputPath, arkEntry.FullPath));
                Console.WriteLine($"Wrote \"{filePath}\"");
            }

            // Create temp path
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            foreach (var miloEntry in milosToExtract)
            {
                // TODO: Implement milo archive extraction
            }

            var successDtas = 0;
            foreach (var scriptEntry in scriptsToConvert)
            {
                // Just extract file if dta script
                if (dtaRegex.IsMatch(scriptEntry.FullPath))
                {
                    var filePath = ExtractEntry(ark, scriptEntry, CombinePath(op.OutputPath, scriptEntry.FullPath));
                    Console.WriteLine($"Wrote \"{filePath}\"");
                    continue;
                }

                // Creates output path
                var dtaPath = CombinePath(op.OutputPath, scriptEntry.FullPath);
                dtaPath = !scriptForgeRegex.IsMatch(dtaPath)
                    ?  $"{dtaPath.Substring(0, dtaPath.Length - 1)}a" // Simply change b -> a
                    : scriptForgeRegex.Replace(dtaPath, "");

                // Removes gen sub directory
                if (genPathedFile.IsMatch(dtaPath))
                {
                    var match = genPathedFile.Match(dtaPath);
                    dtaPath = $"{match.Groups[1]}{match.Groups[4]}";
                }

                var tempDtbPath = ExtractEntry(ark, scriptEntry, Path.Combine(tempDir, Path.GetRandomFileName()));

                try
                {
                    CreateDTAFile(tempDtbPath, tempDir, ark.Encrypted, arkVersion, dtaPath);
                    Console.WriteLine($"Wrote \"{dtaPath}\"");
                    successDtas++;
                }
                catch (DTBParseException ex)
                {
                    Console.WriteLine($"Unable to convert to script, skipping \'{scriptEntry.FullPath}\'");
                    if (File.Exists(dtaPath))
                        File.Delete(dtaPath);
                }
                catch (Exception ex)
                {

                }
            }

            if (scriptsToConvert.Count > 0)
                Console.WriteLine($"Converted {successDtas} of {scriptsToConvert.Count} scripts");

            // Clean up temp files
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
