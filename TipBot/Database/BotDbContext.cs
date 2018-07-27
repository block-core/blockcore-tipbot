using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TipBot.Database.Models;

namespace TipBot.Database
{
    public class BotDbContext : DbContext
    {
        public DbSet<DiscordUser> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // TODO local DB is for testing only.
            optionsBuilder.UseSqlite("Data Source=testOnlyDb.db");

            /*
             * In order to update DB do `Add-Migration [MigrationName]` in Package Manager Console
             */
        }
    }
}
