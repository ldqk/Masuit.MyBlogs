using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Models.Drive;
using Microsoft.Graph;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Infrastructure.Drive
{
    public class DriveService : IDriveService
    {
        readonly IDriveAccountService _accountService;
        readonly GraphServiceClient _graph;
        readonly DriveContext _driveContext;

        public DriveService(IDriveAccountService accountService, DriveContext driveContext)
        {
            _accountService = accountService;
            _graph = accountService.Graph;
            _driveContext = driveContext;
        }
        /// <summary>
        /// 获取根目录的所有项目
        /// </summary>
        /// <returns></returns>
        public async Task<List<DriveFile>> GetRootItems(string siteName = "onedrive", bool showHiddenFolders = false)
        {
            var drive = siteName != "onedrive" ? _graph.Sites[GetSiteId(siteName)].Drive : _graph.Me.Drive;
            var result = await drive.Root.Children.Request().GetAsync();
            List<DriveFile> files = GetItems(result, siteName, showHiddenFolders);
            return files;
        }

        /// <summary>
        /// 根据路径获取文件夹下所有项目
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<List<DriveFile>> GetDriveItemsByPath(string path, string siteName = "onedrive", bool showHiddenFolders = false)
        {
            var drive = siteName != "onedrive" ? _graph.Sites[GetSiteId(siteName)].Drive : _graph.Me.Drive;
            var result = await drive.Root.ItemWithPath(path).Children.Request().GetAsync();
            List<DriveFile> files = GetItems(result, siteName, showHiddenFolders);
            return files;
        }
        /// <summary>
        /// 根据路径获取项目
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<DriveFile> GetDriveItemByPath(string path, string siteName = "onedrive")
        {
            string[] imgArray = { ".png", ".jpg", ".jpeg", ".bmp", ".webp" };
            string extension = Path.GetExtension(path);
            var drive = siteName != "onedrive" ? _graph.Sites[GetSiteId(siteName)].Drive : _graph.Me.Drive;
            //这么写是因为：分块上传图片后直接获取会报错。
            if (imgArray.Contains(extension))
            {
                await drive.Root.ItemWithPath(path).Thumbnails.Request().GetAsync();
            }
            var result = await drive.Root.ItemWithPath(path).Request().GetAsync();
            DriveFile file = GetItem(result);
            return file;
        }

        /// <summary>
        /// 获得上传url
        /// </summary>
        /// <param name="path"></param>
        /// <param name="siteName"></param>
        /// <returns></returns>
        public async Task<string> GetUploadUrl(string path, string siteName = "onedrive")
        {
            var drive = siteName != "onedrive" ? _graph.Sites[GetSiteId(siteName)].Drive : _graph.Me.Drive;
            string requestUrl = drive.Root.ItemWithPath(path).CreateUploadSession().Request().RequestUrl;
            ProtectedApiCallHelper apiCallHelper = new ProtectedApiCallHelper(new HttpClient());
            string uploadUrl = "";
            await apiCallHelper.CallWebApiAndProcessResultASync(requestUrl, _accountService.GetToken(), o =>
            {
                uploadUrl = o["uploadUrl"].ToString();
            }, ProtectedApiCallHelper.Method.Post);
            return uploadUrl;
        }

        #region PrivateMethod
        private DriveFile GetItem(DriveItem result)
        {
            DriveFile file = new DriveFile()
            {
                CreatedTime = result.CreatedDateTime,
                Name = result.Name,
                Size = result.Size,
                Id = result.Id
            };
            if (result.AdditionalData != null)
            {
                //可能是文件夹
                if (result.AdditionalData.TryGetValue("@microsoft.graph.downloadUrl", out var downloadUrl))
                {
                    var dlurl = (string)downloadUrl;
                    ReplaceCDNUrls(ref dlurl);
                    file.DownloadUrl = dlurl;
                }
            }

            return file;
        }

        private List<DriveFile> GetItems(IDriveItemChildrenCollectionPage result, string siteName = "onedrive", bool showHiddenFolders = false)
        {
            List<DriveFile> files = new List<DriveFile>();
            foreach (var item in result)
            {
                //要隐藏文件
                if (!showHiddenFolders)
                {
                    //跳过隐藏的文件
                    string[] hiddenFolders = _driveContext.Sites.Single(site => site.Name == siteName).HiddenFolders;
                    if (hiddenFolders != null)
                    {
                        if (hiddenFolders.Any(str => str == item.Name))
                        {
                            continue;
                        }
                    }
                }
                DriveFile file = new DriveFile()
                {
                    CreatedTime = item.CreatedDateTime,
                    Name = item.Name,
                    Size = item.Size,
                    Id = item.Id
                };
                if (item.AdditionalData != null)
                {
                    //可能是文件夹
                    if (item.AdditionalData.TryGetValue("@microsoft.graph.downloadUrl", out var downloadUrl))
                    {
                        var dlurl = (string)downloadUrl;
                        ReplaceCDNUrls(ref dlurl);
                        file.DownloadUrl = dlurl;
                    }
                }
                files.Add(file);
            }

            return files;
        }

        /// <summary>
        /// 根据名称返回siteid
        /// </summary>
        /// <returns></returns>
        private string GetSiteId(string siteName)
        {
            var site = _driveContext.Sites.SingleOrDefault(s => s.Name == siteName);
            return site?.SiteId;
        }

        private void ReplaceCDNUrls(ref string downloadUrl)
        {
            if (OneDriveConfiguration.CDNUrls.Length != 0)
            {
                downloadUrl = OneDriveConfiguration.CDNUrls.Select(item => item.Split(";")).Aggregate(downloadUrl, (current, a) => current.Replace(a[0], a[1]));
            }
        }
        #endregion
    }
}