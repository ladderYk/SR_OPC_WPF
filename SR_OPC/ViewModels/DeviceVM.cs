using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SR_OPC_WPF.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SR_OPC_WPF.ViewModels
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class DeviceVM
    {
        public string getDeviceList()
        {
            return req("200", "", JArray.FromObject(MainWindow.Devices));
        }
        public bool addDevice(string sbody)
        {
            Device device = JsonConvert.DeserializeObject<Device>(sbody);
            device.ID = Guid.NewGuid();
            MainWindow.Devices.Add(device);
            ResolveDataUtil.OnGetAGVState(device);
            ResolveDataUtil.OnGetData(device);

            string jsonfile = MainWindow.AGVFile;
            JArray jObject = JArray.FromObject(MainWindow.Devices);
            File.WriteAllText(jsonfile, jObject.ToString());
            return true;
        }
        public bool editDevice(string sbody)
        {
            Device device = JsonConvert.DeserializeObject<Device>(sbody);
            int typeIndex = MainWindow.Devices.FindIndex(dev => dev.ID == device.ID);
            if (device.ID == Guid.Empty)
                device.ID = Guid.NewGuid();
            if (typeIndex > -1)
            {
                MainWindow.Devices[typeIndex].Client.DisConnected();
                MainWindow.Devices[typeIndex].SetVal(device);
                string jsonfile = MainWindow.AGVFile;
                JArray jObject = JArray.FromObject(MainWindow.Devices);
                File.WriteAllText(jsonfile, jObject.ToString());
                return true;
            }
            return false;
        }
        private JObject reqBody(string code, string message, JToken data)
        {
            JObject reqData = new JObject();
            reqData.Add("code", code);
            reqData.Add("message", message);
            reqData.Add("data", data);
            return reqData;
        }
        private string req(string code, string message, JToken data)
        {
            return JsonConvert.SerializeObject(reqBody(code, message, data));
        }
    }
}
