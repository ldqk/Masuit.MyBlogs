using Masuit.Tools.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;

namespace Masuit.MyBlogs.Core.Extensions.UEditor;

/// <summary>
/// UploadHandler 的摘要说明
/// </summary>
public class UploadHandler(HttpContext context, UploadConfig config) : Handler(context)
{
    public UploadConfig UploadConfig { get; } = config;

    public UploadResult Result { get; } = new()
    {
        State = UploadState.Unknown
    };

    public override async Task<string> Process()
    {
        var form = await Request.ReadFormAsync();
        foreach (var file in form.Files)
        {
            var uploadFileName = file.FileName;
            if (!CheckFileType(uploadFileName))
            {
                Result.State = UploadState.TypeNotAllow;
                return WriteResult();
            }
            if (!CheckFileSize(file.Length))
            {
                Result.State = UploadState.SizeLimitExceed;
                return WriteResult();
            }

            Result.OriginFileName = uploadFileName;
            var savePath = PathFormatter.Format(uploadFileName, UploadConfig.PathFormat);
            var cts = new CancellationTokenSource(20000);
            var stream = file.OpenReadStream();
            try
            {
                var stream2 = stream.AddWatermark();
                var format = await Image.DetectFormatAsync(stream2, cts.Token).ContinueWith(t => t.IsCompletedSuccessfully ? t.Result : null);
                stream2.Position = 0;
                if (format != null && !Regex.IsMatch(format.Name, "JPEG|PNG|Webp|GIF", RegexOptions.IgnoreCase))
                {
                    using var image = await Image.LoadAsync(stream2);
                    var memoryStream = new PooledMemoryStream();
                    await image.SaveAsJpegAsync(memoryStream);
                    await stream2.DisposeAsync();
                    stream2 = memoryStream;
                    savePath = savePath.Replace(Path.GetExtension(savePath), ".jpg");
                }

                var localPath = AppContext.BaseDirectory + "wwwroot" + savePath;
                var (url, success) = await Context.RequestServices.GetRequiredService<ImagebedClient>().UploadImage(stream2, localPath, cts.Token);
                if (success)
                {
                    Result.Url = url;
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                    await File.WriteAllBytesAsync(localPath, await stream2.ToArrayAsync());
                    Result.Url = savePath;
                }

                Result.State = UploadState.Success;
            }
            catch (Exception e)
            {
                Result.State = UploadState.FileAccessError;
                Result.ErrorMessage = e.Message;
                LogManager.Error(e.Demystify());
            }
            finally
            {
                cts.Dispose();
                stream.Close();
                await stream.DisposeAsync();
            }
        }

        return WriteResult();
    }

    private string WriteResult()
    {
        return WriteJson(new
        {
            state = GetStateMessage(Result.State),
            url = Result.Url,
            title = Result.OriginFileName,
            original = Result.OriginFileName,
            error = Result.ErrorMessage
        });
    }

    private static string GetStateMessage(UploadState state)
    {
        switch (state)
        {
            case UploadState.Success:
                return "SUCCESS";

            case UploadState.FileAccessError:
                return "文件访问出错，请检查写入权限";

            case UploadState.SizeLimitExceed:
                return "文件大小超出服务器限制";

            case UploadState.TypeNotAllow:
                return "不允许的文件格式";

            case UploadState.NetworkError:
                return "网络错误";
        }
        return "未知错误";
    }

    private bool CheckFileType(string filename)
    {
        return UploadConfig.AllowExtensions.Any(x => x.Equals(Path.GetExtension(filename), StringComparison.CurrentCultureIgnoreCase));
    }

    private bool CheckFileSize(long size)
    {
        return size < UploadConfig.SizeLimit;
    }
}

public class UploadConfig
{
    /// <summary>
    /// 文件命名规则
    /// </summary>
    public string PathFormat { get; set; }

    /// <summary>
    /// 上传表单域名称
    /// </summary>
    public string UploadFieldName { get; set; }

    /// <summary>
    /// 上传大小限制
    /// </summary>
    public int SizeLimit { get; set; }

    /// <summary>
    /// 上传允许的文件格式
    /// </summary>
    public string[] AllowExtensions { get; set; }

    /// <summary>
    /// 文件是否以 Base64 的形式上传
    /// </summary>
    public bool Base64 { get; set; }

    /// <summary>
    /// Base64 字符串所表示的文件名
    /// </summary>
    public string Base64Filename { get; set; }
}

public class UploadResult
{
    public UploadState State { get; set; }

    public string Url { get; set; }

    public string OriginFileName { get; set; }

    public string ErrorMessage { get; set; }
}

public enum UploadState
{
    NetworkError = -4,
    FileAccessError = -3,
    TypeNotAllow = -2,
    SizeLimitExceed = -1,
    Success = 0,
    Unknown = 1
}