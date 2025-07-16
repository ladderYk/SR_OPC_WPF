using Newtonsoft.Json.Linq;
using SR_OPC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SR_OPC_WPF
{
    class ResolveDataUtil
    {
        public static void OnGetAGVState(Device agv)
        {
            Online(agv);
        }
        public static void Close(Device agv)
        {
            agv.Client?.DisConnected();
            agv.OnlineThread?.Abort();
            agv.GetDataThread?.Abort();
        }
        public static void OnGetData(Device agv)
        {
            GetData(agv);
        }
        private static void Online(Device agv)
        {
            Task.Run(delegate
            {
                if (!agv.IsConnected && !agv.IsReg)
                {
                    DeviceType type = MainWindow.DeviceTypes.FirstOrDefault(d => d.Name == agv.Model);

                    if (agv.Client == null)
                    {
                        if ((string)type.Type[0] == "Siemens")
                            agv.Client = new TcpClient(agv);
                        else if ((string)type.Type[0] == "ModbusTcp")
                            agv.Client = new ModbusTcp(agv);
                    }
                    //if (agv.tcpClient == null)
                    //    agv.tcpClient = new AsyncTcpClient(agv);
                    agv.IsReg = true;
                    agv.Client.ConnectServer();
                    // agv.tcpClient.OnConnectedCallback += ResolveDataUtil.OnConnectedCallback;
                    //agv.tcpClient.ConnectServer();
                    //Utils.AddErr(agv.Name, "连接中", "100");
                }
                else if (agv.IsReg)
                {
                    //Utils.RemoveErr(agv.Name, "100");
                    agv.IsReg = false;
                }
            }).ContinueWith((Func<Task, Task>)async delegate
            {
                await Task.Delay(agv.Timeout);
                Online(agv);
            });
        }
        private static void GetData(Device agv)
        {
            Task.Run(delegate
            {
                if (agv.IsConnected)
                {
                    Dictionary<string, List<byte>> dataList = new Dictionary<string, List<byte>>();
                    List<object> NdataList = new List<object>();
                    List<object> OdataList = new List<object>();
                    DeviceType type = MainWindow.DeviceTypes.FirstOrDefault(d => d.Name == agv.Model);

                    TypeRead[] reads = type.Read;
                    //Guid guid = Guid.NewGuid();
                    foreach (TypeRead read in reads)
                    {
                        int len = read.Len;
                        byte[] dataBytes = agv.Client.ReadByte(read.Addr, (ushort)len);

                        if (dataBytes != null)
                        {
                            dataList.Add(read.Tag, dataBytes.Reverse().ToList());
                            OdataList.Add(new TypeReadVal() { Data = dataBytes.Reverse().ToList(), Name = read.Name, Tag = read.Tag });
                        }
                    }
                    agv.DataMap = dataList;
                    agv.DataList = OdataList;
                    //JObject agvCfg = MainWindow.AgvCfg.Value<JObject>("s7");
                    //#region 获取数据
                    if (dataList.Count > 0)
                    {
                        foreach (TypeConfig config in type.Config)
                        {
                            List<byte> data = agv.DataMap[config.Name];
                            // 结构体
                            if (config.DType == 99)
                            {
                                char[] c = ResolveDataUtil.byteTo8BitArr(data[config.Offset]);
                                NdataList.Add(c);
                            }
                            // bool
                            else if (config.DType == 0)
                            {
                                NdataList.Add(data[config.Offset] == 1);
                            }
                            // byte
                            else if (config.DType == 1)
                            {
                                NdataList.Add(data[config.Offset]);
                            }
                            // short
                            else if (config.DType == 2)
                            {
                                if (data.Count >= config.Offset + 2)
                                {
                                    short shortD = BitConverter.ToInt16(type.IsBig ? data.Skip(config.Offset).Take(2).Reverse().ToArray() : data.Skip(config.Offset).Take(2).ToArray(), 0);
                                    NdataList.Add(shortD);
                                }
                                else
                                    NdataList.Add(null);

                            }
                            // ushort
                            else if (config.DType == 3)
                            {
                                if (data.Count >= config.Offset + 2)
                                {
                                    ushort ushortD = BitConverter.ToUInt16(type.IsBig ? data.Skip(config.Offset).Take(2).Reverse().ToArray(): data.Skip(config.Offset).Take(2).ToArray(), 0);
                                    NdataList.Add(ushortD);
                                }
                                else
                                    NdataList.Add(null);
                            }
                            // int
                            else if (config.DType == 4)
                            {
                                if (data.Count >= config.Offset + 4)
                                {
                                    int intD = BitConverter.ToInt32(type.IsBig ? data.Skip(config.Offset).Take(4).Reverse().ToArray() : data.Skip(config.Offset).Take(4).ToArray(), 0);
                                    NdataList.Add(intD);
                                }
                                else
                                    NdataList.Add(null);
                            }
                            // uint
                            else if (config.DType == 5)
                            {
                                if (data.Count >= config.Offset + 4)
                                {
                                    uint uintD = BitConverter.ToUInt32(type.IsBig ? data.Skip(config.Offset).Take(4).Reverse().ToArray() : data.Skip(config.Offset).Take(4).ToArray(), 0);
                                    NdataList.Add(uintD);
                                }
                                else
                                    NdataList.Add(null);
                            }
                        }
                        agv.NDataList = NdataList;

                        string oJsonData = agv.JsonData;
                        JArray socketData = new JArray();
                        socketData.Add(JArray.FromObject(OdataList));
                        socketData.Add(JArray.FromObject(NdataList));
                        if (oJsonData != socketData.ToString(Formatting.None))
                        {
                            agv.JsonData = socketData.ToString(Formatting.None);
                            Websocket.WebsocketVM.Instance.SendData("data/" + agv.Name, socketData.ToString(Formatting.None));
                        }
                        Websocket.WebsocketVM.Instance.SendData("online", new JObject { { "online", true }, { "name", agv.Name } }.ToString(Formatting.None));
                    }

                    //JObject info = agvCfg.Value<JObject>("info");
                    //string addr = info.Value<string>("addr");
                    //ushort len = info.Value<ushort>("len");
                    //JArray data = info.Value<JArray>("data");
                    //byte[] dataBytes = agv.Client.ReadByte(addr, len);
                    //if (dataBytes != null)
                    //{
                    //    var index = 0;
                    //    for (var i = 0; i < len; i++)
                    //    {
                    //        JObject datainfo = (JObject)data[index];
                    //        string type = datainfo.Value<string>("type");
                    //        int datalen = datainfo.Value<int>("len");

                    //        switch (type)
                    //        {
                    //            case "char":
                    //                bool reverse = datainfo.Value<bool>("reverse");

                    //                string strStatus = Convert.ToString(dataBytes[i], 2);
                    //                string b = "";
                    //                for (int j = 1; j <= 16 - strStatus.Length; j++)
                    //                {
                    //                    b += "0";
                    //                }
                    //                strStatus = b + strStatus;
                    //                char[] strC = reverse ? strStatus.Reverse().ToArray() : strStatus.ToArray();
                    //                dataList.Add(strC);
                    //                break;
                    //            case "byte":
                    //                dataList.Add(dataBytes[i]);
                    //                break;
                    //            case "ushort":
                    //                ushort usData = BitConverter.ToUInt16(dataBytes.Skip(i).Take(datalen).Reverse().ToArray(), 0);
                    //                dataList.Add(usData);
                    //                break;
                    //            case "double":
                    //                double dbData = Convert.ToDouble(dataBytes[i].ToString());
                    //                dataList.Add(dbData);
                    //                break;
                    //        }
                    //        i += datalen;
                    //        i--;
                    //        index++;
                    //    }
                    //    agv.DataList = dataList;
                    //}
                    //#endregion

                    //#region AGV状态
                    //JArray stateArr = agvCfg.Value<JArray>("state");
                    //foreach (JObject sArr in stateArr)
                    //{
                    //    bool isDefault = sArr.Value<bool>("default");
                    //    int sType = sArr.Value<int>("type");
                    //    JArray sData = sArr.Value<JArray>("data");
                    //    if (isDefault)
                    //    {
                    //        agv.State = (AGVStateEnum)sType;
                    //    }
                    //    else
                    //    {
                    //        string[] dataIndex = sData[0].ToString().Split('.');
                    //        char d = ((char[])dataList[int.Parse(dataIndex[0])])[int.Parse(dataIndex[1])];
                    //        if (sData[1].ToString() == "=")
                    //        {
                    //            if (d.ToString() == sData[2].ToString())
                    //            {
                    //                agv.State = (AGVStateEnum)sType;
                    //            }
                    //        }
                    //    }
                    //}
                    //int tagXIndex = agvCfg.Value<int>("tagX");
                    //int tagYIndex = agvCfg.Value<int>("tagY");
                    //int batteryIndex = agvCfg.Value<int>("battery");
                    //int speedIndex = agvCfg.Value<int>("speed");
                    //string heartBeatIndex = agvCfg.Value<string>("heartBeat");
                    //string[] heartBeatIndexArr = heartBeatIndex.ToString().Split('.');
                    //string HeartBeat = ((char[])dataList[int.Parse(heartBeatIndexArr[0])])[int.Parse(heartBeatIndexArr[1])].ToString();

                    //string isErrIndex = agvCfg.Value<string>("isErr");
                    //string[] isErrIndexArr = isErrIndex.ToString().Split('.');
                    //string isErr = ((char[])dataList[int.Parse(isErrIndexArr[0])])[int.Parse(isErrIndexArr[1])].ToString();

                    //agv.IsErr = isErr == "1";
                    //agv.CurStn = dataList[tagXIndex] + "-" + dataList[tagYIndex];
                    //agv.Battery = Convert.ToDouble(dataList[batteryIndex]);
                    //agv.Speed = Convert.ToDouble(dataList[speedIndex]);

                    //string sendHeartBeatAddr = agvCfg.Value<string>("sendHeartBeat");

                    //bool result1 = false;//心跳位(默认值为0)

                    //if (HeartBeat == "1")
                    //{
                    //    result1 = false;
                    //}
                    //if (HeartBeat == "0")
                    //{
                    //    result1 = true;
                    //}
                    //AGVControlUtil.SendBool(agv, sendHeartBeatAddr, result1);

                    //#endregion

                    //#region 报警
                    //JArray errArr = agvCfg.Value<JArray>("err");
                    //foreach (JObject eArr in errArr)
                    //{
                    //    int sType = eArr.Value<int>("type");
                    //    string sMsg = eArr.Value<string>("msg");
                    //    JArray condition = eArr.Value<JArray>("condition");
                    //    int dataIndex = int.Parse((string)condition[0]);
                    //    var sList = condition[1].ToString().Split(',');
                    //    bool hasErr = ErrDataVM.Instance.hasError(agv.Name, sType.ToString());
                    //    if (sList.Contains(dataList[dataIndex].ToString()))
                    //    {
                    //        Utils.AddErr(agv.Name, string.Format("{0:MM/dd HH:mm:ss}", DateTime.Now) + " 【" + agv.Name + "】" + sMsg, sType.ToString());
                    //        agv.State = (AGVStateEnum)sType;
                    //    }
                    //    else if (hasErr)
                    //        Utils.RemoveErr(agv.Name, sType.ToString());

                    //}
                    //#endregion
                }
            }).ContinueWith((Func<Task, Task>)async delegate
            {
                await Task.Delay(agv.Cycle);
                GetData(agv);
            });
        }
        internal static void OnConnectedCallback(Device agv, string msg, bool result)
        {
            if (agv == null)
            {
                return;
            }
            bool showErr = agv.IsConnected && !result;

            agv.IsConnected = result;
            if (result)
            {
                //Utils.AddRtMsg("AGV" + agv.Name + "[" + agv.IP + "]连接成功！");
                //MagneticMapView.MapVM.PutAgvPosition(agv.Name, "#FF32cd32");
                //Utils.SaveErrorLog(DateTime.Now.ToString() + "  【" + agv.Name + "】 连接成功" + Constants.vbCrLf);
                //OnGetData(agv);
                return;
            }
            Websocket.WebsocketVM.Instance.SendData("online", new JObject { { "online", false }, { "name", agv.Name } }.ToString());
            // Utils.AddRtMsg("AGV" + agv.Name + "[" + agv.IP + "]连接失败：" + msg + "！", 1);
            //MagneticMapView.MapVM.PutAgvPosition(agv.Name, "#FF808080");

            if (showErr)
            {
                //Utils.AddErr(agv.Name, DateTime.Now.ToString() + "  【" + agv.Name + "】 离线", "0");
                //agv.LogicalSite = "";
            }
            if (agv.GetDataThread != null)
            {
                agv.GetDataThread.Abort();
                Console.WriteLine("心跳线程终止");
            }
        }
        private static char[] byteTo8BitArr(byte bye)
        {
            return Convert.ToString(bye, 2).PadLeft(8, '0').Reverse().ToArray();
        }
    }
}
