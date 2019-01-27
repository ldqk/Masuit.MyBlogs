using Models.DTO;
using Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace IBLL
{
    public partial interface IPostBll
    {
        List<PostOutputDto> SearchPage<TOrder>(int page, int size, out int total, string[] keywords, Expression<Func<Post, TOrder>> orderBy);
    }
}