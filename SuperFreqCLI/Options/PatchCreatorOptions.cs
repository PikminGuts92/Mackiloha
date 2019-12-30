using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            foreach (var file in files)
            {
                var internalPath = FileHelper.GetRelativePath(file, op.ArkFilesPath)
                    .Replace("\\", "/");

                var fileName = Path.GetFileName(internalPath);
                var dirPath = Path.GetDirectoryName(internalPath).Replace("\\", "/");

                var pendingEntry = new PendingArkEntry(fileName, dirPath)
                {
                    LocalFilePath = file
                };

                ark.AddPendingEntry(pendingEntry);

                if (!entryInfo.TryGetValue(internalPath, out var hashInfo))
                    continue;

                // Update hash
                using var fs = File.OpenRead(file);
                hashInfo.Hash = Crypt.SHA1Hash(fs);
                updatedHashes.Add(hashInfo);
            }

            ark.CommitChanges(false);

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
