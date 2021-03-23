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
                var cache = JsonSerializer.Deserialize<ArkCache>(cacheJson);

                if (cache.Version != arkVersion
                    || cache.Encrypted != arkEncrypted)
                {
                    // Ignore cached files
                    cache.Files.Clear();
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

        public virtual string GetCachedPathIfNotUpdated(string realPath, string internalPath)
        {
            var realFileInfo = new FileInfo(realPath);

            if (MappedCachedFiles
                .TryGetValue(internalPath, out var info)
                && info.RealPath == realPath
                && info.LastUpdated >= realFileInfo.LastWriteTime)
            {
                var filePath = Path.Combine(CacheDirectory, "files", internalPath);

                return (File.Exists(filePath))
                    ? filePath
                    : null;
            }

            return null;
        }

        public virtual void UpdateCachedFile(string realPath, string internalPath)
        {
            var realFileInfo = new FileInfo(realPath);

            var filePath = Path.Combine(CacheDirectory, "files", internalPath);
            File.Copy(realPath, filePath, true);

            if (MappedCachedFiles.TryGetValue(internalPath, out var info))
            {
                // Update existing
                info.RealPath = realPath;
                info.LastUpdated = realFileInfo.LastWriteTime;
            }
            else
            {
                // Add new entry
                var newInfo = new CachedFileInfo()
                {
                    RealPath = realPath,
                    InternalPath = internalPath,
                    LastUpdated = realFileInfo.LastWriteTime
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
