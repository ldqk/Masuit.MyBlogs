using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class MiscMapper
{
    public static partial MiscDto ToDto(this Misc misc);

    public static partial IQueryable<MiscDto> ProjectDto(this IQueryable<Misc> q);
}