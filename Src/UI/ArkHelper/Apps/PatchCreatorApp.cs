using ArkHelper.Models;
using ArkHelper.Options;
using CliWrap;
using Mackiloha;
using Mackiloha.Ark;
using Mackiloha.DTB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArkHelper.Apps
{
    public class PatchCreatorApp
    {
        private void ConvertOldDtbToNew(string oldDtbPath, string newDtbPath, bool fme = false)
        {
            var encoding = fme ? DTBEncoding.FME : DTBEncoding.RBVR;

            var dtb = DTBFile.FromFile(oldDtbPath, DTBEncoding.Classic);
            dtb.Encoding = encoding;
            dtb.SaveToFile(newDtbPath);
        }

        private string CreateTempDTBFile(string dtaPath, string tempDir, bool newEncryption, int arkVersion)
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

            if (arkVersion < 7)
            {
                // Encrypt dtb (binary)
                Cli.Wrap("dtab")
                    .SetArguments(new[]
                    {
                        newEncryption ? "-e" : "-E",
                        dtbPath,
                        encDtbPath
                    })
                    .Execute();
            }
            else
            {
                // Convert
                ConvertOldDtbToNew(dtbPath, encDtbPath, arkVersion < 9);
            }

            return encDtbPath;
        }

        private string GuessPlatform(string arkPath)
        {
            // TODO: Get platform as arg?
            var platformRegex = new Regex("(?i)_([a-z0-9]+)([.]hdr)$");
            var match = platformRegex.Match(arkPath);

            if (!match.Success)
                return null;

            return match
                .Groups[1]
                .Value
                .ToLower();
        }

        public void Parse(PatchCreatorOptions op)
        {
            var ark = ArkFile.FromFile(op.InputPath);
            var inplaceEdit = string.IsNullOrWhiteSpace(op.OutputPath);

            var platformExt = GuessPlatform(op.InputPath);

            if (!inplaceEdit)
            {
                if ((int)ark.Version <= 3)
                {
                    // If ark version doesn't support multiple parts then copy entire ark to new directory
                    ark = ark.CopyToDirectory(Path.Combine(op.OutputPath, "gen"));
                    inplaceEdit = true;
                }
                else
                {
                    // Add additional ark park
                    var patchPartName = $"{Path.GetFileNameWithoutExtension(op.InputPath)}_{ark.PartCount()}.ark";
                    var fullPartPath = ((int)ark.Version < 9)
                        ? Path.Combine(op.OutputPath, "gen", patchPartName)
                        : Path.Combine(op.OutputPath, patchPartName);

                    ark.AddAdditionalPart(fullPartPath);
                }
            }

            var files = Directory.GetFiles(op.ArkFilesPath, "*", SearchOption.AllDirectories);

            // Open hashes
            var entryInfo = (string.IsNullOrWhiteSpace(op.HashesPath)
                ? new List<ArkEntryInfo>()
                : ArkEntryInfo.ReadFromCSV(op.HashesPath))
                .ToDictionary(x => x.Path, y => y);

            var updatedHashes = new List<ArkEntryInfo>();

            var dtaRegex = new Regex("(?i).dta$");
            var genPathedFile = new Regex(@"(?i)gen[\/][^\/]+$");
            var dotRegex = new Regex(@"\([.]+\)/");
            var forgeScriptRegex = new Regex("(?i).((dta)|(fusion)|(moggsong)|(script))$");

            // Create temp path
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            foreach (var file in files)
            {
                var internalPath = FileHelper.GetRelativePath(file, op.ArkFilesPath)
                    .Replace("\\", "/");

                string inputFilePath = file;

                if ((int)ark.Version < 7 && dtaRegex.IsMatch(internalPath))
                {
                    // Updates path
                    internalPath = $"{internalPath.Substring(0, internalPath.Length - 1)}b";

                    if (!genPathedFile.IsMatch(internalPath))
                        internalPath = internalPath.Insert(internalPath.LastIndexOf('/'), "/gen");

                    // Creates temp dtb file
                    inputFilePath = CreateTempDTBFile(file, tempDir, ark.Encrypted, (int)ark.Version);
                }
                else if ((int)ark.Version >= 7 && forgeScriptRegex.IsMatch(internalPath))
                {
                    // Updates path
                    internalPath = $"{internalPath}_dta_{platformExt}";

                    // Creates temp dtb file
                    inputFilePath = CreateTempDTBFile(file, tempDir, ark.Encrypted, (int)ark.Version);
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
                Console.WriteLine($"Added {pendingEntry.FullPath}");

                if (!entryInfo.TryGetValue(internalPath, out var hashInfo))
                    continue;

                // Update hash
                using var fs = File.OpenRead(inputFilePath);
                hashInfo.Hash = Crypt.SHA1Hash(fs);
                updatedHashes.Add(hashInfo);
            }

            ark.CommitChanges(inplaceEdit);

            // Clean up temp files
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            if (!inplaceEdit)
            {
                // Writes header
                var hdrPath = ((int)ark.Version < 9)
                    ? Path.Combine(op.OutputPath, "gen", Path.GetFileName(op.InputPath))
                    : Path.Combine(op.OutputPath, Path.GetFileName(op.InputPath));
                ark.WriteHeader(hdrPath);
            }
            else
            {
                // TODO: Also look at possibly still patching exe
                return;
            }

            // Copy exe
            var exePath = Path.Combine(op.OutputPath, Path.GetFileName(op.ExePath));
            File.Copy(op.ExePath, exePath, true);

            if (updatedHashes.Count <= 0)
                return;

            // Patch exe
            using var exeStream = File.OpenWrite(exePath);
            foreach (var hashInfo in updatedHashes)
            {
                var hashBytes = FileHelper.GetBytes(hashInfo.Hash);

                exeStream.Seek(hashInfo.Offset, SeekOrigin.Begin);
                exeStream.Write(hashBytes, 0, hashBytes.Length);

                Console.WriteLine($"Updated hash for {hashInfo.Path}");
            }
        }
    }
}
