using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using SR_OPC_WPF.ComData;
using Fleck;
using System.Collections.Concurrent;

namespace Websocket
{
    public class WebsocketVM : BaseVM
    {
        private WebSocketServer wsServer;
        //ModbusTcpNet modbusTcp = new ModbusTcpNet("127.0.0.1");
        private Thread IOThread;
        private bool IsConnect;
        ConcurrentDictionary<string, List<IWebSocketConnection>> allSockets = new ConcurrentDictionary<string, List<IWebSocketConnection>>();

        private static readonly WebsocketVM _Instance = new WebsocketVM();

        public static WebsocketVM Instance => _Instance;

        public void StartIO()
        {
            OnNTJYRun();
        }
        private void OnNTJYRun()
        {
            Task.Run(delegate
            {
                try
                {
                    IOThread = new Thread(NTJYRun);
                    IOThread.Start();
                }
                catch (Exception ex)
                {

                }
            });
        }
        public void StopIO()
        {
            if (IOThread != null)
            {
                IOThread.Abort();
                if (IsConnect)
                {
                    IsConnect = false;
                }
                wsServer.Dispose();
            }
        }
        private void NTJYRun()
        {
            while (true)
            {
                try
                {
                    if (!IsConnect)
                    {
                        wsServer = new WebSocketServer("ws://0.0.0.0:1883");
                        wsServer.SupportedSubProtocols = new[] { "superchat", "chat" };
                        wsServer.Start(socket =>
                        {
                            socket.OnOpen = () =>
                            {
                                Console.WriteLine("Open!");
                            };
                            socket.OnClose = () =>
                            {
                                Console.WriteLine("Close!");
                            };
                            socket.OnMessage = message => ProcessMessage(socket, message);
                            //...use as normal
                        });
                        //wsServer.ServerStart(1883);
                        //wsServer.IsTopicRetain = false;
                        //wsServer.OnClientConnected += (WebSocketSession session) =>
                        //{
                        //    session.AddTopic("agvList");
                        //    //wsServer.AddSessionTopic(session,"agvList");
                        //};
                        //wsServer.OnClientApplicationMessageReceive += (WebSocketSession session, WebSocketMessage message) =>
                        //{
                        //    JObject Payload = JObject.Parse(Encoding.UTF8.GetString(message.Payload));
                        //    if (Payload["action"].ToString() == "subscribe")
                        //    {
                        //        wsServer.AddSessionTopic(session, Payload["data"].ToString());
                        //    }
                        //};
                        IsConnect = true;
                    }
                    if (IsConnect)
                    {
                        //wsServer.PublishClientPayload("agvList", JArray.FromObject(MagneticViewModel.Instance.AGVs.Where(agv => agv.Status == StatusEnum.IsNormal)).ToString());
                    }
                }
                catch (Exception)
                {
                    //return;
                }
                Thread.Sleep(1000);
            }
        }
        void ProcessMessage(IWebSocketConnection socket, string message)
        {
            var msg = JObject.Parse(message);
            string topic = msg["topic"].ToString();
            string action = msg["action"].ToString();
            if (action == "subscribe")
            {
                if (!allSockets.ContainsKey(topic))
                {
                    allSockets[topic] = new List<IWebSocketConnection>();
                }
                allSockets[topic].Add(socket); // Add the socket to the list of connections for this topic if not already there.
            }
            else if (action == "unsubscribe")
            {
                if (!allSockets.ContainsKey(topic))
                {
                    allSockets[topic] = new List<IWebSocketConnection>();
                }
                allSockets[topic].Remove(socket);
            }
            //SendData(topic, data); // Broadcast the message to all sockets subscribed to this topic.
        }

        internal void SendData(string topic, string v)
        {
            if (allSockets.ContainsKey(topic))
            {
                foreach (var socket in allSockets[topic].ToArray())
                {
                    Task task = socket.Send(v);
                    if (task.IsFaulted)
                    {
                        socket.Close();
                        allSockets[topic].Remove(socket);
                    }
                }
            }
        }
    }
}
