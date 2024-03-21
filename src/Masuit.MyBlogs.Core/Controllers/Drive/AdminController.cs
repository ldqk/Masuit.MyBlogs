using Masuit.MyBlogs.Core.Extensions;
using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Infrastructure.Drive;
using Masuit.MyBlogs.Core.Models.Drive;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Masuit.MyBlogs.Core.Controllers.Drive;

[MyAuthorize]
[ApiController]
[Route("api/[controller]")]
public sealed class AdminController(IDriveAccountService driveAccount, SettingService setting, TokenService tokenService) : Controller
{
    /// <summary>
    /// 重定向到 M$ 的 Oauth
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("bind/url")]
    public async Task<RedirectResult> RedirectToBinding()
    {
        string url = await driveAccount.GetAuthorizationRequestUrl();
        var result = new RedirectResult(url);
        return result;
    }

    /// <summary>
    /// 从 Oauth 重定向的url
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpGet("bind/new")]
    public async Task<IActionResult> NewBinding(string code)
    {
        try
        {
            var result = await tokenService.Authorize(code);
            if (result.AccessToken != null)
            {
                await setting.Set("AccountStatus", "已认证");
                return Redirect("/#/admin");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse()
            {
                message = ex.Message
            });
        }
        return StatusCode(500, new ErrorResponse()
        {
            message = "未知错误"
        });
    }

    /// <summary>
    /// 添加 SharePoint Site
    /// </summary>
    /// <returns></returns>
    [HttpPost("sites")]
    public async Task<IActionResult> AddSite(AddSiteModel model)
    {
        try
        {
            await driveAccount.AddSiteId(model.siteName, model.nickName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse()
            {
                message = ex.Message
            });
        }
        return StatusCode(201);
    }

    /// <summary>
    /// 获取基本内容
    /// </summary>
    /// <returns></returns>
    [HttpGet("info")]
    public async Task<IActionResult> GetInfo()
    {
        try
        {
            var driveInfo = new List<DriveAccountService.DriveInfo>();
            if (setting.Get("AccountStatus") == "已认证")
            {
                driveInfo = await driveAccount.GetDriveInfo();
            }
            return Json(new
            {
                officeName = OneDriveConfiguration.AccountName,
                officeType = Enum.GetName(typeof(OneDriveConfiguration.OfficeType), OneDriveConfiguration.Type),
                driveInfo,
                appName = setting.Get("AppName"),
                webName = setting.Get("WebName"),
                defaultDrive = setting.Get("DefaultDrive"),
                accountStatus = setting.Get("AccountStatus"),
                readme = setting.Get("Readme"),
                footer = setting.Get("Footer"),
                allowAnonymouslyUpload = !string.IsNullOrEmpty(setting.Get("AllowAnonymouslyUpload")) && Convert.ToBoolean(setting.Get("AllowAnonymouslyUpload")),
                uploadPassword = setting.Get("UploadPassword"),
            }, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse()
            {
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// 设置readme
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("readme")]
    public async Task<IActionResult> UpdateReadme(ReadmeModel model)
    {
        try
        {
            await setting.Set("Readme", model.text);
        }
        catch (Exception e)
        {
            return StatusCode(500, new ErrorResponse()
            {
                message = e.Message
            });
        }
        return StatusCode(204);
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    [HttpPost("settings")]
    public async Task<IActionResult> UpdateSetting(UpdateSettings settings)
    {
        try
        {
            await setting.Set("AppName", settings.appName);
            await setting.Set("WebName", settings.webName);
            await setting.Set("DefaultDrive", settings.defaultDrive);
            await setting.Set("Footer", settings.footer);
            await setting.Set("UploadPassword", settings.uploadPassword);
            await setting.Set("AllowAnonymouslyUpload", settings.allowAnonymouslyUpload.ToString());
        }
        catch (Exception e)
        {
            return StatusCode(500, new ErrorResponse()
            {
                message = e.Message
            });
        }
        return StatusCode(204);
    }

    /// <summary>
    /// 解除绑定
    /// </summary>
    /// <param name="nickName"></param>
    /// <returns></returns>
    [HttpDelete("sites")]
    public async Task<IActionResult> Unbind(string nickName)
    {
        try
        {
            await driveAccount.Unbind(nickName);
        }
        catch (Exception e)
        {
            return StatusCode(500, new ErrorResponse()
            {
                message = e.Message
            });
        }
        return StatusCode(204);
    }

    /// <summary>
    /// 更新站点设置
    /// </summary>
    /// <returns></returns>
    [HttpPost("sites/settings")]
    public async Task<IActionResult> UpdateSiteSettings(SiteSettingsModel model)
    {
        try
        {
            var site = driveAccount.SiteContext.Sites.SingleOrDefault(site => site.Name == model.siteName);
            if (site != null)
            {
                site.NickName = model.nickName;
                site.HiddenFolders = model.hiddenFolders.Split(',');
                driveAccount.SiteContext.Sites.Update(site);
                await driveAccount.SiteContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ErrorResponse()
            {
                message = ex.Message
            });
        }
        return StatusCode(204);
    }

    #region 接收表单模型

    public class UpdateSettings
    {
        public string appName { get; set; }

        public string webName { get; set; }

        public string navImg { get; set; }

        public string defaultDrive { get; set; }

        public string readme { get; set; }

        public string footer { get; set; }

        public bool allowAnonymouslyUpload { get; set; }

        public string uploadPassword { get; set; }
    }

    public class AddSiteModel
    {
        public string siteName { get; set; }

        public string nickName { get; set; }
    }

    public class SiteSettingsModel
    {
        public string siteName { get; set; }

        public string nickName { get; set; }

        public string hiddenFolders { get; set; }
    }

    public class ReadmeModel
    {
        public string text { get; set; }
    }

    #endregion 接收表单模型
}