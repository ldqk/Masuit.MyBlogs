namespace Masuit.MyBlogs.Core.Extensions.Firewall;

public class TempDenyException : Exception
{
	public TempDenyException(string msg) : base(msg)
	{
	}
}