using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Masuit.MyBlogs.Core.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(name: "Broadcast", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Email = table.Column<string>(nullable: true),
                ValidateCode = table.Column<string>(nullable: true),
                UpdateTime = table.Column<DateTime>(nullable: false),
                SubscribeType = table.Column<int>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Broadcast", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "Category", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Name = table.Column<string>(maxLength: 64, nullable: false),
                Description = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Category", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "Donate", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                NickName = table.Column<string>(nullable: true),
                Email = table.Column<string>(nullable: true),
                QQorWechat = table.Column<string>(nullable: true),
                EmailDisplay = table.Column<string>(nullable: true),
                QQorWechatDisplay = table.Column<string>(nullable: true),
                Amount = table.Column<string>(nullable: true),
                Via = table.Column<string>(nullable: true),
                DonateTime = table.Column<DateTime>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Donate", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "FastShare", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Title = table.Column<string>(nullable: true),
                Link = table.Column<string>(nullable: true),
                Sort = table.Column<int>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_FastShare", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "InternalMessage", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Title = table.Column<string>(nullable: true),
                Content = table.Column<string>(nullable: true),
                Link = table.Column<string>(nullable: true),
                Time = table.Column<DateTime>(nullable: false),
                Read = table.Column<bool>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_InternalMessage", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "LeaveMessage", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                NickName = table.Column<string>(nullable: false),
                Content = table.Column<string>(nullable: false),
                PostDate = table.Column<DateTime>(nullable: false),
                Email = table.Column<string>(nullable: true),
                QQorWechat = table.Column<string>(nullable: true),
                ParentId = table.Column<int>(nullable: false),
                Browser = table.Column<string>(maxLength: 255, nullable: true),
                OperatingSystem = table.Column<string>(maxLength: 255, nullable: true),
                IsMaster = table.Column<bool>(nullable: false),
                IP = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_LeaveMessage", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "Links", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Name = table.Column<string>(nullable: false),
                Url = table.Column<string>(nullable: false),
                Except = table.Column<bool>(nullable: false),
                Recommend = table.Column<bool>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Links", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "Menu", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Name = table.Column<string>(nullable: false),
                Icon = table.Column<string>(nullable: true),
                Url = table.Column<string>(nullable: false),
                Sort = table.Column<int>(nullable: false),
                ParentId = table.Column<int>(nullable: false),
                MenuType = table.Column<int>(nullable: false),
                NewTab = table.Column<bool>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Menu", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "Misc", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Title = table.Column<string>(nullable: false),
                Content = table.Column<string>(nullable: false),
                PostDate = table.Column<DateTime>(nullable: false),
                ModifyDate = table.Column<DateTime>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Misc", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "Notice", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Title = table.Column<string>(nullable: false),
                Content = table.Column<string>(nullable: false),
                PostDate = table.Column<DateTime>(nullable: false),
                ModifyDate = table.Column<DateTime>(nullable: false),
                ViewCount = table.Column<int>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Notice", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "SearchDetails", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                KeyWords = table.Column<string>(nullable: false),
                SearchTime = table.Column<DateTime>(nullable: false),
                IP = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SearchDetails", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "Seminar", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Title = table.Column<string>(nullable: false),
                SubTitle = table.Column<string>(nullable: true),
                Description = table.Column<string>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Seminar", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "SystemSetting", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Name = table.Column<string>(nullable: false),
                Value = table.Column<string>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SystemSetting", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "UserInfo", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Username = table.Column<string>(nullable: false),
                NickName = table.Column<string>(nullable: false),
                Password = table.Column<string>(nullable: false),
                SaltKey = table.Column<string>(nullable: false),
                IsAdmin = table.Column<bool>(nullable: false),
                Email = table.Column<string>(nullable: true),
                QQorWechat = table.Column<string>(nullable: true),
                Avatar = table.Column<string>(nullable: true),
                AccessToken = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_UserInfo", x => x.Id);
            });

            migrationBuilder.CreateTable(name: "Post", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Title = table.Column<string>(nullable: false),
                Author = table.Column<string>(maxLength: 24, nullable: false),
                Content = table.Column<string>(nullable: false),
                ProtectContent = table.Column<string>(nullable: true),
                PostDate = table.Column<DateTime>(nullable: false),
                ModifyDate = table.Column<DateTime>(nullable: false),
                IsFixedTop = table.Column<bool>(nullable: false),
                CategoryId = table.Column<int>(nullable: false),
                ResourceName = table.Column<string>(nullable: true),
                IsWordDocument = table.Column<bool>(nullable: false),
                Email = table.Column<string>(nullable: false),
                Label = table.Column<string>(maxLength: 256, nullable: true),
                Keyword = table.Column<string>(maxLength: 256, nullable: true),
                VoteUpCount = table.Column<int>(nullable: false),
                VoteDownCount = table.Column<int>(nullable: false),
                IsBanner = table.Column<bool>(nullable: false),
                Description = table.Column<string>(maxLength: 255, nullable: true),
                ImageUrl = table.Column<string>(maxLength: 255, nullable: true),
                AverageViewCount = table.Column<double>(nullable: false),
                TotalViewCount = table.Column<int>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Post", x => x.Id);
                table.ForeignKey(name: "FK_Post_Category_CategoryId", column: x => x.CategoryId, principalTable: "Category", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable(name: "LoginRecord", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                IP = table.Column<string>(nullable: true),
                LoginTime = table.Column<DateTime>(nullable: false),
                Province = table.Column<string>(nullable: true),
                PhysicAddress = table.Column<string>(nullable: true),
                LoginType = table.Column<int>(nullable: false),
                UserInfoId = table.Column<int>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_LoginRecord", x => x.Id);
                table.ForeignKey(name: "FK_LoginRecord_UserInfo_UserInfoId", column: x => x.UserInfoId, principalTable: "UserInfo", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable(name: "Comment", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                NickName = table.Column<string>(maxLength: 24, nullable: false),
                Email = table.Column<string>(nullable: true),
                QQorWechat = table.Column<string>(nullable: true),
                Content = table.Column<string>(nullable: false),
                ParentId = table.Column<int>(nullable: false),
                PostId = table.Column<int>(nullable: false),
                CommentDate = table.Column<DateTime>(nullable: false),
                Browser = table.Column<string>(maxLength: 255, nullable: true),
                OperatingSystem = table.Column<string>(maxLength: 255, nullable: true),
                IsMaster = table.Column<bool>(nullable: false),
                VoteCount = table.Column<int>(nullable: false),
                AgainstCount = table.Column<int>(nullable: false),
                IP = table.Column<string>(nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Comment", x => x.Id);
                table.ForeignKey(name: "FK_Comment_Post_PostId", column: x => x.PostId, principalTable: "Post", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable(name: "PostAccessRecord", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                PostId = table.Column<int>(nullable: false),
                AccessTime = table.Column<DateTime>(nullable: false),
                ClickCount = table.Column<int>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PostAccessRecord", x => x.Id);
                table.ForeignKey(name: "FK_PostAccessRecord_Post_PostId", column: x => x.PostId, principalTable: "Post", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable(name: "PostHistoryVersion", columns: table => new
            {
                Id = table.Column<int>(nullable: false).Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                Status = table.Column<int>(nullable: false),
                Title = table.Column<string>(maxLength: 64, nullable: false),
                Content = table.Column<string>(nullable: false),
                ProtectContent = table.Column<string>(nullable: true),
                ViewCount = table.Column<int>(nullable: false),
                ModifyDate = table.Column<DateTime>(nullable: false),
                CategoryId = table.Column<int>(nullable: false),
                PostId = table.Column<int>(nullable: false),
                ResourceName = table.Column<string>(nullable: true),
                IsWordDocument = table.Column<bool>(nullable: false),
                Email = table.Column<string>(maxLength: 255, nullable: true),
                Label = table.Column<string>(maxLength: 255, nullable: true)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PostHistoryVersion", x => x.Id);
                table.ForeignKey(name: "FK_PostHistoryVersion_Category_CategoryId", column: x => x.CategoryId, principalTable: "Category", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "FK_PostHistoryVersion_Post_PostId", column: x => x.PostId, principalTable: "Post", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable(name: "SeminarPost", columns: table => new
            {
                Seminar_Id = table.Column<int>(nullable: false),
                Post_Id = table.Column<int>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SeminarPost", x => new
                {
                    x.Seminar_Id,
                    x.Post_Id
                });
                table.ForeignKey(name: "FK_SeminarPost_Post_Post_Id", column: x => x.Post_Id, principalTable: "Post", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "FK_SeminarPost_Seminar_Seminar_Id", column: x => x.Seminar_Id, principalTable: "Seminar", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateTable(name: "SeminarPostHistoryVersion", columns: table => new
            {
                Seminar_Id = table.Column<int>(nullable: false),
                PostHistoryVersion_Id = table.Column<int>(nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_SeminarPostHistoryVersion", x => new
                {
                    x.Seminar_Id,
                    x.PostHistoryVersion_Id
                });
                table.ForeignKey(name: "FK_SeminarPostHistoryVersion_PostHistoryVersion_PostHistoryVersion_Id", column: x => x.PostHistoryVersion_Id, principalTable: "PostHistoryVersion", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "FK_SeminarPostHistoryVersion_Seminar_Seminar_Id", column: x => x.Seminar_Id, principalTable: "Seminar", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

            migrationBuilder.CreateIndex(name: "IX_Comment_PostId", table: "Comment", column: "PostId");

            migrationBuilder.CreateIndex(name: "IX_LoginRecord_UserInfoId", table: "LoginRecord", column: "UserInfoId");

            migrationBuilder.CreateIndex(name: "IX_Post_CategoryId", table: "Post", column: "CategoryId");

            migrationBuilder.CreateIndex(name: "IX_PostAccessRecord_PostId", table: "PostAccessRecord", column: "PostId");

            migrationBuilder.CreateIndex(name: "IX_PostHistoryVersion_CategoryId", table: "PostHistoryVersion", column: "CategoryId");

            migrationBuilder.CreateIndex(name: "IX_PostHistoryVersion_PostId", table: "PostHistoryVersion", column: "PostId");

            migrationBuilder.CreateIndex(name: "IX_SeminarPost_Post_Id", table: "SeminarPost", column: "Post_Id");

            migrationBuilder.CreateIndex(name: "IX_SeminarPostHistoryVersion_PostHistoryVersion_Id", table: "SeminarPostHistoryVersion", column: "PostHistoryVersion_Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Broadcast");

            migrationBuilder.DropTable(name: "Comment");

            migrationBuilder.DropTable(name: "Donate");

            migrationBuilder.DropTable(name: "FastShare");

            migrationBuilder.DropTable(name: "InternalMessage");

            migrationBuilder.DropTable(name: "LeaveMessage");

            migrationBuilder.DropTable(name: "Links");

            migrationBuilder.DropTable(name: "LoginRecord");

            migrationBuilder.DropTable(name: "Menu");

            migrationBuilder.DropTable(name: "Misc");

            migrationBuilder.DropTable(name: "Notice");

            migrationBuilder.DropTable(name: "PostAccessRecord");

            migrationBuilder.DropTable(name: "SearchDetails");

            migrationBuilder.DropTable(name: "SeminarPost");

            migrationBuilder.DropTable(name: "SeminarPostHistoryVersion");

            migrationBuilder.DropTable(name: "SystemSetting");

            migrationBuilder.DropTable(name: "UserInfo");

            migrationBuilder.DropTable(name: "PostHistoryVersion");

            migrationBuilder.DropTable(name: "Seminar");

            migrationBuilder.DropTable(name: "Post");

            migrationBuilder.DropTable(name: "Category");
        }
    }
}