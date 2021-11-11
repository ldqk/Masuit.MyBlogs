using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Models.Drive;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Masuit.MyBlogs.Core.Infrastructure
{
    public class DriveContext : DbContext
    {
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Site> Sites { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite(OneDriveConfiguration.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var converter = new ValueConverter<string[], string>(model => string.Join(',', model), data => data.Split(',', StringSplitOptions.None));
            modelBuilder.Entity<Site>().Property("HiddenFolders").HasConversion(converter);
        }
    }
}