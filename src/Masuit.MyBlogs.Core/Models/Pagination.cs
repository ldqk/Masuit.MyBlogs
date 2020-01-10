using Masuit.MyBlogs.Core.Models.Enum;

namespace Masuit.MyBlogs.Core.Models
{
    public class Pagination
    {
        public Pagination(int page, int size, OrderBy? orderBy = null)
        {
            Page = page;
            Size = size;
            OrderBy = orderBy;
        }

        public int Page { get; set; } = 1;
        public int Size { get; set; } = 15;
        public OrderBy? OrderBy { get; set; }
    }
}