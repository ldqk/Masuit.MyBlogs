using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class AdvertisementClickRecordMapper
{
    [MapProperty(nameof(AdvertisementClickRecord.Time), nameof(AdvertisementClickRecordViewModel.Time), StringFormat = "yyyy-MM-dd")]
    public static partial AdvertisementClickRecordViewModel ToDto(this AdvertisementClickRecord record);

    public static partial IQueryable<AdvertisementClickRecordViewModel> ProjectViewModel(this IQueryable<AdvertisementClickRecord> q);
}