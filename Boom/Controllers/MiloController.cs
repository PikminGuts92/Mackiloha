using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boom.Data;
using Boom.Extensions;
using Boom.Models;
using Mackiloha;
using Mackiloha.Ark;
using Mackiloha.IO;
using Mackiloha.Milo2;
using Mackiloha.Render;

namespace Boom.Controllers
{
    [Route("[controller]")]
    public class MiloController : ControllerBase
    {
        private readonly MiloContext _miloContext;
        private readonly Regex _miloRegex = new Regex("[.]((rnd([_][a-zA-Z0-9]+)?)|(gh)|(milo([_][a-zA-Z0-9]+)?))$"); // Known milo extensions

        public MiloController(MiloContext miloContext)
        {
            _miloContext = miloContext;
        }

        [HttpGet]
        [Route("Archive")]
        public IActionResult GetArchives()
        {
            var games = _miloContext.Arks.Include(x => x.Entries).ToList();

            var milos = _miloContext.Milos.Include(x => x.Entries).Join(_miloContext.ArkEntries, milo => milo.ArkEntryId, arkEntry => arkEntry.Id, (milo, arkEntry) => new
            {
                ArkId = arkEntry.ArkId,
                FilePath = arkEntry.Path,

                Version = milo.Version,
                TotalSize = milo.TotalSize,
                
                Directory = new
                {
                    Name = milo.Name,
                    Type = milo.Type,
                    Size = milo.Size,
                    Magic = milo.Magic
                },
                
                Entries = milo.Entries.OrderBy(y => y.Type).ThenBy(z => z.Name).ToList()
            });

            var newList = games.Select(x => new
            {
                Title = x.Title,
                Platform = x.Platform,
                Region = x.Region,
                Version = x.ArkVersion,

                DirectoryTypes = new List<NameCollection<string, int>>(),
                EntryTypes = new List<NameCollection<string, int>>(),

                Milos = milos.Where(y => y.ArkId == x.Id).OrderBy(z => z.FilePath).ToList()
            }).ToList();

            foreach (var item in newList)
            {
                var dirTypes = item.Milos
                    .Select(x => x.Directory.Type)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new NameCollection<string, int>()
                    {
                        Name = x,
                        Values = item.Milos
                            .Where(y => y.Directory.Type == x)
                            .Select(z => z.Directory.Magic)
                            .Distinct()
                            .OrderBy(w => w)
                            .ToList()
                    });
                
                var entryTypes = item.Milos
                    .SelectMany(x => x.Entries)
                    .Select(y => y.Type)
                    .Distinct()
                    .OrderBy(x => x)
                    .Select(x => new NameCollection<string, int>()
                    {
                        Name = x,
                        Values = item.Milos
                            .SelectMany(y => y.Entries)
                            .Where(z => z.Type == x)
                            .Select(w => w.Magic)
                            .Distinct()
                            .OrderBy(q => q)
                            .ToList()
                    });
                
                item.DirectoryTypes.AddRange(dirTypes);
                item.EntryTypes.AddRange(entryTypes);
            }

            return Ok(newList);
        }

        [HttpGet]
        [Route("Archive/{id}")]
        public IActionResult GetArchive(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            var game = _miloContext.Arks.Include(x => x.Entries).Single(x => x.Id == id);

            var milos = _miloContext.Milos.Include(x => x.Entries).Join(_miloContext.ArkEntries.Where(x => x.ArkId == game.Id), milo => milo.ArkEntryId, arkEntry => arkEntry.Id, (milo, arkEntry) => new
            {
                FilePath = arkEntry.Path,

                Version = milo.Version,
                TotalSize = milo.TotalSize,

                Directory = new
                {
                    Name = milo.Name,
                    Type = milo.Type,
                    Size = milo.Size,
                    Magic = milo.Magic
                },

                Entries = milo.Entries.OrderBy(y => y.Type).ThenBy(z => z.Name).ToList()
            });

            var item = new
            {
                Title = game.Title,
                Platform = game.Platform,
                Region = game.Region,
                Version = game.ArkVersion,

                DirectoryTypes = new List<NameCollection<string, int>>(),
                EntryTypes = new List<NameCollection<string, int>>(),

                Milos = milos.OrderBy(z => z.FilePath).ToList()
            };

            item.DirectoryTypes.AddRange(item.Milos
                .Select(x => x.Directory.Type)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new NameCollection<string, int>()
                {
                    Name = x,
                    Values = item.Milos
                        .Where(y => y.Directory.Type == x)
                        .Select(z => z.Directory.Magic)
                        .Distinct()
                        .OrderBy(w => w)
                        .ToList()
                }));

            item.EntryTypes.AddRange(item.Milos
                .SelectMany(x => x.Entries)
                .Select(y => y.Type)
                .Distinct()
                .OrderBy(x => x)
                .Select(x => new NameCollection<string, int>()
                {
                    Name = x,
                    Values = item.Milos
                        .SelectMany(y => y.Entries)
                        .Where(z => z.Type == x)
                        .Select(w => w.Magic)
                        .Distinct()
                        .OrderBy(q => q)
                        .ToList()
                }));
            
            return Ok(item);
        }

        [HttpGet]
        [Route("Archive/{id}/Milos")]
        public IActionResult GetArchiveEntries(int? id, bool groupByType)
        {
            if (!id.HasValue)
                return BadRequest();

            var game = _miloContext.Arks.Include(x => x.Entries).Single(x => x.Id == id);

            var milos = _miloContext.Milos.Include(x => x.Entries).Join(_miloContext.ArkEntries.Where(x => x.ArkId == game.Id), milo => milo.ArkEntryId, arkEntry => arkEntry.Id, (milo, arkEntry) => new
            {
                FilePath = arkEntry.Path,

                Version = milo.Version,
                TotalSize = milo.TotalSize,

                Directory = new
                {
                    Name = milo.Name,
                    Type = milo.Type,
                    Size = milo.Size,
                    Magic = milo.Magic
                },

                Entries = milo.Entries.OrderBy(y => y.Type).ThenBy(z => z.Name).ToList()
            }).OrderBy(x => x.FilePath);

            if (!groupByType)
                return Ok(milos);

            var grouped = milos
                .GroupBy(x => x.Directory.Type)
                .OrderBy(y => y.Key)
                .Select(z => new
                {
                    Group = z.Key,
                    Milos = z.ToList()
                });
            
            return Ok(grouped);
        }

        [HttpPost]
        [Route("ScanArk")]
        public IActionResult ScanArkPost([FromBody] ScanRequest request)
        {
            _miloContext.Database.EnsureCreated();
            var sw = Stopwatch.StartNew();

            // Updates games (arks)
            var game = _miloContext.Arks.FirstOrDefault(x => x.Title == request.GameTitle
                && x.Platform == request.Platform
                && x.Region == request.Region);

            if (game == null)
            {
                // Create game
                game = new Data.MiloEntities.Ark()
                {
                    Title = request.GameTitle,
                    Platform = request.Platform,
                    Region = request.Region
                };

                _miloContext.Arks.Add(game);
                _miloContext.SaveChanges();
            }
            
            var ark = ArkFile.FromFile(request.InputPath);
            game.ArkVersion = (int)ark.Version;
            var miloEntries = new List<Data.MiloEntities.ArkEntry>();
            var totalMiloEntries = 0;
            
            // Updates ark entries
            foreach (var arkEntry in ark.Entries)
            {
                var entry = arkEntry as OffsetArkEntry;

                var contextEntry = _miloContext.ArkEntries.FirstOrDefault(x => x.Ark == game && x.Path == entry.FullPath);
                if (contextEntry == null)
                {
                    contextEntry = new Data.MiloEntities.ArkEntry()
                    {
                        Ark = game,
                        Path = entry.FullPath
                    };

                    _miloContext.ArkEntries.Add(contextEntry);
                    _miloContext.SaveChanges();
                }
                
                contextEntry.Part = entry.Part;
                contextEntry.Offset = entry.Offset;
                contextEntry.Size = (int)entry.Size;
                contextEntry.InflatedSize = (int)entry.InflatedSize;

                if (_miloRegex.IsMatch(contextEntry.Path))
                    miloEntries.Add(contextEntry);

                _miloContext.Update(contextEntry);
            }

            // Updates milos
            foreach (var miloEntry in miloEntries)
            {
                var arkEntry = ark.Entries.First(x => x.FullPath == miloEntry.Path);
                var mf = MiloFile.ReadFromStream(ark.GetArkEntryFileStream(arkEntry));
                var serializer = new MiloSerializer(new SystemInfo() { BigEndian = mf.BigEndian, Version = mf.Version });
                MiloObjectDir milo;


                using (var ms = new MemoryStream(mf.Data))
                {
                    milo = serializer.ReadFromStream<MiloObjectDir>(ms);
                }

                totalMiloEntries += milo.Entries.Count;

                var contextEntry = _miloContext.Milos.FirstOrDefault(x => x.ArkEntry == miloEntry);
                if (contextEntry == null)
                {
                    contextEntry = new Data.MiloEntities.Milo()
                    {
                        ArkEntry = miloEntry
                    };

                    _miloContext.Milos.Add(contextEntry);
                    _miloContext.SaveChanges();
                }

                contextEntry.Version = mf.Version;
                contextEntry.TotalSize = mf.Data.Length;

                contextEntry.Name = milo.Name ?? "";
                contextEntry.Type = milo.Type ?? "";

                var dirEntry = milo.Entries
                    .Where(x => ((string)x.Type).EndsWith("Dir") && x is MiloObjectBytes)
                    .Select(x => x as MiloObjectBytes)
                    .FirstOrDefault();

                if (dirEntry != null)
                {
                    contextEntry.Size = dirEntry.Data.Length;
                    contextEntry.Magic = dirEntry.GetMagic();
                }
                else
                {
                    contextEntry.Size = -1;
                    contextEntry.Magic = -1;
                }
                
                // Updates milo entries
                foreach (var mEntry in milo.Entries.Where(x => x is MiloObjectBytes && x != dirEntry).Select(y => y as MiloObjectBytes))
                {
                    var contextMEntry = _miloContext.MiloEntries.FirstOrDefault(x => x.Milo == contextEntry && x.Name == mEntry.Name && x.Type == mEntry.Type);
                    if (contextMEntry == null)
                    {
                        contextMEntry = new Data.MiloEntities.MiloEntry()
                        {
                            Milo = contextEntry
                        };

                        _miloContext.MiloEntries.Add(contextMEntry);
                        _miloContext.SaveChanges();
                    }

                    contextMEntry.Name = mEntry.Name ?? "";
                    contextMEntry.Type = mEntry.Type ?? "";
                    contextMEntry.Size = mEntry.Data.Length;
                    contextMEntry.Magic = mEntry.GetMagic();

                    _miloContext.Update(contextMEntry);
                }

                _miloContext.Update(contextEntry);
            }

            _miloContext.SaveChanges();
            sw.Stop();

            return Ok(new ScanResult()
            {
                TotalArkEntries = ark.Entries.Count,
                TotalMilos = miloEntries.Count,
                TotalMiloEntries = totalMiloEntries,
                TimeElapsed = sw.ElapsedMilliseconds
            });
        }

        [HttpPost]
        [Route("TestSerialization")]
        public IActionResult TestSerialization([FromBody] ScanRequest request, bool testSerialize)
        {
            if (!Directory.Exists(request.InputPath))
                return BadRequest($"Directory \"{request.InputPath}\" does not exist!");

            var miloEntryRegex = new Regex(@"([^\\]*([.](rnd)|(gh)))\\([^\\]+)\\([^\\]+)$", RegexOptions.IgnoreCase);
            var miloEntries = Directory.GetFiles(request.InputPath, "*", SearchOption.AllDirectories)
                .Where(x => miloEntryRegex.IsMatch(x))
                .Select(y =>
                {
                    var match = miloEntryRegex.Match(y);

                    var miloArchive = match.Groups[1].Value;
                    var miloEntryType = match.Groups[5].Value;
                    var miloEntryName = match.Groups[6].Value;

                    return new
                    {
                        FullPath = y,
                        MiloArchive = miloArchive,
                        MiloEntryType = miloEntryType,
                        MiloEntryName = miloEntryName
                    };
                })
                .ToArray();

            //var groupedEntries = miloEntries.GroupBy(x => x.MiloEntryType).ToDictionary(g => g.Key, g => g.ToList());
            MiloSerializer serializer = new MiloSerializer(new SystemInfo() { Version = 10, Platform = Platform.PS2, BigEndian = false });
            var supportedTypes = new [] { "Cam", "Environ", "Mat", "Mesh", "Tex", "View" };

            var results = miloEntries
                .Where(w => supportedTypes.Contains(w.MiloEntryType))
                .OrderBy(x => x.MiloEntryType)
                .ThenBy(y => y.FullPath)
                .Select(z =>
                {
                    ISerializable data = null;
                    string message = "";
                    bool converted = false;
                    bool perfectSerialize = true;

                    try
                    {
                        switch (z.MiloEntryType)
                        {
                            case "Cam":
                                data = serializer.ReadFromFile<Cam>(z.FullPath);
                                break;
                            case "Environ":
                                data = serializer.ReadFromFile<Environ>(z.FullPath);
                                break;
                            case "Mat":
                                data = serializer.ReadFromFile<Mat>(z.FullPath);
                                break;
                            case "Mesh":
                                data = serializer.ReadFromFile<Mesh>(z.FullPath);
                                break;
                            case "Tex":
                                data = serializer.ReadFromFile<Tex>(z.FullPath);
                                break;
                            case "View":
                                data = serializer.ReadFromFile<View>(z.FullPath);
                                break;
                            default:
                                throw new Exception("Not Supported");
                        }

                        (data as IMiloObject).Name = z.MiloEntryName;
                        converted = true;
                    }
                    catch (Exception ex)
                    {
                        var trace = new StackTrace(ex, true);
                        var frame = trace.GetFrame(0);
                        var name = frame.GetMethod().ReflectedType.Name;

                        message = $"{name}: {ex.Message}";
                    }

                    if (testSerialize)
                    {
                        try
                        {
                            byte[] bytes;

                            using (var ms = new MemoryStream())
                            {
                                serializer.WriteToStream(ms, data);
                                bytes = ms.ToArray();
                            }

                            var origBytes = System.IO.File.ReadAllBytes(z.FullPath);

                            if (bytes.Length != origBytes.Length)
                                throw new Exception("Byte count doesn't match");

                            for (int i = 0; i < bytes.Length; i++)
                            {
                                if (bytes[i] != origBytes[i])
                                    throw new Exception("Bytes don't match");
                            }
                        }
                        catch (Exception ex)
                        {
                            perfectSerialize = false;
                        }
                    }

                    return new { Entry = z, Data = data, Message = message, Converted = converted, Serialized = perfectSerialize };
                }).ToList();
            
            /*
            var textures = groupedEntries["Tex"]
                .Select(x => serializer.ReadFromFile<Tex>(x.FullPath))
                .ToList();

            var views = groupedEntries["View"]
                .Select(x => serializer.ReadFromFile<View>(x.FullPath))
                .ToList();

            var meshes = groupedEntries["Mesh"]
                .Select(x => serializer.ReadFromFile<Mesh>(x.FullPath))
                .ToList();
            */        

            return Ok(new
            {
                TotalCoverage = results.Count(x => x.Converted) / (double)results.Count,
                TotalScanned = results.Count,
                ByType = results
                    .GroupBy(x => x.Entry.MiloEntryType)
                    .ToDictionary(g => g.Key, g => new
                        {
                            Coverage = g.Count(x => x.Converted) / (double)g.Count(),
                            Scanned = g.Count(),
                            //Converted = g
                            //    .Where(x => x.Converted)
                            //    .Select(x => new
                            //    {
                            //        x.Entry.FullPath
                            //    }),
                            NotConverted = g
                                .Where(x => !x.Converted)
                                .Select(x => new
                                {
                                    x.Entry.FullPath,
                                    x.Message
                                })  
                        }),
                NotSerialized = results
                    .Where(x => !x.Serialized)
                    .Select(y => y.Entry.FullPath)
                //Converted = results
                //    .Where(x => x.Converted)
                //    .Select(x => new
                //    {
                //        x.Entry.FullPath
                //    }),
                //NotConverted = results
                //    .Where(x => !x.Converted)
                //    .Select(x => new
                //    {
                //        x.Entry.FullPath,
                //        x.Message
                //    })
            });
        }

        [HttpPost]
        [Route("ExtractArk")]
        public IActionResult ExtractFilesFromArk([FromBody] ScanRequest request, bool extractMilos, bool extractDTAs)
        {
            if (!System.IO.File.Exists(request.InputPath))
                return BadRequest($"File \"{request.InputPath}\" does not exist!");

            if (request.OutputPath == null)
                return BadRequest($"Output directory cannot be null!");

            var ark = ArkFile.FromFile(request.InputPath);
            
            string CombinePath(string basePath, string path)
            {
                // Consistent slash
                basePath = (request.OutputPath ?? "").Replace("/", "\\");
                path = (path ?? "").Replace("/", "\\");
                
                Regex dotRegex = new Regex(@"[.]+[\\]");
                if (dotRegex.IsMatch(path))
                {
                    // Replaces dotdot path
                    path = dotRegex.Replace(path, x => $"({x.Value.Substring(0, x.Value.Length - 1)})\\");
                }
                
                return Path.Combine(basePath, path);
            }

            string GetNonGenPath(string path)
            {
                // Consistent slash
                path = (path ?? "").Replace("/", "\\");

                Regex genRegex = new Regex(@"gen\\[^\\]+$", RegexOptions.IgnoreCase);
                Regex platformRegex = new Regex(@"_[^_]+$", RegexOptions.IgnoreCase); // TODO: Revisit for Forge extensions

                if (genRegex.IsMatch(path))
                {
                    var splitPath = path.Split('\\');
                    var dir = string.Join("\\", splitPath.SkipLast(2));
                    var file = platformRegex.Replace(splitPath.Last(), "");

                    path = $"{dir}\\{file}";
                }

                return path;
            }

            void SaveAsFile(MiloObjectBytes miloEntry, string basePath)
            {
                var filePath = basePath = Path.Combine(basePath, miloEntry.Type, miloEntry.Name);
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                System.IO.File.WriteAllBytes(filePath, miloEntry.Data);
                Console.WriteLine($"Wrote \"{filePath}\"");
            }

            // Extract everything
            if (!extractDTAs && !extractMilos)
            {
                foreach (var arkEntry in ark.Entries)
                {
                    var filePath = CombinePath(request.OutputPath, arkEntry.FullPath);

                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var fs = System.IO.File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (var stream = ark.GetArkEntryFileStream(arkEntry))
                        {
                            stream.CopyTo(fs);
                        }
                    }

                    Console.WriteLine($"Wrote \"{filePath}\"");
                }

                return Ok();
            }

            // Extract milos
            foreach (var miloArkEntry in ark.Entries.Where(x => _miloRegex.IsMatch(x.FullPath)))
            {
                var filePath = CombinePath(request.OutputPath, GetNonGenPath(miloArkEntry.FullPath));

                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (var stream = ark.GetArkEntryFileStream(miloArkEntry))
                {
                    var milo = MiloFile.ReadFromStream(stream);
                    var miloSerializer = new MiloSerializer(new SystemInfo()
                    {
                        Version = milo.Version,
                        BigEndian = false,
                        Platform = Enum.Parse<Platform>(request.Platform)
                    });

                    var miloDir = new MiloObjectDir();
                    using (var ms = new MemoryStream(milo.Data))
                    {
                        miloSerializer.ReadFromStream(ms, miloDir);
                    }

                    miloDir.Entries.ForEach(x => SaveAsFile(x as MiloObjectBytes, filePath));

                    if (miloDir.Extras.ContainsKey("DirectoryEntry"))
                    {
                        SaveAsFile(miloDir.Extras["DirectoryEntry"] as MiloObjectBytes, filePath);
                    }
                }
            }

            return Ok();
        }
    }
}
