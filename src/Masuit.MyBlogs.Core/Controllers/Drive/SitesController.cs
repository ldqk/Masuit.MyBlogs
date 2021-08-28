using Masuit.MyBlogs.Core.Extensions.DriveHelpers;
using Masuit.MyBlogs.Core.Infrastructure.Drive;
using Masuit.MyBlogs.Core.Models.Drive;
using Masuit.MyBlogs.Core.Models.DTO;
using Masuit.MyBlogs.Core.Models.ViewModel;
using Masuit.Tools.Core.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;
using Masuit.MyBlogs.Core.Extensions.Firewall;

namespace Masuit.MyBlogs.Core.Controllers.Drive
{
    [ApiController]
    [ServiceFilter(typeof(FirewallAttribute))]
    [Route("api/")]
    public class SitesController : Controller
    {
        readonly IDriveAccountService _siteService;
        readonly IDriveService _driveService;
        readonly SettingService _setting;

        public UserInfoDto CurrentUser => HttpContext.Session.Get<UserInfoDto>(SessionKey.UserInfo) ?? new UserInfoDto();

        public SitesController(IDriveAccountService siteService, IDriveService driveService, SettingService setting)
        {
            this._siteService = siteService;
            this._driveService = driveService;
            this._setting = setting;
        }


        #region Actions
        /// <summary>
        /// 返回所有sites
        /// </summary>
        /// <returns></returns>
        [HttpGet("sites"), ResponseCache(Duration = 600)]
        public IActionResult GetSites()
        {
            return Json(_siteService.GetSites(), new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
        /// <summary>
        /// 根据路径获取文件夹内容
        /// </summary>
        /// <returns></returns>
        [HttpGet("sites/{siteName}/{**path}"), ResponseCache(Duration = 600)]
        public async Task<IActionResult> GetDirectory(string siteName, string path)
        {
            if (string.IsNullOrEmpty(siteName))
            {
                return NotFound(new ErrorResponse()
                {
                    message = "找不到请求的 Site Name"
                });
            }
            if (string.IsNullOrEmpty(path))
            {
                try
                {
                    var result = await _driveService.GetRootItems(siteName, CurrentUser.IsAdmin);
                    return Json(result, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                }
                catch (Exception e)
                {
                    return StatusCode(500, e.Message);
                }
            }
            else
            {
                try
                {
                    var result = await _driveService.GetDriveItemsByPath(path, siteName, CurrentUser.IsAdmin);
                    if (result == null)
                    {
                        return NotFound(new ErrorResponse()
                        {
                            message = $"路径{path}不存在"
                        });
                    }
                    return Json(result, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                }
                catch (Exception e)
                {
                    return NotFound(new ErrorResponse()
                    {
                        message = $"路径{path}不存在"
                    });
                }
            }
        }
        // catch-all 参数匹配路径
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        [HttpGet("files/{siteName}/{**path}"), ResponseCache(Duration = 600)]
        public async Task<IActionResult> Download(string siteName, string path)
        {
            try
            {
                var result = await _driveService.GetDriveItemByPath(path, siteName);
                if (result != null)
                {
                    return Redirect(result.DownloadUrl);
                }

                return NotFound(new ErrorResponse()
                {
                    message = $"所求的{path}不存在"
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// 获取基本信息
        /// </summary>
        /// <returns></returns>
        [HttpGet("info"), ResponseCache(Duration = 600)]
        public IActionResult GetInfo()
        {
            bool isAollowAnonymous = !string.IsNullOrEmpty(_setting.Get("AllowAnonymouslyUpload")) && Convert.ToBoolean(_setting.Get("AllowAnonymouslyUpload"));
            return Json(new
            {
                appName = _setting.Get("AppName"),
                webName = _setting.Get("WebName"),
                defaultDrive = _setting.Get("DefaultDrive"),
                readme = _setting.Get("Readme"),
                footer = _setting.Get("Footer"),
                allowUpload = isAollowAnonymous
            }, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
        /// <summary>
        /// 获得readme
        /// </summary>
        /// <returns></returns>
        [HttpGet("readme"), ResponseCache(Duration = 600)]
        public IActionResult GetReadme()
        {
            return Json(new
            {
                readme = _setting.Get("Readme")
            }, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }

        /// <summary>
        /// 获取文件分片上传路径
        /// </summary>
        /// <returns></returns>
        [HttpGet("upload/{siteName}/{**fileName}"), ResponseCache(Duration = 600)]
        public async Task<IActionResult> GetUploadUrl(string siteName, string fileName)
        {
            bool isAollowAnonymous = !string.IsNullOrEmpty(_setting.Get("AllowAnonymouslyUpload")) && Convert.ToBoolean(_setting.Get("AllowAnonymouslyUpload"));
            if (!isAollowAnonymous)
            {
                if (Request.Headers.ContainsKey("Authorization"))
                {
                    if (!CurrentUser.IsAdmin)
                    {
                        return Unauthorized(new ErrorResponse()
                        {
                            message = "未经授权的访问"
                        });
                    }
                }
                else
                {
                    return Unauthorized(new ErrorResponse()
                    {
                        message = "未经授权的访问"
                    });
                }
            }
            string path = Path.Combine($"upload/{Guid.NewGuid()}", fileName);
            try
            {
                var result = await _driveService.GetUploadUrl(path, siteName);
                return Json(new
                {
                    requestUrl = result,
                    fileUrl = $"{OneDriveConfiguration.BaseUri}/api/files/{siteName}/{path}"
                }, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// 获取文件分片上传路径
        /// </summary>
        /// <returns></returns>
        [HttpGet("cli/upload/{siteName}/:/{**path}")]
        public async Task<IActionResult> GetUploadUrl(string siteName, string path, string uploadPassword)
        {
            if (uploadPassword != _setting.Get("UploadPassword"))
            {
                return Unauthorized(new ErrorResponse()
                {
                    message = "上传密码错误"
                });
            }
            if (string.IsNullOrEmpty(path))
            {
                return BadRequest(new ErrorResponse()
                {
                    message = "必须存在上传路径"
                });
            }
            try
            {
                var result = await _driveService.GetUploadUrl(path, siteName);
                return Json(new
                {
                    requestUrl = result
                }, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }
        #endregion
    }
}