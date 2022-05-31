using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Masuit.MyBlogs.Core.Infrastructure;
using Masuit.Tools.Systems;
using Microsoft.EntityFrameworkCore;

namespace Masuit.MyBlogs.Core.Models.Entity;

[Table(nameof(RequestLogDetail))]
public class RequestLogDetail
{
    public RequestLogDetail()
    {
        Id = SnowFlake.NewId;
    }

    [StringLength(32)]
    public string Id { get; set; }

    [Column(TypeName = "timestamp"), HypertableColumn]
    public DateTime Time { get; set; }

    [StringLength(1024), Unicode]
    public string UserAgent { get; set; }

    [StringLength(4096), Unicode]
    public string RequestUrl { get; set; }

    [StringLength(128), Unicode]
    public string IP { get; set; }

    [StringLength(256), Unicode]
    public string Location { get; set; }

    [StringLength(256)]
    public string Network { get; set; }
}
