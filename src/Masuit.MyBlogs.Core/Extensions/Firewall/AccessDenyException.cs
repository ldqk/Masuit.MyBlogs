namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public class AccessDenyException : Exception
{
	public AccessDenyException(string msg) : base(msg)
	{
	}
}