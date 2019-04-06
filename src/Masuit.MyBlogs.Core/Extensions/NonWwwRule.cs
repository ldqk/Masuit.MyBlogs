using Microsoft.AspNetCore.Rewrite;
using System;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class NonWwwRule : IRule
    {
        public void ApplyRule(RewriteContext context)
        {
            var req = context.HttpContext.Request;
            var currentHost = req.Host;
            if (currentHost.Host.Equals("127.0.0.1") || currentHost.Host.Equals("localhost", StringComparison.InvariantCultureIgnoreCase))
            {
                context.Result = RuleResult.ContinueRules;
                return;
            }

            if (req.Scheme.Equals("http"))
            {
                context.HttpContext.Response.Redirect("https://" + currentHost.Host + req.PathBase + req.Path + req.QueryString);
                context.Result = RuleResult.EndResponse;
            }
        }
    }
}