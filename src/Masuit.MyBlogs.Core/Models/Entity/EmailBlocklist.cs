using Masuit.LuceneEFCore.SearchEngine;

namespace Masuit.MyBlogs.Core.Models.Entity;

[Table(nameof(EmailBlocklist))]
public class EmailBlocklist : LuceneIndexableBaseEntity
{
    public string Email { get; set; }
}
