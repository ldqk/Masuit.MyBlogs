using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class PostVisitRecordMapper
{
    [MapProperty(nameof(PostVisitRecord.Time), nameof(PostVisitRecordViewModel.Time), StringFormat = "yyyy-MM-dd")]
    public static partial PostVisitRecordViewModel ToDto(this PostVisitRecord record);

    public static partial IQueryable<PostVisitRecordViewModel> ProjectViewModel(this IQueryable<PostVisitRecord> q);
}