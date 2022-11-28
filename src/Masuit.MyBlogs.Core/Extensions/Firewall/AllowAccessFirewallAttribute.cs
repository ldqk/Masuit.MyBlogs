using Microsoft.AspNetCore.Mvc.Filters;

namespace Masuit.MyBlogs.Core.Extensions.Firewall;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AllowAccessFirewallAttribute : Attribute, IFilterFactory, IOrderedFilter
{
	public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
	{
		return new AllowAccessFirewallAttribute();
	}

	public bool IsReusable => true;

	public int Order { get; }
}