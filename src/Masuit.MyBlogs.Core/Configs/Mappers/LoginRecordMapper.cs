using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class LoginRecordMapper
{
    public static partial LoginRecordViewModel ToViewModel(this LoginRecord record);

    public static partial IQueryable<LoginRecordViewModel> ProjectViewModel(this IQueryable<LoginRecord> q);
}