
using IoTClient;
using IoTClient.Clients.Modbus;
using IoTClient.Clients.PLC;
using IoTClient.Common.Enums;
using SR_OPC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SR_OPC_WPF
{
    public class ModbusTcp: IClient
    {
        public delegate void ConnectedCallback(Device agv, string msg, bool result);
        public event ConnectedCallback OnConnectedCallback;

        private Device agv;
        private ModbusTcpClient siemensTcpNet;
        private DateTime LastRecTime = DateTime.MinValue;
        public ModbusTcp(Device _agv)
        {
            agv = _agv;
            OnConnectedCallback += ResolveDataUtil.OnConnectedCallback;
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
                #region modbus
                object typeObj = MainWindow.DeviceTypes.FirstOrDefault(d => d.Name == agv.Model).Type[0];
                siemensTcpNet = new ModbusTcpClient(agv.IP, agv.Port, agv.Timeout);
                Result result = siemensTcpNet.Open();
                //OperateResult connect = siemensTcpNet.ConnectServer();
                //Utils.SaveErrorLog(DateTime.Now.ToString() + "  【" + agv.Name + "】 开始连接" + Constants.vbCrLf);
                if (result.IsSucceed)
                {
                    ConnectedCallback onConnectedCallback = this.OnConnectedCallback;
                    if (onConnectedCallback != null)
                    {
                        onConnectedCallback(agv, "连接成功:" + agv.IP, true);
                    }
                    LastRecTime = DateTime.Now;
                }
                else
                {
                    //IsConnect = false;
                }
                #endregion
            }
        }
        public void DisConnected()
        {
            try
            {
                siemensTcpNet.Close();
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
        public byte[] ReadByte(string address, ushort iLen)
        {
            try
            {
                Result<byte[]> result = siemensTcpNet.Read(address,1 ,3 ,iLen);
                if (result.IsSucceed)
                {
                    byte[] resultBys = new byte[iLen];
                    for (int i = 0; i<iLen; i++)
                    {
                        resultBys[i] = (byte)BitConverter.ToUInt16(result.Value.Skip(i * 2).Take(2).ToArray(), 0);
                    }

                    return resultBys;
                }
                else
                {
                    TimeSpan timeSpan = DateTime.Now - LastRecTime;
                    double timeOut = agv.Timeout / 1000f;
                    if (timeSpan.TotalSeconds >= timeOut && agv.IsConnected)
                    {
                        DisConnected();
                    }
                }
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
                var addressRegex = Regex.Match(address, @"DB(\d+)\.(\d+)");
                int addressDB = int.Parse(addressRegex?.Groups[1].Value);
                int startByteAdr = int.Parse(addressRegex?.Groups[2].Value);
                Result result = siemensTcpNet.Write(address, data);
                if (result.IsSucceed)
                    return true;
                else
                {
                    TimeSpan timeSpan = DateTime.Now - LastRecTime;
                    double timeOut = agv.Timeout / 1000f;
                    if (timeSpan.TotalSeconds >= timeOut && agv.IsConnected)
                    {
                        DisConnected();
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
