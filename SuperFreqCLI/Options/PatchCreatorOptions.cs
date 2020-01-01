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
using SuperFreqCLI.Models;

namespace SuperFreqCLI.Options
{
    [Verb("patchcreator", HelpText = "Creates a patch for arks", Hidden = true)]
    public class PatchCreatorOptions
    {
        [Value(0, Required = true, MetaName = "arkPath", HelpText = "Path to ark (hdr file)")]
        public string InputPath { get; set; }

        [Value(1, Required = false, MetaName = "exePath", HelpText = "Path to decrypted executable")]
        public string ExePath { get; set; }

        [Option('h', "hashPath", HelpText = "Path to file containing hash offsets", Required = false)]
        public string HashesPath { get; set; }

        [Option('a', "arkFilesPath", HelpText = "Path to directory for ark files", Required = true)]
        public string ArkFilesPath { get; set; }

        [Option('o', "outputPath", HelpText = "Path to directory for patch", Required = false)]
        public string OutputPath { get; set; }

        private static string CreateTempDTBFile(string dtaPath, string tempDir, bool newEncryption)
        {
            if (!Directory.Exists(tempDir))
                Directory.CreateDirectory(tempDir);

            var dtbPath = Path.Combine(tempDir, Path.GetRandomFileName());
            var encDtbPath = Path.Combine(tempDir, Path.GetRandomFileName());

            // Convert to dtb
            Cli.Wrap("dtab")
                .SetArguments(new[]
                {
                    "-b",
                    dtaPath,
                    dtbPath
                })
                .Execute();

            // Encrypt dtb
            Cli.Wrap("dtab")
                .SetArguments(new[]
                {
                    newEncryption ? "-e" : "-E",
                    dtbPath,
                    encDtbPath
                })
                .Execute();

            return encDtbPath;
        }

        public static void Parse(PatchCreatorOptions op)
        {
            var ark = ArkFile.FromFile(op.InputPath);

            var patchPartName = $"{Path.GetFileNameWithoutExtension(op.InputPath)}_{ark.PartCount()}.ark";
            ark.AddAdditionalPart(Path.Combine(op.OutputPath, "gen", patchPartName));

            var files = Directory.GetFiles(op.ArkFilesPath, "*", SearchOption.AllDirectories);

            // Open hashes
            var entryInfo = ArkEntryInfo.ReadFromCSV(op.HashesPath)
                .ToDictionary(x => x.Path, y => y);

            var updatedHashes = new List<ArkEntryInfo>();

            var dtaRegex = new Regex("(?i).dta$");
            var genPathedFile = new Regex(@"(?i)gen[\/][^\/]+$");
            var dotRegex = new Regex(@"\([.]+\)/");

            // Create temp path
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            foreach (var file in files)
            {
                var internalPath = FileHelper.GetRelativePath(file, op.ArkFilesPath)
                    .Replace("\\", "/");

                string inputFilePath = file;

                if (dtaRegex.IsMatch(internalPath))
                {
                    // Updates path
                    internalPath = $"{internalPath.Substring(0, internalPath.Length - 1)}b";

                    if (!genPathedFile.IsMatch(internalPath))
                        internalPath = internalPath.Insert(internalPath.LastIndexOf('/'), "/gen");

                    // Creates temp dtb file
                    inputFilePath = CreateTempDTBFile(file, tempDir, ark.Encrypted);
                }

                if (dotRegex.IsMatch(internalPath))
                {
                    internalPath = dotRegex.Replace(internalPath, x => $"{x.Value.Substring(1, x.Length - 3)}/");
                }

                var fileName = Path.GetFileName(internalPath);
                var dirPath = Path.GetDirectoryName(internalPath).Replace("\\", "/");

                var pendingEntry = new PendingArkEntry(fileName, dirPath)
                {
                    LocalFilePath = inputFilePath
                };

                ark.AddPendingEntry(pendingEntry);

                if (!entryInfo.TryGetValue(internalPath, out var hashInfo))
                    continue;

                // Update hash
                using var fs = File.OpenRead(inputFilePath);
                hashInfo.Hash = Crypt.SHA1Hash(fs);
                updatedHashes.Add(hashInfo);
            }

            ark.CommitChanges(false);

            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            // Writes header
            var hdrPath = Path.Combine(op.OutputPath, "gen", Path.GetFileName(op.InputPath));
            ark.WriteHeader(hdrPath);

            // Copy exe
            var exePath = Path.Combine(op.OutputPath, Path.GetFileName(op.ExePath));
            File.Copy(op.ExePath, exePath, true);

            if (updatedHashes.Count <= 0)
                return;

            // Patch exe
            using var exeStream = File.OpenWrite(exePath);
            foreach(var hashInfo in updatedHashes)
            {
                var hashBytes = FileHelper.GetBytes(hashInfo.Hash);

                exeStream.Seek(hashInfo.Offset, SeekOrigin.Begin);
                exeStream.Write(hashBytes, 0, hashBytes.Length);
            }
        }
    }
}
