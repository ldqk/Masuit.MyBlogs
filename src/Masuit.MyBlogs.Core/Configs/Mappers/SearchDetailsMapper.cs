using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class SearchDetailsMapper
{
    public static partial SearchDetailsDto ToDto(this SearchDetails details);

    public static partial IQueryable<SearchDetailsDto> ProjectDto(this IQueryable<SearchDetails> q);
}