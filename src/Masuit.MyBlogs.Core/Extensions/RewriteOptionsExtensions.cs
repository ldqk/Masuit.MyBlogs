using Microsoft.AspNetCore.Rewrite;

namespace Masuit.MyBlogs.Core.Extensions
{
    public static class RewriteOptionsExtensions
    {
        public static RewriteOptions AddRedirectToNonWww(this RewriteOptions options)
        {
            options.Rules.Add(new NonWwwRule());
            return options;
        }
    }
}