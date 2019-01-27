using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    [Table(nameof(SeminarPost))]
    public class SeminarPost
    {
        //[Key]
        //public string Id { get; set; }

        [ForeignKey("Seminar_Id")]
        public int SeminarId { get; set; }

        [ForeignKey("Post_Id")]
        public int PostId { get; set; }

        public virtual Seminar Seminar { get; set; }

        public virtual Post Post { get; set; }
    }
}