using Masuit.Tools;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Hardware;
using Masuit.Tools.Systems;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Masuit.MyBlogs.Core.Hubs
{
    /// <summary>
    /// 性能计数器
    /// </summary>
    public class PerformanceCounter
    {
        /// <summary>
        /// 当前时间戳
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// CPU当前负载
        /// </summary>
        public double CpuLoad { get; set; }

        /// <summary>
        /// 内存使用率
        /// </summary>
        public double MemoryUsage { get; set; }

        /// <summary>
        /// 磁盘读
        /// </summary>
        public double DiskRead { get; set; }

        /// <summary>
        /// 磁盘写
        /// </summary>
        public double DiskWrite { get; set; }

        /// <summary>
        /// 网络上行
        /// </summary>
        public double Upload { get; set; }

        /// <summary>
        /// 网络下行
        /// </summary>
        public double Download { get; set; }
    }

    /// <summary>
    /// hub
    /// </summary>
    public class MyHub : Hub
    {
        /// <summary>
        /// 性能计数器缓存
        /// </summary>
        public static ConcurrentLimitedQueue<PerformanceCounter> PerformanceCounter { get; set; } = new ConcurrentLimitedQueue<PerformanceCounter>(5000);

        static MyHub()
        {
            Task.Run(() =>
            {
                int errorCount = 0;
                while (true)
                {
                    try
                    {
                        PerformanceCounter.Enqueue(GetCurrentPerformanceCounter());
                    }
                    catch (Exception e)
                    {
                        if (errorCount > 20)
                        {
                            break;
                        }
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message);
                        Console.ForegroundColor = ConsoleColor.White;
                        errorCount++;
                    }
                    Thread.Sleep(10000);
                }
            });
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
        }

        /// <summary>
        /// 当前连接客户端
        /// </summary>
        public static ConcurrentDictionary<string, bool> Connections { get; set; } = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 连接事件
        /// </summary>
        /// <returns></returns>
        public override Task OnConnectedAsync()
        {
            Connections.TryAdd(Context.ConnectionId, false);
            return base.OnConnectedAsync();
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Connections.TryRemove(Context.ConnectionId, out _);
            return Task.CompletedTask;
        }

        /// <summary>
        /// 性能统计
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public ChannelReader<object> Counter(int delay, CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<object>();
            _ = WriteItemsAsync(channel.Writer, delay, cancellationToken);
            return channel.Reader;
        }

        private async Task WriteItemsAsync(ChannelWriter<object> writer, int delay, CancellationToken cancellationToken)
        {
            if (Connections[Context.ConnectionId])
            {
                return;
            }
            byte errCount = 0;
            while (Connections.Any(s => s.Key.Equals(Context.ConnectionId)))
            {
                Connections[Context.ConnectionId] = true;
                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.WriteAsync(GetCurrentPerformanceCounter(), cancellationToken);
                }
                catch (Exception e)
                {
                    if (errCount > 20)
                    {
                        break;
                    }
                    Console.WriteLine("WebSocket出现错误:" + e.Message);
                    errCount++;
                }
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await Task.Delay(delay, cancellationToken);
            }
            writer.TryComplete();
        }

        private static PerformanceCounter GetCurrentPerformanceCounter()
        {
            double time = DateTime.Now.GetTotalMilliseconds(); // - 28800000;
            float load = SystemInfo.CpuLoad;
            double mem = (1 - SystemInfo.MemoryAvailable.To<double>() / SystemInfo.PhysicalMemory.To<double>()) * 100;

            var read = SystemInfo.GetDiskData(DiskData.Read) / 1024;
            var write = SystemInfo.GetDiskData(DiskData.Write) / 1024;

            var up = SystemInfo.GetNetData(NetData.Received) / 1024;
            var down = SystemInfo.GetNetData(NetData.Sent) / 1024;
            var counter = new PerformanceCounter()
            {
                Time = time,
                CpuLoad = load,
                MemoryUsage = mem,
                DiskRead = read,
                DiskWrite = write,
                Download = down,
                Upload = up
            };
            return counter;
        }
    }
}