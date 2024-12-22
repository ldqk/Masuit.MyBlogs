using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class NoticeMapper
{
    public static partial NoticeDto ToDto(this Notice notice);

    public static partial Notice ToEntity(this NoticeDto dto);

    public static partial IQueryable<NoticeDto> ProjectDto(this IQueryable<Notice> q);
}