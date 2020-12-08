using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Extensions.UEditor
{
    /// <summary>
    /// Config 的摘要说明
    /// </summary>
    public class ConfigHandler : Handler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public ConfigHandler(HttpContext context) : base(context)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override Task<string> Process()
        {
            return Task.FromResult(WriteJson(UeditorConfig.Items));
        }
    }
}