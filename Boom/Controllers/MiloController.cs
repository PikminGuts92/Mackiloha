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
            
            var ark = ArkFile.FromFile(request.FilePath);
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

                contextEntry.Name = (string)milo.Name ?? "";
                contextEntry.Type = (string)milo.Type ?? "";

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

                    contextMEntry.Name = (string)mEntry.Name ?? "";
                    contextMEntry.Type = (string)mEntry.Type ?? "";
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
    }
}
