using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace Masuit.MyBlogs.Core.Infrastructure
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableDetailedErrors().UseLazyLoadingProxies().UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasMany(e => e.Post).WithOne(e => e.Category).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Category>().HasMany(e => e.PostHistoryVersion).WithOne(e => e.Category).HasForeignKey(r => r.CategoryId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>().HasMany(e => e.Comment).WithOne(e => e.Post).HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().HasMany(e => e.PostHistoryVersion).WithOne(e => e.Post).HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().HasMany(e => e.Seminar).WithMany(s => s.Post).UsingEntity(builder => builder.ToTable("SeminarPost"));
            modelBuilder.Entity<Post>().HasMany(e => e.PostMergeRequests).WithOne(s => s.Post).HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().HasMany(e => e.PostVisitRecords).WithOne().HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Post>().HasMany(e => e.PostVisitRecordStats).WithOne().HasForeignKey(r => r.PostId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PostHistoryVersion>().HasMany(e => e.Seminar).WithMany(s => s.PostHistoryVersion).UsingEntity(builder => builder.ToTable("SeminarPostHistoryVersion"));

            modelBuilder.Entity<UserInfo>().HasMany(e => e.LoginRecord).WithOne(e => e.UserInfo).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Menu>().HasMany(e => e.Children).WithOne(m => m.Parent).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Comment>().HasMany(e => e.Children).WithOne(c => c.Parent).HasForeignKey(c => c.ParentId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<LeaveMessage>().HasMany(e => e.Children).WithOne(c => c.Parent).HasForeignKey(c => c.ParentId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Links>().HasMany(e => e.Loopbacks).WithOne(l => l.Links).IsRequired().HasForeignKey(e => e.LinkId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Advertisement>().HasMany(e => e.ClickRecords).WithOne().HasForeignKey(e => e.AdvertisementId).IsRequired().OnDelete(DeleteBehavior.Cascade);

            modelBuilder.HasDbFunction(typeof(DataContext).GetMethod(nameof(Random))).HasName("RAND");
            modelBuilder.HasDbFunction(typeof(Guid).GetMethod(nameof(Guid.NewGuid))).HasName("RAND");
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

        //public virtual DbSet<SeminarPost> SeminarPosts { get; set; }
        //public virtual DbSet<SeminarPostHistoryVersion> SeminarPostHistoryVersions { get; set; }
        public virtual DbSet<InternalMessage> InternalMessage { get; set; }

        public virtual DbSet<FastShare> FastShare { get; set; }

        public virtual DbSet<PostMergeRequest> PostMergeRequests { get; set; }

        public virtual DbSet<Advertisement> Advertisements { get; set; }

        public virtual DbSet<Variables> Variables { get; set; }

        public virtual DbSet<LinkLoopback> LinkLoopbacks { get; set; }

        [DbFunction]
        public static double Random() => throw new NotSupportedException();
    }
}
