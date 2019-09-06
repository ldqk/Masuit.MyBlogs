using System;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string msg) : base(msg)
        {
        }
    }
}