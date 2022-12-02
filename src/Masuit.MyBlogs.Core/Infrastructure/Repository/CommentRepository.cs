using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository;

public sealed partial class CommentRepository : BaseRepository<Comment>, ICommentRepository
{
}