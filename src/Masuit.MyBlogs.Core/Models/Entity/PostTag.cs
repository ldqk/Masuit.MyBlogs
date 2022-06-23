using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity;

[Table("PostTag")]
public class PostTag : BaseEntity
{
    public string Name { get; set; }

    public int Count { get; set; }
}
