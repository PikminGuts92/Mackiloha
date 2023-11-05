using ArkHelper.Exceptions;
using ArkHelper.Helpers;
using ArkHelper.Options;
using Mackiloha;
using Mackiloha.App;
using Mackiloha.App.Extensions;
using Mackiloha.Ark;
using Mackiloha.CSV;
using Mackiloha.IO;
using Mackiloha.Milo2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Mackiloha.FileHelper;

namespace ArkHelper.Apps
{
    public class Ark2DirApp
    {
        protected readonly IScriptHelper ScriptHelper;

        public Ark2DirApp(IScriptHelper scriptHelper)
        {
            ScriptHelper = scriptHelper;
        }

        private string CombinePath(string basePath, string path)
        {
            // Consistent slash
            basePath = FixSlashes(basePath ?? "");
            path = FixSlashes(path ?? "");

            path = ReplaceDotsInPath(path);
            return Path.Combine(basePath, path);
        }

        private string ReplaceDotsInPath(string path)
        {
            var dotRegex = new Regex(@"[.]+[\/\\]");

            if (dotRegex.IsMatch(path))
            {
                // Replaces dotdot path
                path = dotRegex.Replace(path, x => $"({x.Value.Substring(0, x.Value.Length - 1)}){x.Value.Last()}");
            }

            return path;
        }

        private string ExtractEntry(Archive ark, ArkEntry entry, string filePath)
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

        public void Parse(Ark2DirOptions op)
        {
            var scriptRegex = new Regex("(?i).((dtb)|(dta)|(([A-Z]+)(_dta_)([A-Z0-9]+)))$");
            var scriptForgeRegex = new Regex("(?i)(_dta_)([A-Z0-9]+)$");
            var csvRegex = new Regex("(?i).csv_([A-Z0-9]+)$");

            var dtaRegex = new Regex("(?i).dta$");
            var textureRegex = new Regex("(?i).((bmp)|(png))(_[A-Z0-9]+)$");
            var miloRegex = new Regex("(?i).((gh)|(milo)|(rnd))(_[A-Z0-9]+)?$");

            var genPathedFile = new Regex(@"(?i)([\/\\]?(([^\/\\]+[\/\\])*))(gen[\/\\])([^\/\\]+)$");
            var platformExtRegex = new Regex(@"(?i)_([A-Z0-9]+)$");

            Archive ark;
            int arkVersion;
            bool arkEncrypted;

            if (Directory.Exists(op.InputPath))
            {
                // Open as directory
                ark = ArkFileSystem.FromDirectory(op.InputPath);

                // TODO: Get from args probably
                arkVersion = 10;
                arkEncrypted = true;
            }
            else
            {
                // Open as ark
                var arkFile = ArkFile.FromFile(op.InputPath);
                arkVersion = (int)arkFile.Version;
                arkEncrypted = arkFile.Encrypted;

                ark = arkFile;
            }

            var scriptsToConvert = ark.Entries
                .Where(x => op.ConvertScripts
                    && arkVersion >= 3 // Amp dtbs not supported right now
                    && scriptRegex.IsMatch(x.FullPath))
                .ToList();

            var csvsToConvert = ark.Entries
                .Where(x => op.ConvertScripts
                    && csvRegex.IsMatch(x.FullPath))
                .ToList();

            var texturesToConvert = ark.Entries
                .Where(x => op.ConvertTextures
                    && textureRegex.IsMatch(x.FullPath))
                .ToList();

            var milosToInflate = ark.Entries
                .Where(x => op.InflateMilos
                    && !op.ExtractMilos
                    && miloRegex.IsMatch(x.FullPath))
                .ToList();

            var milosToExtract = ark.Entries
                .Where(x => op.ExtractMilos
                    && miloRegex.IsMatch(x.FullPath))
                .ToList();

            var entriesToExtract = ark.Entries
                .Where(x => op.ExtractAll)
                .Except(scriptsToConvert)
                .Except(texturesToConvert)
                .Except(milosToInflate)
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

            foreach (var textureEntry in texturesToConvert)
            {
                using var arkEntryStream = ark.GetArkEntryFileStream(textureEntry);

                var filePath = CombinePath(op.OutputPath, textureEntry.FullPath);
                var pngPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(filePath)), Path.GetFileNameWithoutExtension(filePath) + ".png");

                // Removes gen sub directory
                if (genPathedFile.IsMatch(pngPath))
                {
                    var match = genPathedFile.Match(pngPath);
                    pngPath = $"{match.Groups[1]}{match.Groups[5]}";
                }

                var info = new SystemInfo()
                {
                    Version = 10,
                    Platform = Platform.PS2,
                    BigEndian = false
                };

                var serializer = new MiloSerializer(info);
                var bitmap = serializer.ReadFromStream<HMXBitmap>(arkEntryStream);

                bitmap.SaveAs(info, pngPath);
                Console.WriteLine($"Wrote \"{pngPath}\"");
            }

            foreach (var miloEntry in milosToInflate)
            {
                var filePath = ExtractEntry(ark, miloEntry, CombinePath(op.OutputPath, miloEntry.FullPath));

                // Inflate milo
                var milo = MiloFile.ReadFromFile(filePath);
                milo.Structure = BlockStructure.MILO_A;
                milo.WriteToFile(filePath);

                Console.WriteLine($"Wrote \"{filePath}\"");
            }

            foreach (var miloEntry in milosToExtract)
            {
                var filePath = ExtractEntry(ark, miloEntry, CombinePath(op.OutputPath, miloEntry.FullPath));
                var dirPath = Path.GetDirectoryName(filePath);

                var tempPath = filePath + "_temp";
                File.Move(filePath, tempPath, true);

                var extPath = Path.Combine(
                    Path.GetDirectoryName(filePath),
                    Path.GetFileName(filePath));

                var state = new AppState(dirPath);
                state.ExtractMiloContents(
                    Path.GetFileName(tempPath),
                    extPath,
                    op.ConvertTextures);

                // TODO: Refactor IDirectory and remove temp file write/delete
                File.Delete(tempPath);

                Console.WriteLine($"Wrote \"{extPath}\"");
            }

            foreach (var csvEntry in csvsToConvert)
            {
                var csvStream = ark.GetArkEntryFileStream(csvEntry);
                var csv = CSVFile.FromForgeCSVStream(csvStream);

                // Write to file
                var csvPath = CombinePath(op.OutputPath, csvEntry.FullPath);
                csvPath = platformExtRegex.Replace(csvPath, "");

                csv.SaveToFileAsCSV(csvPath);
                Console.WriteLine($"Wrote \"{csvPath}\"");
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
                    ? $"{dtaPath.Substring(0, dtaPath.Length - 1)}a" // Simply change b -> a
                    : scriptForgeRegex.Replace(dtaPath, "");

                // Removes gen sub directory
                if (genPathedFile.IsMatch(dtaPath))
                {
                    var match = genPathedFile.Match(dtaPath);
                    dtaPath = $"{match.Groups[1]}{match.Groups[5]}";
                }

                var tempDtbPath = ExtractEntry(ark, scriptEntry, Path.Combine(tempDir, Path.GetRandomFileName()));

                try
                {
                    ScriptHelper.ConvertDtbToDta(tempDtbPath, tempDir, arkEncrypted, arkVersion, dtaPath, op.IndentSize);
                    Console.WriteLine($"Wrote \"{dtaPath}\"");
                    successDtas++;
                }
                catch (DTBParseException)
                {
                    Console.WriteLine($"Unable to convert to script, skipping \'{scriptEntry.FullPath}\'");
                    if (File.Exists(dtaPath))
                        File.Delete(dtaPath);
                }
                catch (Exception)
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
