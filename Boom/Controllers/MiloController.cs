using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Boom.Data;
using Boom.Extensions;
using Boom.Models;
using Mackiloha.Ark;
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
                var milo = MiloFile.ReadFromStream(ark.GetArkEntryFileStream(arkEntry));
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

                contextEntry.Version = (int)milo.Version;
                contextEntry.TotalSize = milo.Size;

                contextEntry.Name = milo?.DirectoryEntry?.Name ?? "";
                contextEntry.Type = milo?.DirectoryEntry?.Type ?? "";
                contextEntry.Size = (milo.DirectoryEntry == null || milo.DirectoryEntry.Data == null) ? -1 : milo.DirectoryEntry.Data.Length;
                contextEntry.Magic = (milo.DirectoryEntry == null) ? -1 : milo.DirectoryEntry.GetMagic();
                
                // Updates milo entries
                foreach (var entry in milo.Entries)
                {
                    var mEntry = entry as MiloEntry;

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
    }
}
