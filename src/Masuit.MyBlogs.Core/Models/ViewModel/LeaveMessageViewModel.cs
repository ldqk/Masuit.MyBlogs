using Masuit.Tools.Models;

namespace Masuit.MyBlogs.Core.Models.ViewModel;

/// <summary>
/// 留言板视图模型
/// </summary>
public class LeaveMessageViewModel : BaseEntity, ITreeEntity<LeaveMessageViewModel, int>
{
    /// <summary>
    /// 昵称
    /// </summary>
    public string NickName { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// 发表时间
    /// </summary>
    public string PostDate { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// 父级ID
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 浏览器版本
    /// </summary>
    public string Browser { get; set; }

    /// <summary>
    /// 操作系统版本
    /// </summary>
    public string OperatingSystem { get; set; }

    /// <summary>
    /// 是否是博主
    /// </summary>
    public bool IsMaster { get; set; }

    /// <summary>
    /// 提交人IP地址
    /// </summary>
    public string IP { get; set; }

    /// <summary>
    /// 地理信息
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// 子级
    /// </summary>
    public ICollection<LeaveMessageViewModel> Children { get; set; }
}
