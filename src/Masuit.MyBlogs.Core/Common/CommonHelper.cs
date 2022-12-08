using AngleSharp;
using AngleSharp.Css.Dom;
using AngleSharp.Dom;
using AutoMapper;
using Dispose.Scope;
using FreeRedis;
using Hangfire;
using Masuit.MyBlogs.Core.Common.Mails;
using Masuit.Tools.Media;
using Masuit.Tools.Models;
using MaxMind.GeoIP2;
using MaxMind.GeoIP2.Responses;
using Polly;
using SixLabors.ImageSharp;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TimeZoneConverter;
using ArgumentException = System.ArgumentException;
using Configuration = AngleSharp.Configuration;
using Location = MaxMind.GeoIP2.Model.Location;

namespace Masuit.MyBlogs.Core.Common
{
	/// <summary>
	/// 公共类库
	/// </summary>
	public static class CommonHelper
	{
		private static readonly FileSystemWatcher FileSystemWatcher = new(AppContext.BaseDirectory + "App_Data", "*.txt")
		{
			IncludeSubdirectories = true,
			EnableRaisingEvents = true,
			NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size
		};

		static CommonHelper()
		{
			Init();
			FileSystemWatcher.Changed += (_, _) => Init();
		}

		private static void Init()
		{
			string ReadFile(string s)
			{
				return Policy<string>.Handle<IOException>().WaitAndRetry(5, i => TimeSpan.FromSeconds(i)).Execute(() =>
				{
					using var fs = File.Open(s, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
					using var sr = new StreamReader(fs, Encoding.UTF8);
					return sr.ReadToEnd();
				});
			}

			BanRegex = ReadFile(Path.Combine(AppContext.BaseDirectory + "App_Data", "ban.txt"));
			ModRegex = ReadFile(Path.Combine(AppContext.BaseDirectory + "App_Data", "mod.txt"));
			DenyIP = ReadFile(Path.Combine(AppContext.BaseDirectory + "App_Data", "denyip.txt"));
			var lines = File.Open(Path.Combine(AppContext.BaseDirectory + "App_Data", "DenyIPRange.txt"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite).ReadAllLines(Encoding.UTF8);
			DenyIPRange = new Dictionary<string, string>();
			foreach (var line in lines)
			{
				try
				{
					var strs = line.Split(' ');
					DenyIPRange[strs[0]] = strs[1];
				}
				catch (IndexOutOfRangeException)
				{
				}
			}

			IPWhiteList = ReadFile(Path.Combine(AppContext.BaseDirectory + "App_Data", "whitelist.txt")).Split(',', '，').ToList();
		}

		/// <summary>
		/// 敏感词
		/// </summary>
		public static string BanRegex { get; set; }

		/// <summary>
		/// 审核词
		/// </summary>
		public static string ModRegex { get; set; }

		/// <summary>
		/// 全局禁止IP
		/// </summary>
		public static string DenyIP { get; set; }

		/// <summary>
		/// ip白名单
		/// </summary>
		public static List<string> IPWhiteList { get; set; }

		/// <summary>
		/// 每IP错误的次数统计
		/// </summary>
		public static NullableConcurrentDictionary<string, int> IPErrorTimes { get; set; } = new();

		/// <summary>
		/// 系统设定
		/// </summary>
		public static NullableConcurrentDictionary<string, string> SystemSettings { get; set; } = new();

		/// <summary>
		/// IP黑名单地址段
		/// </summary>
		public static Dictionary<string, string> DenyIPRange { get; set; }

		/// <summary>
		/// 判断IP地址是否被黑名单
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public static bool IsDenyIpAddress(this string ip)
		{
			if (IPWhiteList.Contains(ip))
			{
				return false;
			}

			return DenyIP.Contains(ip) || DenyIPRange.AsParallel().Any(kv => kv.Key.StartsWith(ip.Split('.')[0]) && ip.IpAddressInRange(kv.Key, kv.Value));
		}

		private static readonly QQWrySearcher IPSearcher = new(Path.Combine(AppContext.BaseDirectory + "App_Data", "qqwry.dat"));
		public static readonly DatabaseReader MaxmindReader = new(Path.Combine(AppContext.BaseDirectory + "App_Data", "GeoLite2-City.mmdb"));
		private static readonly DatabaseReader MaxmindAsnReader = new(Path.Combine(AppContext.BaseDirectory + "App_Data", "GeoLite2-ASN.mmdb"));
		private static readonly DatabaseReader MaxmindCountryReader = new(Path.Combine(AppContext.BaseDirectory + "App_Data", "GeoLite2-Country.mmdb"));

		/// <summary>
		/// 是否是代理ip
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public static async Task<bool> IsProxy(this IPAddress ip, CancellationToken cancellationToken = default)
		{
			using var serviceScope = Startup.ServiceProvider.CreateScope();
			var httpClient = serviceScope.ServiceProvider.GetRequiredService<IHttpClientFactory>().CreateClient();
			httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36 Edg/92.0.902.62");
			return await httpClient.GetStringAsync("https://ipinfo.io/" + ip, cancellationToken).ContinueWith(t =>
			{
				if (t.IsCompletedSuccessfully)
				{
					var ctx = BrowsingContext.New(Configuration.Default);
					var doc = ctx.OpenAsync(res => res.Content(t.Result)).Result;
					var isAnycast = doc.DocumentElement.QuerySelectorAll(".title").Where(e => e.TextContent.Contains("Anycast")).Select(e => e.Parent).Any(n => n.TextContent.Contains("True"));
					var isproxy = doc.DocumentElement.QuerySelectorAll("#block-privacy img").Any(e => e.OuterHtml.Contains("right"));
					return isAnycast || isproxy;
				}
				return false;
			});
		}

		public static AsnResponse GetIPAsn(this IPAddress ip)
		{
			if (ip.IsPrivateIP())
			{
				return new AsnResponse();
			}

			return MaxmindAsnReader.TryAsn(ip, out var result) ? result : new AsnResponse();
		}

		private static CityResponse GetCityResp(IPAddress ip)
		{
			return MaxmindReader.TryCity(ip, out var result) ? result : new CityResponse();
		}

		private static CountryResponse GetCountryResp(IPAddress ip)
		{
			return MaxmindCountryReader.TryCountry(ip, out var result) ? result : new CountryResponse();
		}

		public static IPLocation GetIPLocation(this string ip)
		{
			var b = IPAddress.TryParse(ip, out var ipAddress);
			if (b)
			{
				return GetIPLocation(ipAddress);
			}

			throw new ArgumentException("不能将" + ip + "转换成IP地址");
		}

		public static IPLocation GetIPLocation(this IPAddress ip)
		{
			if (ip.IsPrivateIP())
			{
				return new IPLocation("内网", null, "内网IP", null);
			}

			var cityTask = Task.Run(() => GetCityResp(ip));
			var asnTask = Task.Run(() => GetIPAsn(ip));
			var countryTask = Task.Run(() => GetCountryResp(ip));
			Task.WaitAll(cityTask, countryTask, asnTask);
			var city = cityTask.Result;
			var country = countryTask.Result;
			var asn = asnTask.Result;
			var countryName = country.Country.Names.GetValueOrDefault("zh-CN") ?? country.Country.Name;
			var cityName = city.City.Names.GetValueOrDefault("zh-CN") ?? city.City.Name;
			var continent = country.Continent.Names.GetValueOrDefault("zh-CN") ?? country.Continent.Name;
			switch (ip.AddressFamily)
			{
				case AddressFamily.InterNetworkV6 when ip.IsIPv4MappedToIPv6:
					ip = ip.MapToIPv4();
					goto case AddressFamily.InterNetwork;
				case AddressFamily.InterNetwork:
					var location = IPSearcher.GetIpLocation(ip);
					var network = location.Network + "/" + asn.AutonomousSystemOrganization;
					return new IPLocation(countryName, location.City, network.Trim('/'), asn.AutonomousSystemNumber)
					{
						Address2 = new IPLocation.CountryCity(continent, countryName, cityName),
						Coodinate = city.Location,
						Continent = continent
					};
				default:
					return new IPLocation(countryName, cityName, asn.AutonomousSystemOrganization, asn.AutonomousSystemNumber)
					{
						Coodinate = city.Location,
						Continent = continent
					};
			}
		}

		/// <summary>
		/// 获取ip所在时区
		/// </summary>
		/// <param name="ip"></param>
		/// <returns></returns>
		public static string GetClientTimeZone(this IPAddress ip)
		{
			if (ip.IsPrivateIP())
			{
				return "Asia/Shanghai";
			}

			return GetCityResp(ip).Location.TimeZone ?? "Asia/Shanghai";
		}

		/// <summary>
		/// 类型映射
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <returns></returns>
		public static T Mapper<T>(this object source) where T : class => Startup.ServiceProvider.GetRequiredService<IMapper>().Map<T>(source);

		/// <summary>
		/// 发送邮件
		/// </summary>
		/// <param name="title">标题</param>
		/// <param name="content">内容</param>
		/// <param name="tos">收件人</param>
		/// <param name="clientip"></param>
		[AutomaticRetry(Attempts = 1, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
		public static void SendMail(string title, string content, string tos, string clientip)
		{
			using var serviceScope = Startup.ServiceProvider.CreateScope();
			serviceScope.ServiceProvider.GetRequiredService<IMailSender>().Send(title, content, tos);
			var redisClient = serviceScope.ServiceProvider.GetRequiredService<IRedisClient>();
			redisClient.SAdd($"Email:{DateTime.Now:yyyyMMdd}", new { title, content, tos, time = DateTime.Now, clientip });
			redisClient.Expire($"Email:{DateTime.Now:yyyyMMdd}", 86400);
		}

		/// <summary>
		/// 清理html的img标签的除src之外的其他属性
		/// </summary>
		/// <param name="html"></param>
		/// <returns></returns>
		public static async Task<string> ClearImgAttributes(this string html)
		{
			var context = BrowsingContext.New(Configuration.Default);
			var doc = await context.OpenAsync(req => req.Content(html));
			var nodes = doc.DocumentElement.GetElementsByTagName("img");
			var allows = new[] { "src", "data-original", "width", "style", "class" };
			foreach (var node in nodes)
			{
				for (var i = 0; i < node.Attributes.Length; i++)
				{
					if (allows.Contains(node.Attributes[i].Name))
					{
						continue;
					}

					node.RemoveAttribute(node.Attributes[i].Name);
				}
			}

			return doc.Body.InnerHtml;
		}

		/// <summary>
		/// 将html的img标签懒加载
		/// </summary>
		/// <param name="html"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		public static async Task<string> ReplaceImgAttribute(this string html, string title)
		{
			var context = BrowsingContext.New(Configuration.Default);
			var doc = await context.OpenAsync(req => req.Content(html));
			var nodes = doc.DocumentElement.GetElementsByTagName("img");
			foreach (var node in nodes)
			{
				if (node.HasAttribute("src"))
				{
					string src = node.Attributes["src"].Value;
					node.RemoveAttribute("src");
					node.SetAttribute("data-src", src);
					node.SetAttribute("decoding", "async");
					node.SetAttribute("class", node.Attributes["class"]?.Value + " lazyload");
					node.SetAttribute("loading", "lazy");
					node.SetAttribute("alt", SystemSettings["Title"]);
					node.SetAttribute("title", title);
				}
			}

			var elements = doc.DocumentElement.QuerySelectorAll("p,br");
			var els = elements.OrderByRandom().Take(Math.Max(elements.Length / 5, 3)).ToPooledListScope();
			var href = "https://" + SystemSettings["Domain"].Split('|').OrderByRandom().FirstOrDefault();
			foreach (var el in els)
			{
				var a = doc.CreateElement("a");
				a.SetAttribute("href", href);
				a.SetAttribute("target", "_blank");
				a.SetAttribute("title", SystemSettings["Title"]);
				a.SetStyle("position: absolute;color: transparent;z-index: -1");
				a.TextContent = SystemSettings["Title"] + SystemSettings["Domain"];
				el.InsertAfter(a);
				var a2 = doc.CreateElement("a");
				a2.SetAttribute("href", "/craw/" + SnowFlake.NewId);
				a2.SetStyle("position: absolute;color: transparent;z-index: -1");
				a2.TextContent = title;
				a.InsertAfter(a2);
			}

			return doc.Body.InnerHtml;
		}

		/// <summary>
		/// html添加指纹信息
		/// </summary>
		public static async Task<string> InjectFingerprint(this string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return html;
			}

			try
			{
				var context = BrowsingContext.New(Configuration.Default);
				var document = await context.OpenAsync(req => req.Content(html));
				var elements = document.DocumentElement.GetElementsByTagName("p").SelectMany(e => e.ChildNodes).Flatten(n => n.ChildNodes).OfType<IText>().OrderByRandom().Take(5);
				foreach (var e in elements)
				{
					e.TextContent = e.TextContent.InjectZeroWidthString(SystemSettings["Title"] + SystemSettings["Domain"]);
				}
				return document.Body.InnerHtml;
			}
			catch
			{
				return html;
			}
		}

		/// <summary>
		/// 获取文章摘要
		/// </summary>
		/// <param name="html"></param>
		/// <param name="length">截取长度</param>
		/// <param name="min">摘要最少字数</param>
		/// <returns></returns>
		public static Task<string> GetSummary(this string html, int length = 150, int min = 10)
		{
			var context = BrowsingContext.New(Configuration.Default);
			return context.OpenAsync(req => req.Content(html)).ContinueWith(t =>
			{
				var summary = t.Result.DocumentElement.GetElementsByTagName("p").FirstOrDefault(n => n.TextContent.Length > min)?.TextContent ?? "没有摘要";
				return summary.Length > length ? summary[..length] + "..." : summary;
			});
		}

		public static string TrimQuery(this string path)
		{
			return path.Split('&').Where(s => s.Split('=', StringSplitOptions.RemoveEmptyEntries).Length == 2).Join("&");
		}

		/// <summary>
		/// 添加水印
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static Stream AddWatermark(this Stream stream)
		{
			if (!string.IsNullOrEmpty(SystemSettings.GetOrAdd("Watermark", string.Empty)))
			{
				try
				{
					var watermarker = new ImageWatermarker(stream)
					{
						SkipWatermarkForSmallImages = true,
						SmallImagePixelsThreshold = 90000
					};
					var watermarkText = SystemSettings["Watermark"];
					var position = Enum.Parse<WatermarkPosition>(SystemSettings["WatermarkPosition"] ?? "3");
					return watermarker.AddWatermark(watermarkText, AppContext.BaseDirectory + "App_Data/华康勘亭流.ttf", 20, Color.LightGray, position, 30);
				}
				catch
				{
					//
				}
			}
			return stream;
		}

		/// <summary>
		/// 转换时区
		/// </summary>
		/// <param name="time">UTC时间</param>
		/// <param name="zone">时区id</param>
		/// <returns></returns>
		public static DateTime ToTimeZone(this in DateTime time, string zone)
		{
			return TimeZoneInfo.ConvertTime(time, TZConvert.GetTimeZoneInfo(zone));
		}

		/// <summary>
		/// 转换时区
		/// </summary>
		/// <param name="time">UTC时间</param>
		/// <param name="zone">时区id</param>
		/// <param name="format">时间格式字符串</param>
		/// <returns></returns>
		public static string ToTimeZoneF(this in DateTime time, string zone, string format = "yyyy-MM-dd HH:mm:ss")
		{
			return ToTimeZone(time, zone).ToString(format);
		}
	}

	public class IPLocation
	{
		public IPLocation(string country, string city, string isp, long? asn)
		{
			Country = country?.Trim('0');
			City = city?.Trim('0');
			ISP = isp;
			ASN = asn;
		}

		public string Continent { get; set; }

		public string Country { get; set; }

		public string City { get; set; }

		public string ISP { get; set; }

		public long? ASN { get; set; }

		public string Address => new[] { Continent, Country, City }.Where(s => !string.IsNullOrEmpty(s)).Distinct().Join("");

		public CountryCity Address2 { get; set; } = new("", "", "");

		public string Network => ASN.HasValue ? ISP + "(AS" + ASN + ")" : ISP;

		public Location Coodinate { get; set; }

		public override string ToString()
		{
			string address = Address;
			string network = Network;
			if (string.IsNullOrWhiteSpace(address))
			{
				address = "未知地区";
			}

			if (string.IsNullOrWhiteSpace(network))
			{
				network = "未知网络";
			}

			return new[] { address, Address2?.ToString(), network }.Where(s => !string.IsNullOrEmpty(s)).Distinct().Join("|");
		}

		public static implicit operator string(IPLocation entry)
		{
			return entry.ToString();
		}

		public void Deconstruct(out string location, out string network, out string info)
		{
			location = new[] { Address, Address2 }.Where(s => !string.IsNullOrEmpty(s)).Distinct().Join("|");
			network = Network;
			info = ToString();
		}

		public bool Contains(string s)
		{
			return ToString().Contains(s, StringComparison.CurrentCultureIgnoreCase);
		}

		public bool Contains(params string[] s)
		{
			return ToString().Contains(s);
		}

		public record CountryCity(string Continent, string Country, string City)
		{
			public void Deconstruct(out string continent, out string country, out string city)
			{
				country = Country;
				city = City;
				continent = Continent;
			}

			public static implicit operator string(CountryCity entry)
			{
				return entry.ToString();
			}

			public override string ToString()
			{
				return Continent + Country + City;
			}
		}
	}
}
