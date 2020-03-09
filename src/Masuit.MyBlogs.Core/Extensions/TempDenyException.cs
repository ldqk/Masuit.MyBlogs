using System;

namespace Masuit.MyBlogs.Core.Extensions
{
    public class TempDenyException : Exception
    {
        public TempDenyException(string msg) : base(msg)
        {
        }
    }
}