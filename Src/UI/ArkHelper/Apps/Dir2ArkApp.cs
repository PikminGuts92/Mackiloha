using ArkHelper.Exceptions;
using ArkHelper.Helpers;
using ArkHelper.Options;
using Mackiloha;
using Mackiloha.Ark;
using Mackiloha.CSV;
using Mackiloha.Milo2;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ArkHelper.Apps
{
    public class Dir2ArkApp
    {
        protected readonly ICacheHelper CacheHelper;
        protected readonly IScriptHelper ScriptHelper;

        public Dir2ArkApp(ICacheHelper cacheHelper, IScriptHelper scriptHelper)
        {
            CacheHelper = cacheHelper;
            ScriptHelper = scriptHelper;
        }

        public void Parse(Dir2ArkOptions op)
        {
            var dtaRegex = new Regex("(?i).dta$");
            var genPathedFile = new Regex(@"(?i)gen[\/][^\/]+$");
            var dotRegex = new Regex(@"\([.]+\)/");
            var forgeScriptRegex = new Regex("(?i).((dta)|(fusion)|(moggsong)|(script))$");
            var arkPartSizeLimit = (op.PartSizeLimit > 0)
                ? op.PartSizeLimit
                : uint.MaxValue;

            if (!Directory.Exists(op.InputPath))
            {
                throw new DirectoryNotFoundException($"Can't find directory \"{op.InputPath}\"");
            }

            var arkDir = Path.GetFullPath(op.OutputPath);

            // Set encrypted data
            if (op.ArkVersion < 3)
            {
                // Don't encrypt hdr-less arks
                op.Encrypt = false;
                op.EncryptKey = default;
            }
            else if (op.Encrypt && !op.EncryptKey.HasValue)
            {
                op.EncryptKey = 0x5A_4C_4F_4C;
            }
            else if (op.EncryptKey.HasValue)
            {
                // Don't encrypt unless explicitly stated
                op.EncryptKey = default;
            }

            // Load ark cache if path given
            bool usingCache = false;
            if (!(op.CachePath is null))
            {
                CacheHelper.LoadCache(op.CachePath, op.ArkVersion, op.Encrypt);
                usingCache = true;
            }

            // Create directory if it doesn't exist
            if (!Directory.Exists(arkDir))
                Directory.CreateDirectory(arkDir);

            // If name is all caps, match extension
            var hdrExt = op.ArkVersion >= 3
                ? ".hdr"
                : ".ark";

            if (op.ArkName.All(c => char.IsUpper(c)))
                hdrExt = hdrExt.ToUpper();

            // Create ark
            var hdrPath = Path.Combine(arkDir, $"{op.ArkName}{hdrExt}");
            var ark = ArkFile.Create(hdrPath, (ArkVersion)op.ArkVersion, (int?)op.EncryptKey);

            var files = Directory.GetFiles(op.InputPath, "*", SearchOption.AllDirectories);

            // Create temp path and guess platform
            var tempDir = CreateTemporaryDirectory();
            var platformExt = GuessPlatform(op.InputPath);

            var currentPartSize = 0u;

            foreach (var file in files)
            {
                var internalPath = FileHelper.GetRelativePath(file, op.InputPath)
                    .Replace("\\", "/"); // Must be "/" in ark

                string inputFilePath = file;

                if ((int)ark.Version < 7 && dtaRegex.IsMatch(internalPath))
                {
                    // Updates path
                    internalPath = $"{internalPath.Substring(0, internalPath.Length - 1)}b";

                    if (!genPathedFile.IsMatch(internalPath))
                        internalPath = internalPath.Insert(internalPath.LastIndexOf('/'), "/gen");

                    // Use cache if available
                    if (usingCache)
                    {
                        var cachePath = CacheHelper
                            .GetCachedPathIfNotUpdated(inputFilePath, internalPath);

                        if (cachePath is null)
                        {
                            var dtbPath = ScriptHelper
                                .ConvertDtaToDtb(file, tempDir, ark.Encrypted, (int)ark.Version);

                            CacheHelper.UpdateCachedFile(inputFilePath, internalPath, dtbPath);
                            inputFilePath = dtbPath;
                        }
                        else
                        {
                            inputFilePath = cachePath;
                        }
                    }
                    else
                    {
                        // Creates temp dtb file
                        inputFilePath = ScriptHelper.ConvertDtaToDtb(file, tempDir, ark.Encrypted, (int)ark.Version);
                    }
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

                // Check part limit
                var fileSizeLong = new FileInfo(inputFilePath).Length;
                var fileSize = (uint)fileSizeLong;
                var potentialPartSize = currentPartSize + fileSize;

                if (fileSizeLong > (long)uint.MaxValue)
                {
                    throw new NotSupportedException($"File size above 4GB is unsupported for \"{file}\"");
                }
                else if ((int)ark.Version >= 3 && potentialPartSize >= arkPartSizeLimit)
                {
                    // Kind of hacky but multiple part writing isn't implemented in commit changes yet
                    ark.CommitChanges(true);
                    ark.AddAdditionalPart();

                    currentPartSize = 0;
                }

                var fileName = Path.GetFileName(internalPath);
                var dirPath = Path.GetDirectoryName(internalPath).Replace("\\", "/"); // Must be "/" in ark

                var pendingEntry = new PendingArkEntry(fileName, dirPath)
                {
                    LocalFilePath = inputFilePath
                };

                ark.AddPendingEntry(pendingEntry);
                Log.Information("Added {EntryPath}", pendingEntry.FullPath);

                currentPartSize += fileSize;
            }

            ark.CommitChanges(true);
            if (op.ArkVersion < 3)
                Log.Information("Wrote ark to \"{ArkPath}\"", hdrPath);
            else
                Log.Information("Wrote hdr to \"{HdrPath}\"", hdrPath);

            if (usingCache)
            {
                CacheHelper.SaveCache();
            }
        }

        protected virtual string GuessPlatform(string arkPath)
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

        protected virtual string CreateTemporaryDirectory()
        {
            // Create directory in temp path
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            return tempDir;
        }

        protected virtual void DeleteDirectory(string dirPath)
        {
            // Clean up files
            if (Directory.Exists(dirPath))
                Directory.Delete(dirPath, true);
        }
    }
}
