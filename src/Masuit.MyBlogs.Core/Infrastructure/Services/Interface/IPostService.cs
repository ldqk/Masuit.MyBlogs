using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface
{
    public partial interface IPostService : IBaseService<Post>
    {
        SearchResult<PostOutputDto> SearchPage(int page, int size, string keyword);
    }
}