using Masuit.MyBlogs.Core.Configs;
using Masuit.Tools;
using MaxMind.GeoIP2.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Masuit.MyBlogs.Core.Common
{
    public static class HttpContextExtension
    {
        /// <summary>
        /// 地理位置信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string Location(this HttpRequest request)
        {
            return (string)request.HttpContext.Items.GetOrAdd("ip.location", () => request.HttpContext.GetTrueIP().GetIPLocation());
        }

        /// <summary>
        /// asn信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static AsnResponse Asn(this HttpRequest request)
        {
            return (AsnResponse)request.HttpContext.Items.GetOrAdd("ip.asn", () => request.HttpContext.GetTrueIP().GetIPAsn());
        }

        /// <summary>
        /// 获取真实客户端ip
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetTrueIP(this HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress.ToString();
            var trueip = context.Request.Headers[AppConfig.TrueClientIPHeader].ToString();
            if (!string.IsNullOrEmpty(trueip) && ip != trueip)
            {
                ip = trueip;
            }

            return ip;
        }

        /// <summary>
        /// 是否是机器人访问
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public static bool IsRobot(this HttpRequest req) => UserAgent.Parse(req.Headers[HeaderNames.UserAgent].ToString()).IsRobot;
    }
}