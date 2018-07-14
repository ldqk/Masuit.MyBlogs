using System.Web.Mvc;
using System.Web.Routing;

namespace Masuit.MyBlogs.WebApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{resource}.ashx");
            routes.MapMvcAttributeRoutes();
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "page2",
                url: "{controller}/{action}/{page}/{size}/{orderby}",
                defaults: new { controller = "Home", action = "Post", page = UrlParameter.Optional, size = UrlParameter.Optional, orderby = UrlParameter.Optional }
            );
        }
    }
}

