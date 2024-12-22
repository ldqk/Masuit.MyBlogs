using Riok.Mapperly.Abstractions;

namespace Masuit.MyBlogs.Core.Models;

[Mapper]
public static partial class CategoryMapper
{
    public static partial Category ToCategory(this CategoryCommand cmd);

    [MapProperty("Parent", nameof(Category.Parent))]
    public static partial CategoryDto_P ToDto_P(this Category category);

    [MapProperty("Parent", nameof(Category.Parent))]
    public static partial List<CategoryDto_P> ToDto_P(this IEnumerable<Category> categories);

    public static partial CategoryDto ToDto(this Category category);

    public static partial List<CategoryDto> ToDto(this IEnumerable<CategoryCommand> commands);

    public static partial CategoryDto ToDto(this CategoryCommand cmd);

    public static partial IQueryable<CategoryCommand> ProjectCommand(this IQueryable<Category> q);

    private static int? MapParentId(int? pid) => pid > 0 ? pid : null;
}