using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Security;
using Masuit.Tools.Win32;
using Models.Application;
using Models.Entity;
using Models.Enum;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;

namespace Models.Migrations
{
    /// <summary>
    /// 数据上下文配置
    /// </summary>
    internal sealed class Configuration : DbMigrationsConfiguration<DataContext>
    {
        public Configuration()
        {
            //开启自动迁移
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }
        /// <summary>
        /// 种子数据
        /// </summary>
        /// <param name="context"></param>
        protected override void Seed(DataContext context)
        {
#if DEBUG

            #region 初始数据

            if (!context.SystemSetting.Any())
            {
                context.Database.ExecuteSqlCommand(@"if not exists(select * from sysindexes where id=object_id('Interview') and name='IX_ViewTime') 
                                                                    CREATE NONCLUSTERED INDEX [IX_ViewTime] ON [dbo].[Interview] ([ViewTime] DESC)");
                context.Database.ExecuteSqlCommand(@"if not exists(select * from sysindexes where id=object_id('Post') and name='IX_ModifyDate') 
                                                                    CREATE NONCLUSTERED INDEX [IX_ModifyDate] ON [dbo].[Post] ([ModifyDate] DESC)");
                context.Database.ExecuteSqlCommand(@"if not exists(select * from sysindexes where id=object_id('PostHistoryVersion') and name='IX_ModifyDate') 
                                                                    CREATE NONCLUSTERED INDEX [IX_ModifyDate] ON [dbo].[PostHistoryVersion] ([ModifyDate] DESC)");
                context.Database.ExecuteSqlCommand(@"if not exists(select * from sysindexes where id=object_id('InterviewDetail') and name='IX_InterviewId') 
                                                                    CREATE NONCLUSTERED INDEX [IX_InterviewId] ON [dbo].[InterviewDetail] ([InterviewId] ASC)");
                context.Database.ExecuteSqlCommand(@"if not exists(select * from sysindexes where id=object_id('SearchDetails') and name='IX_SearchTime') 
                                                                    CREATE NONCLUSTERED INDEX [IX_SearchTime] ON [dbo].[SearchDetails] ([SearchTime] DESC)");
                try
                {
                    #region 添加约束

                    context.Database.ExecuteSqlCommand(@"ALTER TABLE [dbo].[Broadcast] ADD DEFAULT 0 FOR [IsSubscribe];
                                                                    ALTER TABLE [dbo].[Broadcast] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Broadcast] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Category] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Category] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Comment] ADD DEFAULT 0 FOR [IsPended];
                                                                    ALTER TABLE [dbo].[Comment] ADD DEFAULT 0 FOR [ParentId];
                                                                    ALTER TABLE [dbo].[Comment] ADD DEFAULT getdate() FOR [CommentDate];
                                                                    ALTER TABLE [dbo].[Comment] ADD DEFAULT 0 FOR [IsMaster];
                                                                    ALTER TABLE [dbo].[Comment] ADD DEFAULT 0 FOR [VoteCount];
                                                                    ALTER TABLE [dbo].[Comment] ADD DEFAULT 0 FOR [AgainstCount];
                                                                    ALTER TABLE [dbo].[Comment] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Comment] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Contacts] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Contacts] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Desktop] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Desktop] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Interview] ADD DEFAULT getdate() FOR [ViewTime];
                                                                    ALTER TABLE [dbo].[LeaveMessage] ADD DEFAULT getdate() FOR [PostDate];
                                                                    ALTER TABLE [dbo].[LeaveMessage] ADD DEFAULT 0 FOR [IsPended];
                                                                    ALTER TABLE [dbo].[LeaveMessage] ADD DEFAULT 0 FOR [ParentId];
                                                                    ALTER TABLE [dbo].[LeaveMessage] ADD DEFAULT 0 FOR [IsMaster];
                                                                    ALTER TABLE [dbo].[LeaveMessage] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[LeaveMessage] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Links] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Links] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Menu] ADD DEFAULT 0 FOR [ParentId];
                                                                    ALTER TABLE [dbo].[Menu] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Menu] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Misc] ADD DEFAULT getdate() FOR [PostDate];
                                                                    ALTER TABLE [dbo].[Misc] ADD DEFAULT getdate() FOR [ModifyDate];
                                                                    ALTER TABLE [dbo].[Misc] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Misc] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Notice] ADD DEFAULT getdate() FOR [PostDate];
                                                                    ALTER TABLE [dbo].[Notice] ADD DEFAULT getdate() FOR [ModifyDate];
                                                                    ALTER TABLE [dbo].[Notice] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Notice] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT 0 FOR [ViewCount];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT getdate() FOR [PostDate];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT getdate() FOR [ModifyDate];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT 0 FOR [IsFixedTop];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT 0 FOR [IsPended];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT 0 FOR [IsWordDocument];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT 0 FOR [VoteUpCount];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT 0 FOR [VoteDownCount];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT getdate() FOR [LastAccessTime];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Post] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[SearchDetails] ADD DEFAULT getdate() FOR [SearchTime];
                                                                    ALTER TABLE [dbo].[SystemSetting] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[SystemSetting] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[Top] ADD DEFAULT 1 FOR [IsDisplay];
                                                                    ALTER TABLE [dbo].[Top] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[Top] ADD DEFAULT 1 FOR [IsAvailable];
                                                                    ALTER TABLE [dbo].[UserInfo] ADD DEFAULT 0 FOR [IsAdmin];
                                                                    ALTER TABLE [dbo].[UserInfo] ADD DEFAULT getdate() FOR [LastLogin];
                                                                    ALTER TABLE [dbo].[UserInfo] ADD DEFAULT 0 FOR [IsDeleted];
                                                                    ALTER TABLE [dbo].[UserInfo] ADD DEFAULT 1 FOR [IsAvailable];
                                                                ");

                    #endregion

                    #region 创建存储过程

                    context.Database.ExecuteSqlCommand(@"Create PROC [dbo].[sp_getChildrenCommentByParentId](@ParentId int)
                                                                    AS
                                                                    BEGIN    
                                                                        WITH Tree
                                                                            AS (SELECT * FROM Comment WHERE Id = @ParentId  --第一个查询作为递归的基点(锚点)
                                                                                UNION ALL
                                                                                SELECT Comment.*     --第二个查询作为递归成员， 下属成员的结果为空时，此递归结束。
                                                                                  FROM Tree INNER JOIN Comment ON Tree.Id = Comment.ParentId) 
                                                                            SELECT * FROM Tree   
                                                                    END
                                                                    ");
                    context.Database.ExecuteSqlCommand(@"Create PROC [dbo].[sp_getChildrenLeaveMsgByParentId](@ParentId int)
                                                                    AS
                                                                    BEGIN    
                                                                        WITH Tree
                                                                            AS (SELECT * FROM LeaveMessage WHERE Id = @ParentId  --第一个查询作为递归的基点(锚点)
                                                                                UNION ALL
                                                                                SELECT LeaveMessage.*     --第二个查询作为递归成员， 下属成员的结果为空时，此递归结束。
                                                                                  FROM Tree INNER JOIN LeaveMessage ON Tree.Id = LeaveMessage.ParentId) 
                                                                            SELECT * FROM Tree   
                                                                    END
                                                                    ");
                    context.Database.ExecuteSqlCommand(@"Create PROC [dbo].[sp_getInterviewsCurrentMonthDetailsByDays] (@recent INT) AS
                                                                    BEGIN
                                                                     SELECT
                                                                      DATEPART(DAY, ViewTime) AS vt,
                                                                      COUNT (ViewTime) AS [count]
                                                                     FROM
                                                                      [dbo].[Interview]
                                                                     WHERE
                                                                      DATEDIFF(DAY, ViewTime, GETDATE()) < @recent
                                                                     AND DATEPART(MONTH, ViewTime) = DATEPART(MONTH, GETDATE())
                                                                     GROUP BY
                                                                      DATEPART(DAY, ViewTime)
                                                                     ORDER BY
                                                                      vt
                                                                     END
                                                                    ");

                    #endregion
                }
                catch (SqlException)
                {
                }
                IList<SystemSetting> ssList = new List<SystemSetting>
                {
                    new SystemSetting { Name = "Domain", Value = "masuit.com" },
                    new SystemSetting { Name = "logo", Value = "/assets/images/logo.png" },
                    new SystemSetting { Name = "Title", Value = "懒得勤快的博客" },
                    new SystemSetting { Name = "Brand", Value = "互联网分享精神，传播智慧。" },
                    new SystemSetting { Name = "EmailFrom", Value = "anduyi@163.com" },
                    new SystemSetting { Name = "EmailPwd", Value = "123456" },
                    new SystemSetting { Name = "SMTP", Value = "smtp.163.com" },
                    new SystemSetting { Name = "SmtpPort", Value = "587" },
                    new SystemSetting { Name = "ReceiveEmail", Value = "1170397736@qq.com" },
                    new SystemSetting { Name = "Slogan", Value = "懒得勤快，全栈开发者，互联网分享精神！" },
                    new SystemSetting { Name = "Keyword", Value = "懒得勤快" },
                    new SystemSetting { Name = "Description", Value = "懒得勤快" },
                    new SystemSetting { Name = "Donate", Value = "https://ww4.sinaimg.cn/large/87c01ec7gy1fsqnp79malj20q911qtbd.jpg" },
                    new SystemSetting { Name = "Copyright", Value = "懒得勤快" },
                    new SystemSetting { Name = "ReservedName", Value = "懒得勤快|system|admin|Administrator|root" },
                    new SystemSetting { Name = "Disclaimer", Value = "免责声明" },
                    new SystemSetting { Name = "DonateWechat", Value = "https://ww3.sinaimg.cn/large/87c01ec7gy1fsqnp6iaj4j20u715fjuc.jpg" },
                    new SystemSetting { Name = "DonateQQ", Value = "https://ww2.sinaimg.cn/large/87c01ec7gy1fsqnp77bktj20i30jxq42.jpg" },
                    new SystemSetting { Name = "DonateJingdong", Value = "https://ww4.sinaimg.cn/large/87c01ec7gy1fsqnp6uc00j20ng0nt75j.jpg" },
                    new SystemSetting { Name = "PathRoot", Value = "C:" },
                };
                context.SystemSetting.AddOrUpdate(s => s.Name, ssList.ToArray());
            }

            if (!context.Category.Any())
            {
                var catList = new List<Category>
                {
                    new Category { Name = "默认分类" },
                    new Category { Name = "操作系统" },
                    new Category { Name = "资源分享" },
                    new Category { Name = "影视精品" },
                    new Category { Name = "技术教程" },
                    new Category { Name = "程序开发" },
                    new Category { Name = "开发工具" },
                    new Category { Name = "科学上网" },
                    new Category { Name = "教学资源" },
                    new Category { Name = "高清壁纸" },
                    new Category { Name = "网络运维" },
                    new Category { Name = "数据库" },
                    new Category { Name = "共享文献" },
                    new Category { Name = "其他" },
                    new Category { Name = "绿色软件" },
                    new Category { Name = "专题研究" },
                    new Category { Name = "电脑硬件" },
                    new Category { Name = "玩机分享" },
                    new Category { Name = "服务器教程" },
                    new Category { Name = "随笔" },
                    new Category { Name = "福利资源" }
                };
                context.Category.AddOrUpdate(c => c.Name, catList.ToArray());
            }

            if (!context.Contacts.Any())
            {
                var contacts = new List<Contacts>
                {
                    new Contacts { Title = "腾讯QQ", Url = "http://wpa.qq.com/msgrd?v=3&uin=1170397736&site=qq&menu=yes" },
                    new Contacts { Title = "QQ空间", Url = "http://user.qzone.qq.com/1170397736/infocenter" },
                    new Contacts { Title = "百度云", Url = "http://pan.baidu.com/share/home?uk=3372842977" },
                    new Contacts { Title = "QQ邮箱", Url = "http://mail.qq.com/cgi-bin/qm_share?t=qm_mailme&email=1170397736@qq.com" },
                    new Contacts { Title = "GitHub", Url = "https://github.com/ldqk" }
                };
                context.Contacts.AddOrUpdate(c => c.Title, contacts.ToArray());
            }

            if (!context.Links.Any())
            {
                var links = new List<Links>
                {
                    new Links { Name = "懒得勤快的博客", Url = "https://masuit.com" },
                    new Links { Name = "懒得勤快的简历", Url = "http://resume.masuit.com" },
                    new Links { Name = "github", Url = "https://github.com/ldqk" },
                    new Links { Name = "QQ空间", Url = "http://user.qzone.qq.com/1170397736/infocenter" },
                    new Links { Name = "百度云", Url = "http://yun.baidu.com/share/home?uk=3372842977" }
                };
                context.Links.AddOrUpdate(l => l.Name, links.ToArray());
            }

            if (!context.Menu.Any())
            {
                var menus = new List<Menu>
                {
                    new Menu { Name = "首页", Url = "/", Sort = 10, MenuType = MenuType.MainMenu },
                    new Menu { Name = "文章", Url = "/p", Sort = 15, MenuType = MenuType.MainMenu },
                    new Menu { Name = "分类", Url = "#", Sort = 20, MenuType = MenuType.MainMenu },
                    new Menu { Name = "专题", Url = "#", Sort = 30, MenuType = MenuType.MainMenu },
                    new Menu { Name = "黑科技", Url = "#", Sort = 40, MenuType = MenuType.MainMenu },
                    new Menu { Name = "投稿", Url = "/post/publish", Sort = 50, MenuType = MenuType.MainMenu },
                    new Menu { Name = "留言板", Url = "/msg", Sort = 60, MenuType = MenuType.MainMenu },
                    new Menu { Name = "捐赠", Url = "/donate", Sort = 70, MenuType = MenuType.MainMenu },
                    new Menu { Name = "关于", Url = "/about", Sort = 70, MenuType = MenuType.MainMenu },
                };
                context.Menu.AddOrUpdate(m => m.Name, menus.ToArray());
            }

            if (!context.UserInfo.Any())
            {
                var salt = $"{new Random().StrictNext()}{DateTime.Now.GetTotalMilliseconds()}".MDString2(Guid.NewGuid().ToString()).AESEncrypt();
                context.UserInfo.AddOrUpdate(u => u.Username, new UserInfo
                {
                    Username = "masuit",
                    Password = "123abc@#$".MDString3(salt),
                    SaltKey = salt,
                    IsAdmin = true,
                    Email = "admin@masuit.com",
                    QQorWechat = "admin@masuit.com",
                    NickName = "懒得勤快"
                });
            }

            //if (!context.Interview.Any())
            //{
            //    context.Interview.Add(new Interview()
            //    {
            //        IP = "114.114.114.114",
            //        UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3205.2 Safari/537.36",
            //        OperatingSystem = "WinNT",
            //        BrowserType = "Chrome63",
            //        ViewTime = DateTime.Now,
            //        FromUrl = "http://member.webweb.com/cp/domainbind.asp",
            //        HttpMethod = "GET"
            //    });
            //}
            #endregion

            context.SaveChanges();
#endif
        }
    }
}