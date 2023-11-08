using ArkHelper.Helpers;
using ArkHelper.Models;
using ArkHelper.Options;
using Mackiloha;
using Mackiloha.Ark;
using System.Text.RegularExpressions;

namespace ArkHelper.Apps;

public class PatchCreatorApp
{
    protected readonly IScriptHelper ScriptHelper;

    public PatchCreatorApp(IScriptHelper scriptHelper)
    {
        ScriptHelper = scriptHelper;
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

        var hdrFileName = new FileInfo(op.InputPath).Name;
        var isUppercase = Path
            .GetFileNameWithoutExtension(hdrFileName)
            .All(c => char.IsUpper(c));

        var genDirName = isUppercase
            ? "GEN"
            : "gen";

        var arkExt = isUppercase
            ? ".ARK"
            : ".ark";

        if (!inplaceEdit)
        {
            if ((int)ark.Version < 3)
            {
                Log.Warning("Hdr version doesn't support multi-part arks, copying single ark for edit");

                // If ark version doesn't support multiple parts then copy entire ark to new directory
                ark = ark.CopyToDirectory(Path.Combine(op.OutputPath, genDirName));
                inplaceEdit = true;
            }
            else
            {
                Log.Information("Adding additional ark part");

                // Add additional ark park
                var patchPartName = $"{Path.GetFileNameWithoutExtension(hdrFileName)}_{ark.PartCount()}{arkExt}";

                var fullPartPath = ((int)ark.Version < 9)
                    ? Path.Combine(op.OutputPath, genDirName, patchPartName)
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
            var internalPath = FileHelper
                .GetRelativePath(file, op.ArkFilesPath)
                .Replace("\\", "/"); // Must be "/" in ark

            string inputFilePath = file;

            if ((int)ark.Version < 7 && dtaRegex.IsMatch(internalPath))
            {
                // Updates path
                internalPath = $"{internalPath.Substring(0, internalPath.Length - 1)}b";

                if (!genPathedFile.IsMatch(internalPath))
                    internalPath = internalPath.Insert(internalPath.LastIndexOf('/'), $"/{genDirName}");

                // Creates temp dtb file
                inputFilePath = ScriptHelper.ConvertDtaToDtb(file, tempDir, ark.Encrypted, (int)ark.Version);
            }
            else if ((int)ark.Version >= 7 && forgeScriptRegex.IsMatch(internalPath))
            {
                // Updates path
                internalPath = $"{internalPath}_dta_{platformExt}";

                // Creates temp dtb file
                inputFilePath = ScriptHelper.ConvertDtaToDtb(file, tempDir, ark.Encrypted, (int)ark.Version);
            }

            if (dotRegex.IsMatch(internalPath))
            {
                internalPath = dotRegex.Replace(internalPath, x => $"{x.Value.Substring(1, x.Length - 3)}/");
            }

            var fileName = Path.GetFileName(internalPath);
            var dirPath = Path
                .GetDirectoryName(internalPath)
                .Replace("\\", "/"); // Must be "/" in ark

            var pendingEntry = new PendingArkEntry(fileName, dirPath)
            {
                LocalFilePath = inputFilePath
            };

            ark.AddPendingEntry(pendingEntry);
            Log.Information("Added {EntryPath}", pendingEntry.FullPath);

            if (!entryInfo.TryGetValue(internalPath, out var hashInfo))
                continue;

            // Update hash
            using var fs = File.OpenRead(inputFilePath);
            hashInfo.Hash = Crypt.SHA1Hash(fs);
            updatedHashes.Add(hashInfo);
        }

        Log.Information("Executing file changes");
        ark.CommitChanges(inplaceEdit);

        // Clean up temp files
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);

        if (!inplaceEdit)
        {
            // Writes header
            var hdrPath = ((int)ark.Version < 9)
                ? Path.Combine(op.OutputPath, genDirName, Path.GetFileName(hdrFileName))
                : Path.Combine(op.OutputPath, Path.GetFileName(hdrFileName));
            ark.WriteHeader(hdrPath);
        }
        else
        {
            // TODO: Also look at possibly still patching exe
            return;
        }

        Log.Information("Wrote hdr/ark files");

        // Copy exe
        var exeName = Path.GetFileName(op.ExePath);
        var exePath = Path.Combine(op.OutputPath, exeName);
        File.Copy(op.ExePath, exePath, true);
        Log.Information("Copied executable \"{ExecutableName}\"", exeName);

        if (updatedHashes.Count <= 0)
            return;

        // Patch exe
        using var exeStream = File.OpenWrite(exePath);
        foreach (var hashInfo in updatedHashes)
        {
            var hashBytes = FileHelper.GetBytes(hashInfo.Hash);

            exeStream.Seek(hashInfo.Offset, SeekOrigin.Begin);
            exeStream.Write(hashBytes, 0, hashBytes.Length);

            Log.Information("Updated hash for {HashFilePath}", hashInfo.Path);
        }
    }
}
