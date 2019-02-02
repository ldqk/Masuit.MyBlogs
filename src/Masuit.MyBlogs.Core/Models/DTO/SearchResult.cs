using System.Collections.Generic;

namespace Masuit.MyBlogs.Core.Models.DTO
{
    public class SearchResult<T>
    {
        public int Total { get; set; }
        public double Elapsed { get; set; }
        public List<T> Results { get; set; }
    }
}