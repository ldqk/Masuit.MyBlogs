using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class DonateMapper
{
    public static partial DonateDto ToDto(this Donate donate);

    public static partial IQueryable<DonateDto> ProjectDto(this IQueryable<Donate> q);
}