using System.ComponentModel.DataAnnotations;

namespace Masuit.MyBlogs.Core.Models.Drive
{
    public class Site
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string SiteId { get; set; }
        public string NickName { get; set; }
        public string[] HiddenFolders { get; set; }
    }
}