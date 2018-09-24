using Microsoft.EntityFrameworkCore;
using System.IO;

namespace TSBot.DB
{
    public class DatabaseContext : DbContext
    {
        public readonly string Path;

        public DbSet<TelegramUser> TelegramUser { get; set; }

        public DbSet<TSUser> TSUser { get; set; }

        public DatabaseContext(string path)
        {
            this.Path = new FileInfo(path).FullName;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source='{this.Path}'");
        }
    }
}
