using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    [Table("PostVisitRecord")]
    public class PostVisitRecord : BaseEntity
    {
        public int PostId { get; set; }

        public string IP { get; set; }

        public string Location { get; set; }

        public string Referer { get; set; }

        public string RequestUrl { get; set; }

        public string RequestHeader { get; set; }

        public DateTime Time { get; set; }
    }
}
