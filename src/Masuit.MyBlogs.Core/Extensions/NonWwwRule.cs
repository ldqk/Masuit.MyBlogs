using Microsoft.AspNetCore.Rewrite;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class NonWwwRule : IRule
    {
        public void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;
            var currentHost = req.Host;
            if (currentHost.Host.Equals("127.0.0.1") || currentHost.Host.Equals("localhost"))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            if (req.Scheme.Equals("http") || currentHost.Host.StartsWith("www."))
            {
                context.HttpContext.Response.Redirect("https://" + currentHost.Value[4..] + req.PathBase + req.Path + req.QueryString, true);
                context.Result = RuleResult.EndResponse;
            }
        }
    }
}
