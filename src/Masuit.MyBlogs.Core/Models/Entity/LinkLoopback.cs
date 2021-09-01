using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Masuit.MyBlogs.Core.Models.Entity
{
    [Table("LinkLoopback")]
    public class LinkLoopback : BaseEntity
    {
        public string Referer { get; set; }
        public string IP { get; set; }
        public DateTime Time { get; set; }
        public int LinkId { get; set; }

        public virtual Links Links { get; set; }
    }
}