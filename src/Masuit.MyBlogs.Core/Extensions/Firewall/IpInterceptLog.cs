namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public class IpInterceptLog
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [StringLength(32)]
    public string IP { get; set; }

    public string RequestUrl { get; set; }

    public string Referer { get; set; }

    [StringLength(256)]
    public string Address { get; set; }

    public string UserAgent { get; set; }

    public DateTime Time { get; set; }

    [StringLength(256)]
    public string Remark { get; set; }

    [StringLength(10)]
    public string HttpVersion { get; set; }

    public string Headers { get; set; }
}

public class IpReportLog
{
    [Key, StringLength(32), DatabaseGenerated(DatabaseGeneratedOption.Identity), ConcurrencyCheck]
    public string IP { get; set; }

    public DateTime Time { get; set; }
}
