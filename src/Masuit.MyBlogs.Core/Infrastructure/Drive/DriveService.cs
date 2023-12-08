using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Models.Drive;
using Microsoft.Graph;

namespace Masuit.MyBlogs.Core.Infrastructure.Drive;

public sealed class DriveService(IDriveAccountService accountService, DriveContext driveContext) : IDriveService
{
    /// <summary>
    /// 获取根目录的所有项目
    /// </summary>
    /// <returns></returns>
    public async Task<List<DriveFile>> GetRootItems(string siteName, bool showHiddenFolders)
    {
        var drive = siteName != "onedrive" ? accountService.Graph.Sites[GetSiteId(siteName)].Drive : accountService.Graph.Me.Drive;
        var request = drive.Root.Children.Request();
        var result = await request.GetAsync();
        var files = await GetItems(result, siteName, showHiddenFolders);
        return files.OrderByDescending(f => f.CreatedTime).ToList();
    }

    /// <summary>
    /// 根据路径获取文件夹下所有项目
    /// </summary>
    /// <param name="path"></param>
    /// <param name="siteName"></param>
    /// <param name="showHiddenFolders"></param>
    /// <returns></returns>
    public async Task<List<DriveFile>> GetDriveItemsByPath(string path, string siteName, bool showHiddenFolders)
    {
        var drive = siteName != "onedrive" ? accountService.Graph.Sites[GetSiteId(siteName)].Drive : accountService.Graph.Me.Drive;
        var result = await drive.Root.ItemWithPath(path).Children.Request().GetAsync();
        var files = await GetItems(result, siteName, showHiddenFolders);
        files = files.OrderByDescending(f => f.CreatedTime).ToList();
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
        var extension = Path.GetExtension(path);
        var drive = siteName != "onedrive" ? accountService.Graph.Sites[GetSiteId(siteName)].Drive : accountService.Graph.Me.Drive;

        //这么写是因为：分块上传图片后直接获取会报错。
        if (imgArray.Contains(extension))
        {
            await drive.Root.ItemWithPath(path).Thumbnails.Request().GetAsync();
        }
        var result = await drive.Root.ItemWithPath(path).Request().GetAsync();
        return GetItem(result);
    }

    /// <summary>
    /// 获得上传url
    /// </summary>
    /// <param name="path"></param>
    /// <param name="siteName"></param>
    /// <returns></returns>
    public async Task<string> GetUploadUrl(string path, string siteName = "onedrive")
    {
        var drive = siteName != "onedrive" ? accountService.Graph.Sites[GetSiteId(siteName)].Drive : accountService.Graph.Me.Drive;
        var requestUrl = drive.Root.ItemWithPath(path).CreateUploadSession().Request().RequestUrl;
        var apiCallHelper = new ProtectedApiCallHelper(new HttpClient());
        var uploadUrl = "";
        await apiCallHelper.CallWebApiAndProcessResultASync(requestUrl, accountService.GetToken(), o =>
        {
            uploadUrl = o["uploadUrl"].ToString();
        }, ProtectedApiCallHelper.Method.Post);
        return uploadUrl;
    }

    #region PrivateMethod

    private DriveFile GetItem(DriveItem result)
    {
        var file = new DriveFile()
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
                file.DownloadUrl = ReplaceCDNUrls(downloadUrl.ToString());
            }
        }

        return file;
    }

    private async Task<List<DriveFile>> GetItems(IDriveItemChildrenCollectionPage result, string siteName = "onedrive", bool showHiddenFolders = false)
    {
        var files = new List<DriveFile>();
        foreach (var item in result)
        {
            //要隐藏文件
            if (!showHiddenFolders)
            {
                //跳过隐藏的文件
                var hiddenFolders = driveContext.Sites.Single(site => site.Name == siteName).HiddenFolders;
                if (hiddenFolders != null)
                {
                    if (hiddenFolders.Any(str => str == item.Name))
                    {
                        continue;
                    }
                }
            }
            var file = new DriveFile()
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
                    file.DownloadUrl = ReplaceCDNUrls(downloadUrl.ToString());
                }
            }
            files.Add(file);
        }

        if (result.Count == 200)
        {
            files.AddRange(await GetItems(await result.NextPageRequest.GetAsync(), siteName, showHiddenFolders));
        }

        return files;
    }

    /// <summary>
    /// 根据名称返回siteid
    /// </summary>
    /// <returns></returns>
    private string GetSiteId(string siteName)
    {
        var site = driveContext.Sites.SingleOrDefault(s => s.Name == siteName);
        return site?.SiteId;
    }

    private string ReplaceCDNUrls(string downloadUrl)
    {
        if (OneDriveConfiguration.CDNUrls.Length > 0)
        {
            return OneDriveConfiguration.CDNUrls.Select(item => item.Split(";", StringSplitOptions.RemoveEmptyEntries)).Where(strings => strings.Length > 1).Aggregate(downloadUrl, (current, strings) => current.Replace(strings[0], strings[1..][DateTime.Now.Second % (strings.Length - 1)]));
        }

        return downloadUrl;
    }

    #endregion PrivateMethod
}
