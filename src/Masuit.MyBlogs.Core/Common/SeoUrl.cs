namespace Masuit.MyBlogs.Core.Common;

public static class SeoUrl
{
    public static string Canonical(HttpRequest request)
    {
        var controller = request.RouteValues["controller"]?.ToString();
        var action = request.RouteValues["action"]?.ToString();
        var isPost = string.Equals(controller, "Post", StringComparison.OrdinalIgnoreCase) && string.Equals(action, "Details", StringComparison.OrdinalIgnoreCase);
        var isHome = string.Equals(controller, "Home", StringComparison.OrdinalIgnoreCase) && string.Equals(action, "Index", StringComparison.OrdinalIgnoreCase);
        var isPostList = string.Equals(controller, "Home", StringComparison.OrdinalIgnoreCase) && string.Equals(action, "Post", StringComparison.OrdinalIgnoreCase);
        var path = isPost ? "/" + request.RouteValues["id"] : isHome ? "/" : isPostList ? "/posts" : request.Path.ToUriComponent();
        var query = isPost || isHome ? QueryString.Empty : QueryString.Create(request.Query.Where(item => !item.Key.StartsWith("utm_", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("gclid", StringComparison.OrdinalIgnoreCase) && !item.Key.Equals("fbclid", StringComparison.OrdinalIgnoreCase) && !(item.Key.Equals("page", StringComparison.OrdinalIgnoreCase) && item.Value.ToString() == "1")).SelectMany(item => item.Value.Select(value => new KeyValuePair<string, string>(item.Key, value))));
        return $"{request.Scheme}://{request.Host}{request.PathBase.ToUriComponent()}{path}{query}";
    }
}