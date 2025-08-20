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
    public class DeviceTypeVM
    {
        public string getDeviceType()
        {
            return req("200", "", JArray.FromObject(MainWindow.DeviceTypes));
        }
        public bool addDeviceType(string sbody)
        {
            DeviceType device = JsonConvert.DeserializeObject<DeviceType>(sbody);
            device.ID = Guid.NewGuid();
            MainWindow.DeviceTypes.Add(device);
            string jsonfile = MainWindow.ModelFile;
            JArray jObject = JArray.FromObject(MainWindow.DeviceTypes);
            File.WriteAllText(jsonfile, jObject.ToString(Formatting.None));
            return true;
        }
        public bool editDeviceType(string sbody)
        {
            DeviceType device = JsonConvert.DeserializeObject<DeviceType>(sbody);
            int typeIndex = MainWindow.DeviceTypes.FindIndex(dev => dev.ID == device.ID);
            if (device.ID == Guid.Empty)
                device.ID = Guid.NewGuid();
            MainWindow.DeviceTypes[typeIndex] = device;
            string jsonfile = MainWindow.ModelFile;
            JArray jObject = JArray.FromObject(MainWindow.DeviceTypes);
            File.WriteAllText(jsonfile, jObject.ToString(Formatting.None));
            return true;
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
