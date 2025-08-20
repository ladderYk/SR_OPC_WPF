using Newtonsoft.Json.Linq;
using SR_OPC_WPF.Models;
using SR_OPC_WPF.Client;
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
                    if (type != null)
                    {

                        if (agv.Client == null)
                        {
                            if ((string)type.Type[0] == "Siemens")
                                agv.Client = new TcpClient(agv);
                            else if ((string)type.Type[0] == "ModbusTcp")
                                agv.Client = new ModbusTcp(agv);
                        }
                        agv.Client.ConnectServer();
                    }
                    //if (agv.tcpClient == null)
                    //    agv.tcpClient = new AsyncTcpClient(agv);
                    agv.IsReg = true;
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
                if (agv.IsConnected && agv.Client != null)
                {
                    Dictionary<string, List<object>> dataList = new Dictionary<string, List<object>>();
                    List<object> OdataList = new List<object>();
                    DeviceType type = MainWindow.DeviceTypes.FirstOrDefault(d => d.Name == agv.Model);

                    TypeRead[] reads = type.Read;
                    //Guid guid = Guid.NewGuid();
                    foreach (TypeRead read in reads)
                    {
                        int len = read.Len;
                        byte[] dataBytes = agv.Client.ReadByte(read.Addr, (ushort)len);
                        List<object> tdataList = new List<object>();

                        if (dataBytes != null)
                        {
                            List<byte> rdataBytes = dataBytes.Reverse().ToList();
                            OdataList.Add(new TypeReadVal(){ Name = read.Name, Tag=read.Tag, Data=rdataBytes });
                            int i = 0;
                            foreach (int dType in read.Config)
                            {
                                switch (dType)
                                {
                                    case -2:
                                        break;
                                    case 99:
                                        char[] c = ResolveDataUtil.byteTo8BitArr(rdataBytes[i]);
                                        tdataList.Add(c);
                                        break;
                                    case 0:
                                        tdataList.Add(rdataBytes[i] == 1);
                                        break;
                                    case -1:
                                    case 1:
                                        tdataList.Add(rdataBytes[i]);
                                        break;
                                    case 2:
                                        if (rdataBytes.Count >= 2)
                                        {
                                            short shortD = BitConverter.ToInt16(type.IsBig ? rdataBytes.Skip(i).Take(2).Reverse().ToArray() : rdataBytes.Skip(i).Take(2).ToArray(), 0);
                                            tdataList.Add(shortD);
                                        }
                                        else
                                            tdataList.Add(null);
                                        break;
                                    case 3:
                                        if (rdataBytes.Count >= 2)
                                        {
                                            ushort shortD = BitConverter.ToUInt16(type.IsBig ? rdataBytes.Skip(i).Take(2).Reverse().ToArray() : rdataBytes.Skip(i).Take(2).ToArray(), 0);
                                            tdataList.Add(shortD);
                                        }
                                        else
                                            tdataList.Add(null);
                                        break;
                                    case 4:
                                        if (rdataBytes.Count >= 4)
                                        {
                                            int intD = BitConverter.ToInt32(type.IsBig ? rdataBytes.Skip(i).Take(4).Reverse().ToArray() : rdataBytes.Skip(i).Take(4).ToArray(), 0);
                                            tdataList.Add(intD);
                                        }
                                        else
                                            tdataList.Add(null);
                                        break;
                                    case 5:
                                        if (rdataBytes.Count >= 4)
                                        {
                                            uint intD = BitConverter.ToUInt32(type.IsBig ? rdataBytes.Skip(i).Take(4).Reverse().ToArray() : rdataBytes.Skip(i).Take(4).ToArray(), 0);
                                            tdataList.Add(intD);
                                        }
                                        else
                                            tdataList.Add(null);
                                        break;
                                }
                                i++;
                            }
                            dataList.Add(read.Tag, tdataList);
                        }
                    }
                    agv.DataMap = dataList;
                    agv.DataList = OdataList;
                    //JObject agvCfg = MainWindow.AgvCfg.Value<JObject>("s7");
                    //#region 获取数据
                    JObject obj = new JObject();
                    if (dataList.Count > 0)
                    {
                        foreach (TypeConfig config in type.Config)
                        {
                            string[] source = config.Source;
                            object data = agv.DataMap[source[0]][int.Parse(source[1])];
                            if (data is char[])
                            {
                                obj.Add(config.Tag, (data as char[])[config.Offset] == '1');
                            }
                            else
                                obj.Add(config.Tag, JToken.FromObject(data));
                        }
                        
                        JObject oJsonData = agv.JsonDataList;
                        if (!JObject.DeepEquals(oJsonData, obj))
                        {
                            JArray socketData = new JArray();
                            socketData.Add(JArray.FromObject(OdataList));
                            socketData.Add(obj);
                            agv.JsonDataList = obj;
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
