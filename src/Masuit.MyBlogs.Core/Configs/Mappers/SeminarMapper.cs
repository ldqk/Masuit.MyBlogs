using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class SeminarMapper
{
    public static partial SeminarDto ToDto(this Seminar seminar);

    public static partial IQueryable<SeminarDto> ProjectDto(this IQueryable<Seminar> q);
}