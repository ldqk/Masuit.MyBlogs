using Microsoft.AspNetCore.Http;

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
        public override string Process()
        {
            return WriteJson(UeditorConfig.Items);
        }
    }
}