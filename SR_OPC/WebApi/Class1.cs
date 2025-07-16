using Nancy;
using Nancy.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SR_OPC_WPF;
using SR_OPC_WPF.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BYD_CS.WebApi
{
    public class Class1 : NancyModule
    {
        public Class1(IRootPathProvider path)
        {
            string curDir = Directory.GetCurrentDirectory();

            After.AddItemToEndOfPipeline((ctx) => ctx.Response
            .WithHeader("Access-Control-Allow-Origin", "*")
            .WithHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS")
            .WithHeader("Access-Control-Allow-Headers", "Accept, Origin, Content-type"));
            Get("/", d =>
            {
                return Response.AsFile(curDir + "/dist/index.html" as String);
            });

            Get("/{fileName*}", parameters =>
            {
                return Response.AsFile(curDir + "/dist/" + parameters.fileName as String);
            });
            Get("/DeviceList", d =>
            {
                JArray jObject = JArray.FromObject(MainWindow.Devices);
                return req("200", "", jObject);
            });
            Get("/TypeList", d =>
            {
                JArray jObject = JArray.FromObject(MainWindow.DeviceTypes);
                return req("200", "", jObject);
            });
            Post("/data", d =>
            {
                var s = Request.Form;
                string device = (string)s["device"];
                string name = (string)s["name"];

                Device device1 = MainWindow.Devices.FirstOrDefault(dev => dev.Name == device);
                if (device1 == null)
                    return req("0", "设备不存在！", null);
                if (!device1.IsConnected)
                    return req("0", "设备不在线！", null);
                DeviceType type = MainWindow.DeviceTypes.FirstOrDefault(dt => dt.Name == device1.Model);

                TypeWrite typeWrite = type.Write.FirstOrDefault(w => w.Tag == name);
                if (typeWrite == null)
                    return req("0", "标签不存在！", null);

                bool isSend = device1.Client.SendBool(typeWrite.Addr, (bool)s["data"]);
                return req("200", "成功", null);
                //return JsonConvert.SerializeObject(new { data = MainWindow.devices });
            });
            Post("/addType", d =>
            {
                string sbody = Request.Body.AsString();
                DeviceType device = JsonConvert.DeserializeObject<DeviceType>(sbody);
                device.ID = Guid.NewGuid();
                MainWindow.DeviceTypes.Add(device);
                string jsonfile = MainWindow.ModelFile;
                JArray jObject = JArray.FromObject(MainWindow.DeviceTypes);
                File.WriteAllText(jsonfile, jObject.ToString());
                //string name = s.Value<string>("name");
                //string[] type = s.Value<string[]>("type");
                //DeviceType device = new DeviceType() { id = Guid.NewGuid(), name = name, type = type };

                //Device device1 = MainWindow.devices.FirstOrDefault(dev => dev.Name == device);
                //if (device1 == null)
                //    return req("0", "设备不存在！", null);
                //if (!device1.IsConnected)
                //    return req("0", "设备不在线！", null);
                //TypeWrite typeWrite = device1.type.write.FirstOrDefault(w => w.name == name);
                //if (typeWrite == null)
                //    return req("0", "标签不存在！", null);

                //bool isSend = device1.Client.WriyeBool(typeWrite.addr, (bool)s["data"]);
                return req("200", "成功", null);
                //return JsonConvert.SerializeObject(new { data = MainWindow.devices });
            });
            Post("/updateType", d =>
            {
                string sbody = Request.Body.AsString();
                DeviceType device = JsonConvert.DeserializeObject<DeviceType>(sbody);
                int typeIndex = MainWindow.DeviceTypes.FindIndex(dev => dev.ID == device.ID);
                if (device.ID == Guid.Empty)
                    device.ID = Guid.NewGuid();
                MainWindow.DeviceTypes[typeIndex] = device;
                string jsonfile = MainWindow.ModelFile;
                JArray jObject = JArray.FromObject(MainWindow.DeviceTypes);
                File.WriteAllText(jsonfile, jObject.ToString());
                //string name = s.Value<string>("name");
                //string[] type = s.Value<string[]>("type");
                //DeviceType device = new DeviceType() { id = Guid.NewGuid(), name = name, type = type };

                //Device device1 = MainWindow.devices.FirstOrDefault(dev => dev.Name == device);
                //if (device1 == null)
                //    return req("0", "设备不存在！", null);
                //if (!device1.IsConnected)
                //    return req("0", "设备不在线！", null);
                //TypeWrite typeWrite = device1.type.write.FirstOrDefault(w => w.name == name);
                //if (typeWrite == null)
                //    return req("0", "标签不存在！", null);

                //bool isSend = device1.Client.WriyeBool(typeWrite.addr, (bool)s["data"]);
                return req("200", "成功", null);
                //return JsonConvert.SerializeObject(new { data = MainWindow.devices });
            });
            //Post("/delType", d =>
            //{
            //    return req("200", "成功", null);
            //})
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
