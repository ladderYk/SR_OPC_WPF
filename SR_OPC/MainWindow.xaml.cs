using Nancy.Hosting.Self;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SR_OPC_WPF.Models;
using SR_OPC_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace SR_OPC_WPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private NancyHost host;

        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        public static string AGVFile
        {
            get
            {
                return Path.Combine(BasePath, "UserData", "AGV.json");
            }
        }
        public static string ModelFile
        {
            get
            {
                return Path.Combine(BasePath, "UserData", "dType.json");
            }
        }
        public MainWindow()
        {
            InitializeComponent();
        }
        public static JArray AgvList { get; set; }
        public static JArray AgvModel { get; set; }
        public static List<DeviceType> DeviceTypes { get; set; } = new List<DeviceType>();
        public static List<Device> Devices { get; set; } = new List<Device>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (System.IO.StreamReader file = File.OpenText(AGVFile))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    AgvList = (JArray)JToken.ReadFrom(reader);
                }
            }

            using (System.IO.StreamReader file = File.OpenText(ModelFile))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    AgvModel = (JArray)JToken.ReadFrom(reader);
                }
            }
            foreach (JObject model in AgvModel)
            {
                DeviceType deviceType = model.ToObject<DeviceType>();
                DeviceTypes.Add(deviceType);
            }
            foreach (JObject agv in AgvList)
            {
                Device device = agv.ToObject<Device>();
                Devices.Add(device);
                ResolveDataUtil.OnGetAGVState(device);
                ResolveDataUtil.OnGetData(device);
            }
            Websocket.WebsocketVM.Instance.StartIO();
            abc();
            webView.CoreWebView2InitializationCompleted += CoreWebView2InitializationCompleted;
        }
        private void CoreWebView2InitializationCompleted(object sender, EventArgs e)
        {
            webView.CoreWebView2.AddHostObjectToScript("DeviceType", new DeviceTypeVM());
            webView.CoreWebView2.AddHostObjectToScript("Device", new DeviceVM());
        }
        public void abc()
        {
            var url = $"http://localhost";

            host = new NancyHost(new Uri(url));
            host.Start();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            foreach (Device device in Devices)
            {
                ResolveDataUtil.Close(device);
            }
            Websocket.WebsocketVM.Instance.StopIO();
            host?.Stop();
            host?.Dispose();
            base.OnClosing(e);
        }
    }
}
