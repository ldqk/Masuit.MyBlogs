using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using System;

namespace Masuit.MyBlogs.Core.Common
{
    /// <summary>
    /// HangfireHelper
    /// </summary>
    public static class HangfireHelper
    {
        private static BackgroundJobClient Client { get; } = new BackgroundJobClient();

        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="type">任务类</param>
        /// <param name="method">调用方法</param>
        /// <param name="queue">队列名</param>
        /// <param name="args">调用参数</param>
        /// <returns></returns>
        public static string CreateJob(Type type, string method, string queue = "", params dynamic[] args)
        {
            var job = new Job(type, type.GetMethod(method), args);
            return string.IsNullOrEmpty(queue) ? Client.Create(job, new EnqueuedState()) : Client.Create(job, new EnqueuedState(queue));
        }
    }
}