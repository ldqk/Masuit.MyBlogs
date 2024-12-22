using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class MenuMapper
{
    public static partial Menu ToMenu(this MenuCommand cmd);

    public static partial void Update(this MenuCommand cmd, Menu menu);

    [MapProperty(nameof(menu.Children), nameof(Menu.Children))]
    public static partial MenuDto ToDto(this Menu menu);

    public static partial List<MenuDto> ToDto(this IEnumerable<Menu> menus);

    private static int? MapParentId(int? pid) => pid > 0 ? pid : null;

    private static ICollection<MenuDto> MapChildren(ICollection<Menu> children) => children.OrderBy(c => c.Sort).Select(ToDto).ToList();

    public static partial IQueryable<MenuDto> ProjectDto(this IQueryable<Menu> q);
}