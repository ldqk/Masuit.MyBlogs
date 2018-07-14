using System;
using System.Threading;
using Hangfire;
using Masuit.MyBlogs.WebApp.Hubs;
using Masuit.Tools.DateTimeExt;
using Masuit.Tools.Hardware;
using Masuit.Tools.Logging;
using Masuit.Tools.Win32;
using static Common.CommonHelper;

namespace Masuit.MyBlogs.WebApp
{
    /// <summary>
    /// 收集系统运行状态
    /// </summary>
    public class CollectRunningInfo
    {
        public static void Start()
        {
            MyHub.PushData(a =>
            {
                try
                {
                    double time = DateTime.Now.GetTotalMilliseconds();// - 28800000;
                    float load = SystemInfo.CpuLoad;
                    double temperature = SystemInfo.GetCPUTemperature();
                    double mem = (1 - SystemInfo.MemoryAvailable.To<double>() / SystemInfo.PhysicalMemory.To<double>()) * 100;
                    a.receiveLoad($"[{time},{load},{mem},{temperature}]");//CPU

                    var read = SystemInfo.GetDiskData(DiskData.Read) / 1024;
                    var write = SystemInfo.GetDiskData(DiskData.Write) / 1024;
                    a.receiveReadWrite($"[{time},{read},{write}]");//磁盘IO

                    var up = SystemInfo.GetNetData(NetData.Received) / 1024;
                    var down = SystemInfo.GetNetData(NetData.Sent) / 1024;
                    a.receiveUpDown($"[{time},{down},{up}]");//网络上下载

                    if (mem > 90)
                    {
                        mem = Windows.ClearMemory();
                        Thread.Sleep(2000);
                    }
                    if (mem > 90)
                    {
                        BackgroundJob.Enqueue(() => SendMail("网站服务器负载过大预警！", "网站服务器负载过大，内存使用率已经超过90%，请及时检查服务器，避免网站被终止运行", GetSettings("ReceiveEmail")));
                    }
                    //缓存历史数据
                    if (HistoryCpuLoad.Count < 50 || (time / 10000).ToInt32() % 12 == 0)
                    {
                        HistoryCpuLoad.Add(new object[] { time, load });
                        HistoryCpuTemp.Add(new object[] { time, temperature });
                        HistoryMemoryUsage.Add(new object[] { time, mem });
                        HistoryIORead.Add(new object[] { time, read });
                        HistoryIOWrite.Add(new object[] { time, write });
                        HistoryNetReceive.Add(new object[] { time, up });
                        HistoryNetSend.Add(new object[] { time, down });
                        if (HistoryCpuLoad.Count > 720)
                        {
                            HistoryCpuLoad.RemoveAt(0);
                            HistoryMemoryUsage.RemoveAt(0);
                            HistoryCpuTemp.RemoveAt(0);
                        }
                        if (HistoryIORead.Count > 720)
                        {
                            HistoryIORead.RemoveAt(0);
                            HistoryIOWrite.RemoveAt(0);
                            HistoryNetReceive.RemoveAt(0);
                            HistoryNetSend.RemoveAt(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    LogManager.Error(e);
                }
            });
        }
    }
}