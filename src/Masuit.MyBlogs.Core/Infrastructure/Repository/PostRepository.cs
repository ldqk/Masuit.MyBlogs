using Masuit.MyBlogs.Core.Infrastructure.Repository.Interface;
using Masuit.MyBlogs.Core.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Infrastructure.Repository
{
    public partial class PostRepository : BaseRepository<Post>, IPostRepository
    {
        /// <summary>
        /// 添加实体
        /// </summary>
        /// <param name="t">需要添加的实体</param>
        /// <returns>添加成功</returns>
        public override Post AddEntity(Post t)
        {
            DataContext.Add(t);
            return t;
        }

        /// <summary>
        /// 获取第一条数据，优先从缓存读取
        /// </summary>
        /// <param name="where">查询条件</param>
        /// <returns>实体</returns>
        public override Task<Post> GetAsync(Expression<Func<Post, bool>> @where)
        {
            return DataContext.Post.Include(p => p.Category).Include(p => p.Seminar).FirstOrDefaultAsync(@where);
        }

    }
}