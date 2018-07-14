using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Masuit.MyBlogs.WebApp.Hubs
{
    [HubName("myhub")]
    public class MyHub : Hub
    {
        public static List<string> SubscribeClientIds { get; set; } = new List<string>(); //存放客户端id集合

        public static readonly IHubConnectionContext<dynamic> Connections = GlobalHost.ConnectionManager.GetHubContext<MyHub>().Clients;

        public override Task OnDisconnected(bool stopCalled)
        {
            SubscribeClientIds.Remove(Context.ConnectionId);
            return base.OnDisconnected(stopCalled);
        }

        /// <summary>
        /// 订阅
        /// </summary>
        public void Update()
        {
            SubscribeClientIds.Add(Context.ConnectionId);
        }

        /// <summary>
        /// 推送数据
        /// </summary>
        /// <param name="cb"></param>
        public static void PushData(Action<dynamic> cb)
        {
            cb(Connections.Clients(SubscribeClientIds));
        }
    }
}