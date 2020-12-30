using Masuit.MyBlogs.Core.Models.Drive;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Infrastructure.Drive
{
    public interface IDriveService
    {
        public Task<List<DriveFile>> GetRootItems(string siteName, bool showHiddenFolders);
        public Task<List<DriveFile>> GetDriveItemsByPath(string path, string siteName, bool showHiddenFolders);
        public Task<DriveFile> GetDriveItemByPath(string path, string siteName);
        public Task<string> GetUploadUrl(string path, string siteName = "onedrive");
    }
}