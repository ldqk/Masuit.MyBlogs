using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class AdvertisementMapper
{
    [MapProperty(nameof(advertisement.ClickRecords), nameof(AdvertisementViewModel.AverageViewCount))]
    [MapProperty(nameof(advertisement.ClickRecords), nameof(AdvertisementViewModel.ViewCount))]
    public static partial AdvertisementViewModel ToViewModel(this Advertisement advertisement);

    [MapperIgnoreTarget(nameof(Advertisement.ClickRecords))]
    [MapperIgnoreTarget(nameof(Advertisement.Status))]
    [MapValue(nameof(Advertisement.UpdateTime), Use = nameof(GetTime))]
    public static partial Advertisement ToEntity(this AdvertisementDto advertisement);

    [MapperIgnoreTarget(nameof(Advertisement.ClickRecords))]
    [MapperIgnoreTarget(nameof(Advertisement.Status))]
    [MapValue(nameof(Advertisement.UpdateTime), Use = nameof(GetTime))]
    public static partial void Update(this AdvertisementDto dto, Advertisement advertisement);

    private static double MapAverageViewCount(ICollection<AdvertisementClickRecord> records) => records.Where(o => o.Time >= DateTime.Today.AddMonths(-1)).GroupBy(r => r.Time.Date).Select(g => g.Count()).DefaultIfEmpty().Average();

    private static int MapViewCount(ICollection<AdvertisementClickRecord> records) => records.Count(o => o.Time >= DateTime.Today.AddMonths(-1));

    private static DateTime GetTime() => DateTime.Now;

    public static partial IQueryable<AdvertisementViewModel> ProjectViewModel(this IQueryable<Advertisement> q);

    public static partial IQueryable<AdvertisementDto> ProjectDto(this IQueryable<Advertisement> q);
}