using EFSecondLevelCache.Core;
using EFSecondLevelCache.Core.Contracts;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Masuit.MyBlogs.Core.Infrastructure
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies().UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasMany(e => e.Post).WithOne(e => e.Category).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Category>().HasMany(e => e.PostHistoryVersion).WithOne(e => e.Category).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().Property(e => e.Email).IsUnicode();
            modelBuilder.Entity<Post>().Property(e => e.Label).IsUnicode();
            modelBuilder.Entity<Post>().HasMany(e => e.Comment).WithOne(e => e.Post).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().HasMany(e => e.PostHistoryVersion).WithOne(e => e.Post).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().HasMany(e => e.PostAccessRecord).WithOne(e => e.Post).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().HasMany(e => e.Seminar).WithOne(s => s.Post).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PostHistoryVersion>().HasMany(e => e.Seminar).WithOne(s => s.PostHistoryVersion);
            modelBuilder.Entity<SearchDetails>().Property(e => e.KeyWords).IsUnicode();
            modelBuilder.Entity<UserInfo>().HasMany(e => e.LoginRecord).WithOne(e => e.UserInfo).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SeminarPost>().HasKey(s => new { s.SeminarId, s.PostId });
            modelBuilder.Entity<SeminarPost>().Property(s => s.SeminarId).HasColumnName("Seminar_Id");
            modelBuilder.Entity<SeminarPost>().Property(s => s.PostId).HasColumnName("Post_Id");
            modelBuilder.Entity<SeminarPostHistoryVersion>().HasKey(s => new { s.SeminarId, s.PostHistoryVersionId });
            modelBuilder.Entity<SeminarPostHistoryVersion>().Property(s => s.SeminarId).HasColumnName("Seminar_Id");
            modelBuilder.Entity<SeminarPostHistoryVersion>().Property(s => s.PostHistoryVersionId).HasColumnName("PostHistoryVersion_Id");
        }

        public override int SaveChanges()
        {
            ChangeTracker.DetectChanges();
            var changedEntityNames = this.GetChangedEntityNames();
            var result = base.SaveChanges();
            this.GetService<IEFCacheServiceProvider>().InvalidateCacheDependencies(changedEntityNames);
            return result;
        }
        public virtual DbSet<Broadcast> Broadcast { get; set; }
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
        public virtual DbSet<SeminarPost> SeminarPosts { get; set; }
        public virtual DbSet<SeminarPostHistoryVersion> SeminarPostHistoryVersions { get; set; }
        public virtual DbSet<PostAccessRecord> PostAccessRecord { get; set; }
        public virtual DbSet<InternalMessage> InternalMessage { get; set; }
        public virtual DbSet<FastShare> FastShare { get; set; }

    }
    /// <summary>
    /// 勿删，数据库迁移时powershell会执行该方法
    /// </summary>
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext>
    {
        public DataContext CreateDbContext(string[] args)
        {
            //IConfigurationRoot config = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();
            var conn = "Server=192.168.135.70;Database=matrixone_portfolio_test_db;Uid=portfolio;Pwd=portfolio@#$123;";
            var builder = new DbContextOptionsBuilder<DataContext>();
            //builder.UseMySql(conn);
            builder.UseSqlServer("Data Source=.;Initial Catalog=CoreTest;Integrated Security=True");

            return new DataContext(builder.Options);
        }
    }
}
