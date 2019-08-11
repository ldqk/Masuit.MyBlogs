using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Masuit.MyBlogs.Core.Extensions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowAccessFirewallAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        /// <summary>Creates an instance of the executable filter.</summary>
        /// <param name="serviceProvider">The request <see cref="T:System.IServiceProvider" />.</param>
        /// <returns>An instance of the executable filter.</returns>
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return new AllowAccessFirewallAttribute();
        }

        /// <summary>
        /// Gets a value that indicates if the result of <see cref="M:Microsoft.AspNetCore.Mvc.Filters.IFilterFactory.CreateInstance(System.IServiceProvider)" />
        /// can be reused across requests.
        /// </summary>
        public bool IsReusable => true;

        /// <summary>
        /// Gets the order value for determining the order of execution of filters. Filters execute in
        /// ascending numeric value of the <see cref="P:Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter.Order" /> property.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Filters are executed in an ordering determined by an ascending sort of the <see cref="P:Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter.Order" /> property.
        /// </para>
        /// <para>
        /// Asynchronous filters, such as <see cref="T:Microsoft.AspNetCore.Mvc.Filters.IAsyncActionFilter" />, surround the execution of subsequent
        /// filters of the same filter kind. An asynchronous filter with a lower numeric <see cref="P:Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter.Order" />
        /// value will have its filter method, such as <see cref="M:Microsoft.AspNetCore.Mvc.Filters.IAsyncActionFilter.OnActionExecutionAsync(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext,Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate)" />,
        /// executed before that of a filter with a higher value of <see cref="P:Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter.Order" />.
        /// </para>
        /// <para>
        /// Synchronous filters, such as <see cref="T:Microsoft.AspNetCore.Mvc.Filters.IActionFilter" />, have a before-method, such as
        /// <see cref="M:Microsoft.AspNetCore.Mvc.Filters.IActionFilter.OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext)" />, and an after-method, such as
        /// <see cref="M:Microsoft.AspNetCore.Mvc.Filters.IActionFilter.OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext)" />. A synchronous filter with a lower numeric <see cref="P:Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter.Order" />
        /// value will have its before-method executed before that of a filter with a higher value of
        /// <see cref="P:Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter.Order" />. During the after-stage of the filter, a synchronous filter with a lower
        /// numeric <see cref="P:Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter.Order" /> value will have its after-method executed after that of a filter with a higher
        /// value of <see cref="P:Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter.Order" />.
        /// </para>
        /// <para>
        /// If two filters have the same numeric value of <see cref="P:Microsoft.AspNetCore.Mvc.Filters.IOrderedFilter.Order" />, then their relative execution order
        /// is determined by the filter scope.
        /// </para>
        /// </remarks>
        public int Order { get; }
    }
}