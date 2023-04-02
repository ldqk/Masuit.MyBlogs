using System.ComponentModel;

namespace Masuit.MyBlogs.Core.Models.Enum;

/// <summary>
/// 广告类型
/// </summary>
public enum AdvertiseType
{
    [Description("首页头图")]
    Banner = 1,

    [Description("文章列表")]
    ListItem = 2,

    [Description("边栏")]
    SideBar = 3,

    [Description("内页")]
    InPage = 4
}
