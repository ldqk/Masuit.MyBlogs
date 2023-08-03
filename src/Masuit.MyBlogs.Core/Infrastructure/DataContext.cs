using EntityFramework.Exceptions.Common;
using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace Masuit.MyBlogs.Core.Infrastructure;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseExceptionProcessor();
        optionsBuilder.EnableDetailedErrors().UseLazyLoadingProxies().UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll).ConfigureWarnings(builder => builder.Ignore(CoreEventId.DetachedLazyLoadingWarning, CoreEventId.LazyLoadOnDisposedContextWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Category>().HasMany(e => e.Post).WithOne(e => e.Category).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Category>().HasMany(e => e.PostHistoryVersion).WithOne(e => e.Category).HasForeignKey(r => r.CategoryId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Category>().HasMany(e => e.Children).WithOne(c => c.Parent).IsRequired(false).HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Category>().Property(c => c.Path).IsRequired();

        modelBuilder.Entity<Post>().HasMany(e => e.Comment).WithOne(e => e.Post).HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Post>().HasMany(e => e.PostHistoryVersion).WithOne(e => e.Post).HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Post>().HasMany(e => e.Seminar).WithMany(s => s.Post).UsingEntity(builder => builder.ToTable("SeminarPost"));
        modelBuilder.Entity<Post>().HasMany(e => e.PostMergeRequests).WithOne(s => s.Post).HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Post>().HasMany(e => e.PostVisitRecords).WithOne().HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Post>().HasMany(e => e.PostVisitRecordStats).WithOne().HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<PostHistoryVersion>().HasMany(e => e.Seminar).WithMany(s => s.PostHistoryVersion).UsingEntity(builder => builder.ToTable("SeminarPostHistoryVersion"));

        modelBuilder.Entity<UserInfo>().HasMany(e => e.LoginRecord).WithOne(e => e.UserInfo).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Menu>().HasMany(e => e.Children).WithOne(m => m.Parent).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Menu>().Property(c => c.Path).IsRequired();

        modelBuilder.Entity<Comment>().HasMany(e => e.Children).WithOne(c => c.Parent).HasForeignKey(c => c.ParentId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Comment>().Property(c => c.Path).IsRequired();
        modelBuilder.Entity<Comment>().Property(c => c.GroupTag).IsRequired();

        modelBuilder.Entity<LeaveMessage>().HasMany(e => e.Children).WithOne(c => c.Parent).HasForeignKey(c => c.ParentId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<LeaveMessage>().Property(c => c.Path).IsRequired();
        modelBuilder.Entity<LeaveMessage>().Property(c => c.GroupTag).IsRequired();

        modelBuilder.Entity<Links>().HasMany(e => e.Loopbacks).WithOne(l => l.Links).IsRequired().HasForeignKey(e => e.LinkId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Advertisement>().HasMany(e => e.ClickRecords).WithOne().HasForeignKey(e => e.AdvertisementId).IsRequired().OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Advertisement>().HasIndex(a => a.Price).IsDescending();
        modelBuilder.Entity<AdvertisementClickRecord>().HasIndex(a => a.Time);
        modelBuilder.Entity<AdvertisementClickRecord>().HasIndex(a => a.AdvertisementId);
        modelBuilder.Entity<Category>().HasIndex(a => a.ParentId);
        modelBuilder.Entity<Comment>().HasIndex(a => a.PostId);
        modelBuilder.Entity<LeaveMessage>().HasIndex(a => a.PostDate).IsDescending();
        modelBuilder.Entity<LinkLoopback>().HasIndex(a => a.LinkId);
        modelBuilder.Entity<Links>().HasIndex(a => a.Recommend);
        modelBuilder.Entity<LoginRecord>().HasIndex(a => a.UserInfoId);
        modelBuilder.Entity<Menu>().HasIndex(a => a.Sort);
        modelBuilder.Entity<Menu>().HasIndex(a => a.ParentId);
        modelBuilder.Entity<Notice>().HasIndex(a => a.ModifyDate).IsDescending();
        modelBuilder.Entity<Post>().HasIndex(a => a.CategoryId);
        modelBuilder.Entity<Post>().HasIndex(a => a.ModifyDate).IsDescending();
        modelBuilder.Entity<Post>().HasIndex(a => a.AverageViewCount).IsDescending();
        modelBuilder.Entity<Post>().HasIndex(a => a.TotalViewCount).IsDescending();
        modelBuilder.Entity<PostHistoryVersion>().HasIndex(a => a.CategoryId);
        modelBuilder.Entity<PostHistoryVersion>().HasIndex(a => a.PostId);
        modelBuilder.Entity<PostMergeRequest>().HasIndex(a => a.PostId);
        modelBuilder.Entity<PostVisitRecord>().HasIndex(a => a.PostId);
        modelBuilder.Entity<PostVisitRecord>().HasIndex(a => a.Time).IsDescending();
        modelBuilder.Entity<PostVisitRecordStats>().HasIndex(a => a.Date).IsDescending();
        modelBuilder.Entity<PostVisitRecordStats>().HasIndex(a => a.PostId);
        modelBuilder.Entity<SearchDetails>().HasIndex(a => a.SearchTime).IsDescending();
    }

    public override int SaveChanges()
    {
        DbUpdateConcurrencyException ex = null;
        for (int i = 0; i < 5; i++)
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException e)
            {
                ex = e;
                var entry = e.Entries.Single();
                var databaseValues = entry.GetDatabaseValues();
                var resolvedValues = databaseValues.Clone();
                entry.OriginalValues.SetValues(databaseValues);
                entry.CurrentValues.SetValues(resolvedValues);
            }
            catch (MaxLengthExceededException e)
            {
                var list = new List<Exception>() { e.InnerException };
                foreach (var entry in e.Entries)
                {
                    foreach (var property in entry.CurrentValues.Properties)
                    {
                        if (property.GetTypeMapping() is NpgsqlStringTypeMapping m)
                        {
                            var value = entry.CurrentValues.GetValue<string>(property);
                            if (m.Size < value.Length)
                            {
                                list.Add(new MaxLengthExceededException($"{entry.Metadata.Name}.{property.Name}字段值【{value}】超出长度限制：{m.Size}"));
                            }
                        }
                    }
                }
                throw new AggregateException(list);
            }
        }

        throw ex;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        DbUpdateConcurrencyException ex = null;
        for (int i = 0; i < 5; i++)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException e)
            {
                ex = e;
                var entry = e.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);
                var resolvedValues = databaseValues.Clone();
                entry.OriginalValues.SetValues(databaseValues);
                entry.CurrentValues.SetValues(resolvedValues);
            }
        }

        throw ex;
    }

    public virtual DbSet<Category> Category { get; set; }

    public virtual DbSet<Comment> Comment { get; set; }

    public virtual DbSet<LeaveMessage> LeaveMessage { get; set; }

    public virtual DbSet<Links> Links { get; set; }

    public virtual DbSet<Menu> Menu { get; set; }

    public virtual DbSet<Misc> Misc { get; set; }

    public virtual DbSet<Notice> Notice { get; set; }

    public virtual DbSet<Post> Post { get; set; }

    public virtual DbSet<PostHistoryVersion> PostHistoryVersion { get; set; }

    public virtual DbSet<SearchDetails> SearchDetails { get; set; }

    public virtual DbSet<SystemSetting> SystemSetting { get; set; }

    public virtual DbSet<UserInfo> UserInfo { get; set; }

    public virtual DbSet<LoginRecord> LoginRecord { get; set; }

    public virtual DbSet<Donate> Donate { get; set; }

    public virtual DbSet<Seminar> Seminar { get; set; }

    public virtual DbSet<InternalMessage> InternalMessage { get; set; }

    public virtual DbSet<FastShare> FastShare { get; set; }

    public virtual DbSet<PostMergeRequest> PostMergeRequests { get; set; }

    public virtual DbSet<Advertisement> Advertisements { get; set; }

    public virtual DbSet<Variables> Variables { get; set; }

    public virtual DbSet<LinkLoopback> LinkLoopbacks { get; set; }

    public virtual DbSet<PostTag> PostTags { get; set; }
}
