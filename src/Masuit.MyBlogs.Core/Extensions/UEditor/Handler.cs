using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Masuit.MyBlogs.Core.Extensions.UEditor
{
    /// <summary>
    /// Handler 的摘要说明
    /// </summary>
    public abstract class Handler
    {
        protected Handler(HttpContext context)
        {
            this.Request = context.Request;
            this.Response = context.Response;
            this.Context = context;
            //this.Server = context.Server;
        }

        public abstract string Process();

        protected string WriteJson(object response)
        {
            string jsonpCallback = Request.Query["callback"];
            string json = JsonConvert.SerializeObject(response);
            return string.IsNullOrWhiteSpace(jsonpCallback) ? json : $"{jsonpCallback}({json});";
        }

        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }
        public HttpContext Context { get; private set; }
        //public HttpServerUtility Server { get; private set; }
    }
}