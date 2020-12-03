using System;

namespace Masuit.MyBlogs.Core.Models.Drive
{
    public class DriveFile
    {
        public string Name { get; set; }
        public string DownloadUrl { get; set; }
        public long? Size { get; set; }
        public DateTimeOffset? CreatedTime { get; set; }
        public string Id { get; set; }
    }
    
}