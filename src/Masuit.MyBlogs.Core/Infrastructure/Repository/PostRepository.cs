using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }
}