using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class LeaveMessageMapper
{
    [MapValue(nameof(LeaveMessage.Status), Status.Pending)]
    public static partial LeaveMessage ToLeaveMessage(this LeaveMessageCommand cmd);

    public static partial LeaveMessageDto ToDto(this LeaveMessage message);

    [MapProperty(nameof(LeaveMessage.PostDate), nameof(LeaveMessageViewModel.PostDate), StringFormat = "yyyy-MM-dd")]
    public static partial LeaveMessageViewModel ToViewModel(this LeaveMessage message);

    public static partial List<LeaveMessageViewModel> ToViewModel(this IEnumerable<LeaveMessage> messages);

    public static partial LeaveMessageDto ToDto(this LeaveMessageCommand cmd);

    public static partial IQueryable<LeaveMessageDto> ProjectDto(this IQueryable<LeaveMessage> q);

    private static int? MapParentId(int? pid) => pid > 0 ? pid : null;
}