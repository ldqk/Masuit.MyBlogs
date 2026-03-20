using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class AdvertisementClickRecordMapper
{
    [MapProperty(nameof(AdvertisementClickRecord.Time), nameof(AdvertisementClickRecordViewModel.Time), StringFormat = "yyyy-MM-dd HH:mm:ss")]
    public static partial AdvertisementClickRecordViewModel ToDto(this AdvertisementClickRecord record);

    public static partial IQueryable<AdvertisementClickRecordViewModel> ProjectViewModel(this IQueryable<AdvertisementClickRecord> q);
}