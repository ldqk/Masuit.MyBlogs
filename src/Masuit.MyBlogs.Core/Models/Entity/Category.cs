using Masuit.Tools.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Masuit.MyBlogs.Core.Models.Entity;

/// <summary>
/// 文章分类
/// </summary>
[Table("Category")]
public class Category : BaseEntity, ITree<Category>, IEntityTypeConfiguration<Category>, ITreeEntity<Category, int>
{
    public Category()
    {
        Post = new HashSet<Post>();
        Status = Status.Available;
    }

    /// <summary>
    /// 分类名
    /// </summary>
    [Required(ErrorMessage = "分类名不能为空"), MaxLength(64, ErrorMessage = "分类名最大允许64个字符"), MinLength(2, ErrorMessage = "分类名至少2个字符")]
    public string Name { get; set; }

    /// <summary>
    /// 分类描述
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 父级id
    /// </summary>
    public int? ParentId { get; set; }

    public string Path { get; set; }

    public virtual ICollection<Post> Post { get; set; }

    public virtual ICollection<PostHistoryVersion> PostHistoryVersion { get; set; }

    /// <summary>
    /// 父节点
    /// </summary>
    public virtual Category Parent { get; set; }

    /// <summary>
    /// 子级
    /// </summary>
    public virtual ICollection<Category> Children { get; set; }

    /// <summary>
    ///     Configures the entity of type <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasMany(e => e.Post).WithOne(e => e.Category).IsRequired(false).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.PostHistoryVersion).WithOne(e => e.Category).HasForeignKey(r => r.CategoryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(e => e.Children).WithOne(c => c.Parent).IsRequired(false).HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(c => c.Path).IsRequired();
    }
}
