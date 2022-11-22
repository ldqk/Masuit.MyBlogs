using Masuit.MyBlogs.Core.Common;
using Masuit.Tools;
using Masuit.Tools.AspNetCore.Mime;
using Masuit.Tools.Logging;
using SixLabors.ImageSharp;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;

namespace Masuit.MyBlogs.Core.Extensions.UEditor
{
	/// <summary>
	/// Crawler 的摘要说明
	/// </summary>
	public class CrawlerHandler : Handler
	{
		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;

		public CrawlerHandler(HttpContext context) : base(context)
		{
			_httpClient = context.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();
			_configuration = context.RequestServices.GetRequiredService<IConfiguration>();
		}

		public override async Task<string> Process()
		{
			var form = await Request.ReadFormAsync();
			string[] sources = form["source[]"];
			if (sources?.Length > 0 || sources?.Length <= 10)
			{
				using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
				return WriteJson(new
				{
					state = "SUCCESS",
					list = (await sources.SelectAsync(s =>
					{
						return new Crawler(s, _httpClient, _configuration, Context).Fetch(cts.Token).ContinueWith(t => new
						{
							state = t.Result.State,
							source = t.Result.SourceUrl,
							url = t.Result.ServerUrl
						});
					}))
				});
			}

			return WriteJson(new
			{
				state = "参数错误：没有指定抓取源"
			});
		}
	}

	public class Crawler
	{
		public string SourceUrl { get; set; }

		public string ServerUrl { get; set; }

		public string State { get; set; }

		private readonly HttpClient _httpClient;
		private readonly IConfiguration _configuration;
		private readonly HttpContext _httpContext;
		public Crawler(string sourceUrl, HttpClient httpClient, IConfiguration configuration, HttpContext httpContext)
		{
			SourceUrl = sourceUrl;
			_httpClient = httpClient;
			_configuration = configuration;
			_httpContext = httpContext;
		}

		public async Task<Crawler> Fetch(CancellationToken token)
		{
			if (!SourceUrl.IsExternalAddress())
			{
				State = "INVALID_URL";
				return this;
			}
			try
			{
				_httpClient.DefaultRequestHeaders.Referrer = new Uri(SourceUrl);
				using var response = await _httpClient.GetAsync(_configuration["HttpClientProxy:UriPrefix"] + SourceUrl);
				if (response.StatusCode != HttpStatusCode.OK)
				{
					State = "远程地址返回了错误的状态吗：" + response.StatusCode;
					return this;
				}

				ServerUrl = PathFormatter.Format(Path.GetFileNameWithoutExtension(SourceUrl), CommonHelper.SystemSettings.GetOrAdd("UploadPath", "upload") + UeditorConfig.GetString("catcherPathFormat")) + MimeMapper.ExtTypes[response.Content.Headers.ContentType?.MediaType ?? "image/jpeg"];
				var stream = await response.Content.ReadAsStreamAsync();
				var format = await Image.DetectFormatAsync(stream).ContinueWith(t => t.IsCompletedSuccessfully ? t.Result : null);
				stream.Position = 0;
				if (format != null)
				{
					ServerUrl = ServerUrl.Replace(Path.GetExtension(ServerUrl), "." + format.Name.ToLower());
					if (!Regex.IsMatch(format.Name, "JPEG|PNG|Webp|GIF", RegexOptions.IgnoreCase))
					{
						using var image = await Image.LoadAsync(stream);
						var memoryStream = new MemoryStream();
						await image.SaveAsJpegAsync(memoryStream);
						await stream.DisposeAsync();
						stream = memoryStream;
						ServerUrl = ServerUrl.Replace(Path.GetExtension(ServerUrl), ".jpg");
					}
				}

				var savePath = AppContext.BaseDirectory + "wwwroot" + ServerUrl;
				var (url, success) = await _httpContext.RequestServices.GetRequiredService<ImagebedClient>().UploadImage(stream, savePath, token);
				if (success)
				{
					ServerUrl = url;
				}
				else
				{
					Directory.CreateDirectory(Path.GetDirectoryName(savePath));
					await File.WriteAllBytesAsync(savePath, await stream.ToArrayAsync());
				}

				await stream.DisposeAsync();
				State = "SUCCESS";
			}
			catch (Exception e)
			{
				State = "抓取错误：" + e.Message;
				LogManager.Error(e.Demystify());
			}

			return this;
		}
	}
}
