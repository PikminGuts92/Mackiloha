using ArkHelper.Json;
using ArkHelper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ArkHelper.Helpers
{
    public class CacheHelper : ICacheHelper
    {
        protected string CacheDirectory;
        protected int ArkVersion;
        protected bool ArkEncrypted;

        protected Dictionary<string, CachedFileInfo> MappedCachedFiles;

        public virtual void LoadCache(string path, int arkVersion, bool arkEncrypted)
        {
            (CacheDirectory, ArkVersion, ArkEncrypted) = (path, arkVersion, arkEncrypted);
            MappedCachedFiles = new Dictionary<string, CachedFileInfo>();

            CreateDirectory(path);
            var cachePath = GetCacheFilePath();

            if (File.Exists(cachePath))
            {
                var cacheJson = File.ReadAllText(cachePath);
                var cache = JsonSerializer.Deserialize<ArkCache>(cacheJson, ArkHelperJsonContext.Default.ArkCache);

                if (cache.Version != arkVersion
                    || cache.Encrypted != arkEncrypted)
                {
                    // Ignore cached files
                    cache.Files.Clear();

                    var cachedFiles = Path.Combine(CacheDirectory, "files");
                    if (Directory.Exists(cachedFiles))
                        Directory.Delete(cachedFiles, true);
                }
                else
                {
                    // Update mapped files
                    MappedCachedFiles = cache
                        .Files
                        .ToDictionary(
                            x => x.InternalPath,
                            y => y);
                }
            }
        }

        public virtual void SaveCache()
        {
            var cachePath = GetCacheFilePath();

            var cache = new ArkCache()
            {
                Version = ArkVersion,
                Encrypted = ArkEncrypted,
                Files = MappedCachedFiles
                    .Select(x => x.Value)
                    .OrderBy(x => x.InternalPath)
                    .ToList()
            };

            var cacheJson = JsonSerializer.Serialize<ArkCache>(cache, ArkHelperJsonContext.Default.ArkCache);
            File.WriteAllText(cachePath, cacheJson);
        }

        public virtual string GetCachedPathIfNotUpdated(string sourcePath, string internalPath)
        {
            var fullSourcePath = Path.GetFullPath(sourcePath);
            var lastWriteTime = File.GetLastWriteTime(sourcePath);

            if (MappedCachedFiles
                .TryGetValue(internalPath, out var info)
                && info.SourcePath == fullSourcePath
                && info.LastUpdated >= lastWriteTime)
            {
                var filePath = Path.Combine(CacheDirectory, "files", internalPath);

                return (File.Exists(filePath))
                    ? filePath
                    : null;
            }

            return null;
        }

        public virtual void UpdateCachedFile(string sourcePath, string internalPath, string genFilePath)
        {
            var fullSourcePath = Path.GetFullPath(sourcePath);
            var lastWriteTime = File.GetLastWriteTime(sourcePath);

            var internalFilePath = Path.GetFullPath(Path.Combine(CacheDirectory, "files", internalPath));

            // Create directory if one doesn't exist
            var internalFileDirectory = Path.GetDirectoryName(internalFilePath);
            CreateDirectory(internalFileDirectory);

            // Copy gen file to internal
            File.Copy(genFilePath, internalFilePath, true);

            if (MappedCachedFiles.TryGetValue(internalPath, out var info))
            {
                // Update existing
                info.SourcePath = fullSourcePath;
                info.LastUpdated = lastWriteTime;
            }
            else
            {
                // Add new entry
                var newInfo = new CachedFileInfo()
                {
                    SourcePath = fullSourcePath,
                    InternalPath = internalPath,
                    LastUpdated = lastWriteTime
                };

                MappedCachedFiles.Add(newInfo.InternalPath, newInfo);
            }
        }

        protected virtual void CreateDirectory(string path)
        {
            if (Directory.Exists(path))
                return;

            // Create directory
            Directory.CreateDirectory(path);
        }

        protected string GetCacheFilePath()
            => Path.Combine(CacheDirectory, "cache.json");
    }
}
