namespace Masuit.MyBlogs.Core.Models.Enum;

public enum AdvertiseOrderBy
{
    [Display(Name = nameof(Default))]
    Default,

    [Display(Name = nameof(Price))]
    Price,

    [Display(Name = nameof(DisplayCount))]
    DisplayCount,

    [Display(Name = nameof(ViewCount))]
    ViewCount,

    [Display(Name = nameof(AverageViewCount))]
    AverageViewCount,

    [Display(Name = nameof(ClickRate))]
    ClickRate
}
