using System;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class AccessDenyException : Exception
    {
        public AccessDenyException(string msg) : base(msg)
        {
        }
    }
}