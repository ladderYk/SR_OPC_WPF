using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SR_OPC_WPF.Models
{
    public class Device
    {
        public Guid ID;
        public string Name;

        public string IP;
        public int Port;
        public bool IsAutoConn;

        public string Model;
        public int Timeout;
        public int Cycle;

        public bool IsConnected;
        public string JsonData;

        public Dictionary<string, List<byte>> DataMap = new Dictionary<string, List<byte>>();
        public List<object> DataList = new List<object>();
        public List<object> NDataList = new List<object>();
        public ObservableCollection<object> DataList1 = new ObservableCollection<object>();

        [IgnoreDataMember]
        public bool IsReg;
        [IgnoreDataMember]
        public IClient Client;
        [IgnoreDataMember]
        public AsyncTcpClient tcpClient;

        [IgnoreDataMember]
        public Thread OnlineThread;
        [IgnoreDataMember]
        public Thread GetDataThread;

        public void SetVal(Device device)
        {
            this.ID = device.ID;
            this.Name = device.Name;
            this.IP = device.IP;
            this.Port = device.Port;
            this.IsAutoConn = device.IsAutoConn;

            this.Model = device.Model;
            this.Timeout = device.Timeout;
            this.Cycle = device.Cycle;
        }
    }
}
