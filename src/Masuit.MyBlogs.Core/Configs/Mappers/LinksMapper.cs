using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class LinksMapper
{
    public static partial LinksDto ToDto(this Links links);

    private static int MapLoopbacks(ICollection<LinkLoopback> loopbacks) => loopbacks.GroupBy(e =>
        e.IP).Count();

    public static partial IQueryable<LinksDto> ProjectDto(this IQueryable<Links> q);
}