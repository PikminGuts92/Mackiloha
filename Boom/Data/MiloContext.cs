using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Boom.Data.MiloEntities;

namespace Boom.Data
{
    public class MiloContext : DbContext
    {
        public MiloContext(DbContextOptions<MiloContext> options) : base(options)
        {

        }

        public DbSet<Ark> Arks { get; set; }
        public DbSet<ArkEntry> ArkEntries { get; set; }
        public DbSet<Milo> Milos { get; set; }
        public DbSet<MiloEntry> MiloEntries { get; set; }
    }
}
