using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class PostMergeRequestMapper
{
    [MapProperty("Post", nameof(PostMergeRequestDto.PostTitle))]
    public static partial PostMergeRequestDto ToDto(this PostMergeRequest request);

    [MapProperty("Post", nameof(PostMergeRequestDto.PostTitle))]
    public static partial PostMergeRequestDtoBase ToDtoBase(this PostMergeRequest request);

    private static string MapPostTitle(Post p) => p.Title;

    [MapperIgnoreTarget(nameof(cmd.Id))]
    [MapperIgnoreTarget(nameof(PostMergeRequest.MergeState))]
    public static partial PostMergeRequest ToEntity(this PostMergeRequestCommand cmd);

    [MapperIgnoreTarget(nameof(cmd.Id))]
    [MapperIgnoreTarget(nameof(Post.Status))]
    public static partial Post ToPost(this PostMergeRequestCommand cmd);

    [MapperIgnoreTarget(nameof(request.Id))]
    [MapperIgnoreTarget(nameof(Post.Status))]
    public static partial Post ToPost(this PostMergeRequest request);

    [MapperIgnoreTarget(nameof(request.Id))]
    [MapperIgnoreTarget(nameof(Post.Status))]
    public static partial void UpdatePost(this PostMergeRequest request, Post post);

    [MapperIgnoreTarget(nameof(request.Id))]
    [MapperIgnoreTarget(nameof(Post.Status))]
    public static partial void ApplyUpdate([MappingTarget] this Post post, PostMergeRequest request);

    [MapperIgnoreTarget(nameof(request.Id))]
    [MapperIgnoreTarget(nameof(request.Status))]
    public static partial void Update(this PostMergeRequestCommandBase cmd, PostMergeRequest request);

    [MapperIgnoreTarget(nameof(post.Id))]
    [MapperIgnoreTarget(nameof(post.Status))]
    public static partial void Update(this PostMergeRequestCommand dto, Post post);

    public static partial IQueryable<PostMergeRequestDto> ProjectDto(this IQueryable<PostMergeRequest> q);

    public static partial IQueryable<PostMergeRequestDtoBase> ProjectDtoBase(this IQueryable<PostMergeRequest> q);
}