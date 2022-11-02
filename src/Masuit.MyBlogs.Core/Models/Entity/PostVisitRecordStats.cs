using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity;

[Table("PostVisitRecordStats")]
public class PostVisitRecordStats : BaseEntity
{
    public int PostId { get; set; }

    [ConcurrencyCheck]
    public DateTime Date { get; set; }

    [ConcurrencyCheck]
    public int Count { get; set; }

    [ConcurrencyCheck]
    public int UV { get; set; }
}
