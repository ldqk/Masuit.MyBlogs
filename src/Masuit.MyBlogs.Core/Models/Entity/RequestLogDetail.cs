using Masuit.Tools.Systems;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

    /// <summary>
    /// 请求时间
    /// </summary>
    [Column(TypeName = "timestamp")]
    public DateTime Time { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    [StringLength(1024), Unicode]
    public string UserAgent { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    [StringLength(4096), Unicode]
    public string RequestUrl { get; set; }

    /// <summary>
    /// 客户端IP
    /// </summary>
    [StringLength(128), Unicode]
    public string IP { get; set; }

    /// <summary>
    /// 客户端完整地理信息
    /// </summary>
    [StringLength(256), Unicode]
    public string Location { get; set; }

    /// <summary>
    /// 国家
    /// </summary>
    [StringLength(256), Unicode]
    public string Country { get; set; }

    /// <summary>
    /// 城市
    /// </summary>
    [StringLength(256), Unicode]
    public string City { get; set; }

    /// <summary>
    /// 运营商网络
    /// </summary>
    [StringLength(256)]
    public string Network { get; set; }

    /// <summary>
    /// 跟踪id
    /// </summary>
    [StringLength(128)]
    public string TraceId { get; set; }
}
