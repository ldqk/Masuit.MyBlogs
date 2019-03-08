using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using System.Data;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class PostRepository : BaseRepository<Post>, IPostRepository
    {
        public PostRepository(DataContext dbContext, IDbConnection connection) : base(dbContext, connection)
        {
        }
    }
}