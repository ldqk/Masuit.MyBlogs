using Masuit.MyBlogs.Core.Models.Entity;
using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class PostMapper
{
    [MapProperty("Category", nameof(PostDto.CategoryName))]
    [MapProperty("Comment", nameof(PostDto.CommentCount))]
    [MapProperty(nameof(Post.LimitMode), nameof(PostDto.LimitMode))]
    [MapperIgnoreTarget(nameof(PostDto.Category))]
    public static partial PostDto ToDto(this Post post);

    public static partial PostModelBase ToModelBase(this Post post);

    public static partial PostMergeRequestDto ToPostMergeRequestDto(this Post post);

    [MapProperty("PostHistoryVersion", nameof(PostDataModel.ModifyCount))]
    [MapProperty(nameof(post.TotalViewCount), nameof(PostDataModel.ViewCount))]
    [MapProperty("Seminar", nameof(PostDataModel.Seminars))]
    public static partial PostDataModel MapDataModel(Post post);

    [MapperIgnoreTarget(nameof(post.Id))]
    [MapProperty(nameof(post.Id), nameof(PostHistoryVersion.PostId))]
    public static partial PostHistoryVersion ToHistoryVersion(this Post post);

    public static partial Post ToPost(this PostCommand cmd);

    public static partial void Update(this PostCommand cmd, Post p);

    public static partial PostDto ToPostDto(this PostCommand cmd);

    [MapProperty("Category", nameof(PostDto.CategoryName))]
    public static partial PostDto ToPostDto(this PostHistoryVersion cmd);

    public static partial IQueryable<PostDto> ProjectDto(this IQueryable<Post> q);

    [MapProperty("PostHistoryVersion", nameof(PostDataModel.ModifyCount))]
    [MapProperty(nameof(Post.TotalViewCount), nameof(PostDataModel.ViewCount))]
    [MapProperty("Seminar", nameof(PostDataModel.Seminars))]
    public static partial IQueryable<PostDataModel> ProjectDataModel(this IQueryable<Post> q);

    public static partial IQueryable<PostModelBase> ProjectModelBase(this IQueryable<Post> q);

    public static string MapStatus(Status status) => status.GetDisplay();

    public static int MapModifyCount(ICollection<PostHistoryVersion> versions) => versions.Count;

    public static RegionLimitMode MapLimitMode(RegionLimitMode? limitMode) => limitMode ?? RegionLimitMode.All;

    public static string MapCategoryName(Category category) => category?.Name;

    public static int MapCommentCount(ICollection<Comment> comments) => comments.Count;

    public static int[] MapSeminars(ICollection<Seminar> seminars) => seminars.Select(s => s.Id).ToArray();
}