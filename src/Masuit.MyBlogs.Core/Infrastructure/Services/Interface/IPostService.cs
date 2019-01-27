using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Masuit.MyBlogs.Core.Infrastructure.Services.Interface
{
    public partial interface IPostService : IBaseService<Post>
    {
        List<PostOutputDto> SearchPage<TOrder>(int page, int size, out int total, string[] keywords, Expression<Func<Post, TOrder>> orderBy);
    }
}