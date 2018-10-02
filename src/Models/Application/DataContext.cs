using EFSecondLevelCache;
using Models.Entity;
using Models.Migrations;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using static System.Data.Entity.Core.Objects.ObjectContext;

namespace Models.Application
{
    public class DataContext : DbContext
    {
        public DataContext() : base("name=DataContext")
        {
            Database.CreateIfNotExists();
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DataContext, Configuration>());
#if DEBUG
            //Database.Log = s =>
            //{
            //    LogManager.Debug(typeof(Database), s);
            //};
#endif
        }

        public virtual DbSet<Broadcast> Broadcast { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<Contacts> Contacts { get; set; }
        public virtual DbSet<Interview> Interview { get; set; }
        public virtual DbSet<InterviewDetail> InterviewDetails { get; set; }
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
        public virtual DbSet<PostAccessRecord> PostAccessRecord { get; set; }
        public virtual DbSet<Issue> Issues { get; set; }
        public virtual DbSet<InternalMessage> InternalMessage { get; set; }
        public virtual DbSet<FastShare> FastShare { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasMany(e => e.Post).WithRequired(e => e.Category).WillCascadeOnDelete(true);
            modelBuilder.Entity<Category>().HasMany(e => e.PostHistoryVersion).WithRequired(e => e.Category).WillCascadeOnDelete(false);
            modelBuilder.Entity<Contacts>().Property(e => e.Title).IsUnicode(true);
            modelBuilder.Entity<Contacts>().Property(e => e.Url).IsUnicode(true);
            modelBuilder.Entity<Interview>().Property(e => e.IP).IsUnicode(true);
            modelBuilder.Entity<Interview>().Property(e => e.UserAgent).IsUnicode(true);
            modelBuilder.Entity<Interview>().Property(e => e.OperatingSystem).IsUnicode(true);
            modelBuilder.Entity<Interview>().Property(e => e.BrowserType).IsUnicode(true);
            modelBuilder.Entity<LeaveMessage>().Property(e => e.PostDate).HasPrecision(0);
            modelBuilder.Entity<Misc>().Property(e => e.PostDate).HasPrecision(0);
            modelBuilder.Entity<Misc>().Property(e => e.ModifyDate).HasPrecision(0);
            modelBuilder.Entity<Notice>().Property(e => e.PostDate).HasPrecision(0);
            modelBuilder.Entity<Notice>().Property(e => e.ModifyDate).HasPrecision(0);
            modelBuilder.Entity<Post>().Property(e => e.PostDate).HasPrecision(0);
            modelBuilder.Entity<Post>().Property(e => e.ModifyDate).HasPrecision(0);
            modelBuilder.Entity<Post>().Property(e => e.Email).IsUnicode(true);
            modelBuilder.Entity<Post>().Property(e => e.Label).IsUnicode(true);
            modelBuilder.Entity<Post>().HasMany(e => e.Comment).WithRequired(e => e.Post).WillCascadeOnDelete(true);
            modelBuilder.Entity<Post>().HasMany(e => e.PostHistoryVersion).WithRequired(e => e.Post).WillCascadeOnDelete(true);
            modelBuilder.Entity<Post>().HasMany(e => e.PostAccessRecord).WithRequired(e => e.Post).WillCascadeOnDelete(true);
            modelBuilder.Entity<Post>().HasMany(e => e.Seminar).WithMany(s => s.Post).Map(m => m.ToTable("SeminarPost"));
            modelBuilder.Entity<PostHistoryVersion>().HasMany(e => e.Seminar).WithMany(s => s.PostHistoryVersion).Map(m => m.ToTable("SeminarPostHistoryVersion"));
            modelBuilder.Entity<SearchDetails>().Property(e => e.KeyWords).IsUnicode(true);
            modelBuilder.Entity<SearchDetails>().Property(e => e.SearchTime).HasPrecision(0);
            modelBuilder.Entity<UserInfo>().HasMany(e => e.LoginRecord).WithRequired(e => e.UserInfo).WillCascadeOnDelete(true);
        }

        //重写 SaveChanges
        public int SaveChanges(bool invalidateCacheDependencies = true)
        {
            return SaveAllChanges(invalidateCacheDependencies);
        }

        public int SaveAllChanges(bool invalidateCacheDependencies = true)
        {
            var changedEntityNames = GetChangedEntityNames();
            var result = base.SaveChanges();
            if (invalidateCacheDependencies)
            {
                new EFCacheServiceProvider().InvalidateCacheDependencies(changedEntityNames);
            }
            return result;
        }

        //修改、删除、添加数据时缓存失效
        private string[] GetChangedEntityNames()
        {
            return ChangeTracker.Entries().Where(x => x.State == EntityState.Added || x.State == EntityState.Modified || x.State == EntityState.Deleted).Select(x => GetObjectType(x.Entity.GetType()).FullName).Distinct().ToArray();
        }

        /// <summary>
        /// 根据父级ID获取下面所有的评论
        /// </summary>
        /// <param name="parentId">父级ID</param>
        /// <returns></returns>
        public virtual ObjectResult<Comment> sp_getChildrenCommentByParentId(int? parentId)
        {
            var parentIdParameter = parentId.HasValue ? new ObjectParameter("ParentId", parentId) : new ObjectParameter("ParentId", typeof(int));
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Comment>("sp_getChildrenCommentByParentId", parentIdParameter);
        }

        /// <summary>
        /// 根据父级ID获取下面所有的留言
        /// </summary>
        /// <param name="parentId">父级ID</param>
        /// <returns></returns>
        public virtual ObjectResult<LeaveMessage> sp_getChildrenLeaveMsgByParentId(int? parentId)
        {
            var parentIdParameter = parentId.HasValue ? new ObjectParameter("ParentId", parentId) : new ObjectParameter("ParentId", typeof(int));
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<LeaveMessage>("sp_getChildrenLeaveMsgByParentId", parentIdParameter);
        }

        /// <summary>
        /// 获取最近天数的访客记录
        /// </summary>
        /// <param name="recent">天数</param>
        /// <returns></returns>
        public virtual ObjectResult<Interview> sp_getInterviewsCurrentMonthDetailsByDays(int? recent)
        {
            var recentParameter = recent.HasValue ? new ObjectParameter("recent", recent) : new ObjectParameter("recent", typeof(int));
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Interview>("sp_getInterviewsCurrentMonthDetailsByDays", recentParameter);
        }
    }
}