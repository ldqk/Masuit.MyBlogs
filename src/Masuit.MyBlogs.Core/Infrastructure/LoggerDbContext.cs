using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Masuit.MyBlogs.Core.Common;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Masuit.MyBlogs.Core.Infrastructure;

public class LoggerDbContext : DbContext
{
    public LoggerDbContext(DbContextOptions<LoggerDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableDetailedErrors().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RequestLogDetail>().HasKey(e => new { e.Id, e.Time });
        modelBuilder.Entity<PerformanceCounter>().HasKey(e => e.Time);
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class HypertableColumnAttribute : Attribute
{ }

public static class TimeScaleExtensions
{
    public static void ApplyHypertables(this DbContext context)
    {
        if (context.Database.IsNpgsql())
        {
            context.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;");
            var entityTypes = context.Model.GetEntityTypes();
            foreach (var entityType in entityTypes)
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.PropertyInfo.GetCustomAttribute(typeof(HypertableColumnAttribute)) != null)
                    {
                        var tableName = entityType.GetTableName();
                        var schema = entityType.GetSchema();
                        var identifier = StoreObjectIdentifier.Table(tableName, schema);
                        var columnName = property.GetColumnName(identifier);
                        if (property.ClrType == typeof(DateTime))
                        {
                            context.Database.ExecuteSqlRaw($"SELECT create_hypertable('\"{tableName}\"', '{columnName}');");
                        }
                        else
                        {
                            context.Database.ExecuteSqlRaw($"SELECT create_hypertable('\"{tableName}\"', '{columnName}', chunk_time_interval => 100000);");
                        }
                    }
                }
            }
        }
    }
}
