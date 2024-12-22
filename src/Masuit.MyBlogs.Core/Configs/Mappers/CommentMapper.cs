using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class CommentMapper
{
    [MapValue(nameof(Comment.Status), Status.Pending)]
    public static partial Comment ToComment(this CommentCommand cmd);

    public static partial CommentDto ToDto(this Comment comment);

    [MapProperty(nameof(Comment.CommentDate), nameof(CommentViewModel.CommentDate), StringFormat = "yyyy-MM-dd")]
    public static partial CommentViewModel ToViewModel(this Comment comment);

    public static partial List<CommentViewModel> ToViewModel(this IEnumerable<Comment> comments);

    public static partial CommentDto ToDto(this CommentCommand cmd);

    private static int? MapParentId(int? pid) => pid > 0 ? pid : null;

    public static partial IQueryable<CommentDto> ProjectDto(this IQueryable<Comment> q);
}