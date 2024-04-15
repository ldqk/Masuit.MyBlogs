using Masuit.MyBlogs.Core.Models.Validation;
using Masuit.Tools.Core.Validator;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// 留言板
/// </summary>
[Table("LeaveMessage")]
public class LeaveMessage : BaseEntity, ITreeParent<LeaveMessage>, ITreeChildren<LeaveMessage>, IEntityTypeConfiguration<LeaveMessage>
{
    public LeaveMessage()
    {
        PostDate = DateTime.Now;
        Status = Status.Pending;
        IsMaster = false;
        Children = new List<LeaveMessage>();
    }

    /// <summary>
    /// 昵称
    /// </summary>
    [Required(ErrorMessage = "昵称不能为空！")]
    public string NickName { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [Required(ErrorMessage = "留言内容不能为空！"), SubmitCheck]
    public string Content { get; set; }

    /// <summary>
    /// 发表时间
    /// </summary>
    public DateTime PostDate { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [IsEmail]
    public string Email { get; set; }

    /// <summary>
    /// 父级ID
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 浏览器版本
    /// </summary>
    [StringLength(255)]
    public string Browser { get; set; }

    /// <summary>
    /// 操作系统版本
    /// </summary>
    [StringLength(255)]
    public string OperatingSystem { get; set; }

    /// <summary>
    /// 是否是博主
    /// </summary>
    [DefaultValue(false)]
    public bool IsMaster { get; set; }

    /// <summary>
    /// 提交人IP
    /// </summary>
    public string IP { get; set; }

    /// <summary>
    /// 地理信息
    /// </summary>
    public string Location { get; set; }

    public string GroupTag { get; set; }

    public string Path { get; set; }

    /// <summary>
    /// 父节点
    /// </summary>
    public LeaveMessage Parent { get; set; }

    /// <summary>
    /// 子级
    /// </summary>
    public ICollection<LeaveMessage> Children { get; set; }

    /// <summary>
    ///     Configures the entity of type <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<LeaveMessage> builder)
    {
        builder.HasMany(e => e.Children).WithOne(c => c.Parent).HasForeignKey(c => c.ParentId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        builder.Property(c => c.Path).IsRequired();
        builder.Property(c => c.GroupTag).IsRequired();
    }
}
