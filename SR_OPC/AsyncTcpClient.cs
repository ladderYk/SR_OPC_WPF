
using SR_OPC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SR_OPC_WPF
{
    public class AsyncTcpClient
    {
        public delegate void ConnectedCallback(Device agv, string msg, bool result);
        public event ConnectedCallback OnConnectedCallback;

        private Device agv;
        //private S7Client siemensTcpNet;
        private DateTime LastRecTime = DateTime.MinValue;
        private Socket client;
        private bool IsConnect;
        private int ErrTimes;

        public AsyncTcpClient(Device _agv)
        {
            agv = _agv;
        }
        public void ConnectServer()
        {
            PingReply pingReply = new Ping().Send(agv.IP, 500);
            if (pingReply.Status != 0)
            {
                ConnectedCallback onConnectedCallback = this.OnConnectedCallback;
                if (onConnectedCallback != null)
                {
                    onConnectedCallback(agv, pingReply.Status.ToString(), false);
                }
            }
            else
            {
                #region 西门子plc
                //object typeObj = MainWindow.DeviceTypes.FirstOrDefault(d => d.Name == agv.Model).Type[1];
                //int type = int.Parse(typeObj.ToString());
                //siemensTcpNet = new SiemensS7Net((SiemensPLCS)type);
                //siemensTcpNet.IpAddress = agv.IP;
                //siemensTcpNet.Port = agv.Port;

                //siemensTcpNet.ConnectTimeOut = agv.Timeout;
                //siemensTcpNet.ReceiveTimeOut = agv.Timeout;
                //OperateResult connect = siemensTcpNet.ConnectServer();
                ////Utils.SaveErrorLog(DateTime.Now.ToString() + "  【" + agv.Name + "】 开始连接" + Constants.vbCrLf);
                //if (connect.IsSuccess)
                //{
                //    ConnectedCallback onConnectedCallback = this.OnConnectedCallback;
                //    if (onConnectedCallback != null)
                //    {
                //        onConnectedCallback(agv, "连接成功:" + siemensTcpNet.GetPipeSocket().IpAddress.ToString(), true);
                //    }
                //    LastRecTime = DateTime.Now;
                //}
                //else if (connect.ErrorCode == -1)
                //{
                //    //IsConnect = false;
                //}

                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.ReceiveTimeout = 3000;
                client.SendTimeout = 3000;
                client.BeginConnect(new IPEndPoint(IPAddress.Parse(agv.IP), agv.Port), AsyncConnectCallback, client);
                #endregion
            }
        }
        private void AsyncConnectCallback(IAsyncResult ar)
        {
            if (!ar.AsyncWaitHandle.WaitOne(3000))
            {
                return;
            }
            client = (Socket)ar.AsyncState;
            try
            {
                if (client.Connected)
                {
                    client.EndConnect(ar);
                    IsConnect = true;
                    LastRecTime = DateTime.Now;
                    Receive(client);
                    ConnectedCallback onConnectedCallback = this.OnConnectedCallback;
                    if (onConnectedCallback != null)
                    {
                        onConnectedCallback(agv, "连接成功:" + client.RemoteEndPoint.ToString(), true);
                    }
                    ErrTimes = 0;
                }
                else
                {
                    ConnectedCallback onConnectedCallback2 = this.OnConnectedCallback;
                    if (onConnectedCallback2 != null)
                    {
                        onConnectedCallback2(agv, "连接失败!", false);
                    }
                }
            }
            catch (SocketException ex)
            {
                ConnectedCallback onConnectedCallback3 = this.OnConnectedCallback;
                if (onConnectedCallback3 != null)
                {
                    onConnectedCallback3(agv, "连接失败：" + ex.Message, false);
                }
                Console.WriteLine(ex.Message ?? "");
            }
            catch (Exception ex2)
            {
                ErrTimes++;
                ConnectedCallback onConnectedCallback4 = this.OnConnectedCallback;
                if (onConnectedCallback4 != null)
                {
                    onConnectedCallback4(agv, "连接失败：" + ex2.Message, false);
                }
                IsConnect = false;
                if (ErrTimes < 3)
                {
                    client.Dispose();
                    Thread.Sleep(3000);
                    ConnectServer();
                }
            }
        }
        private void Receive(Socket client)
        {
            try
            {
                //StateObject stateObject = new StateObject
                //{
                //    workSocket = client
                //};
                //client.BeginReceive(stateObject.buffer, 0, 26, SocketFlags.None, ReceiveCallback, stateObject);
            }
            catch (Exception ex)
            {
                Console.WriteLine("接收异常：" + ex.ToString());
            }
        }
        public void DisConnected()
        {
            try
            {
                //OperateResult close = siemensTcpNet.ConnectClose();
                //siemensTcpNet.Dispose();
                //IsConnect = false;
                ConnectedCallback onConnectedCallback = this.OnConnectedCallback;
                if (onConnectedCallback != null)
                {
                    onConnectedCallback(agv, "超过30s无数据，断开重连", false);
                }
            }
            catch (Exception ex)
            {
            }
        }
        public byte[] ReadTest()
        {
            byte[] reqBytes = S7CmdUtil.Test();
            client.Send(reqBytes, 0, reqBytes.Length, SocketFlags.None);
            byte[] res = new byte[1024];
            client.Receive(res);
            return res;
        }
        public byte[] ReadByte(string address, ushort iLen)
        {
            try
            {
                //OperateResult<byte[]> read = siemensTcpNet.Read(address, iLen);
                //if (read.IsSuccess)
                //    return read.Content;
                //else
                //{
                //    TimeSpan timeSpan = DateTime.Now - LastRecTime;
                //    if (timeSpan.TotalSeconds > 3.0 && agv.IsConnected)
                //    {
                //        DisConnected();
                //    }
                //}
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool SendBool(string address, bool data)
        {
            try
            {
                //OperateResult write = siemensTcpNet.Write(address, data);
                //if (write.IsSuccess)
                //    return true;
                //else
                //{
                //    TimeSpan timeSpan = DateTime.Now - LastRecTime;
                //    if (timeSpan.TotalSeconds > 3.0 && agv.IsConnected)
                //    {
                //        DisConnected();
                //    }
                //}
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
