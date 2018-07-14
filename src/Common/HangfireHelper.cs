using System;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;

namespace Common
{
    public static class HangfireHelper
    {
        private static BackgroundJobClient Client { get; set; } = new BackgroundJobClient();

        public static string CreateJob(Type type, string method, string queue = "", params dynamic[] args)
        {
            var job = new Job(type, type.GetMethod(method), args);
            return string.IsNullOrEmpty(queue) ? Client.Create(job, new EnqueuedState()) : Client.Create(job, new EnqueuedState(queue));
        }
    }
}