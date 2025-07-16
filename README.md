<h1 align="center">
SR_OPC 
</h1>

## 简体中文

- 这是一个物联网设备网关，参考OPC设计，现支持西门子PLC、ModBus协议工业通讯协议，后期可以加入更多协议。
- 本工具分前后端，后端C#基于.NET framework 4.7开发，前端使用vue3，后端使用轻量级web服务，将前端界面呈现在webview2上。
- 本组件终身开源免费，采用最宽松MIT协议，您也可以随意修改和商业使用（商业使用请做好评估和测试）。  
- 开发工具：Visual Studio 2019 

## 文档目录
<!-- TOC -->

- [使用说明](#%E4%BD%BF%E7%94%A8%E8%AF%B4%E6%98%8E)
    - [运行项目](#%E8%BF%90%E8%A1%8C%E9%A1%B9%E7%9B%AE)
    - [添加协议](#%E6%B7%BB%E5%8A%A0%E5%8D%8F%E8%AE%AE)
    - [添加设备](#%E6%B7%BB%E5%8A%A0%E8%AE%BE%E5%A4%87)
    - [查看数据](#%E6%9F%A5%E7%9C%8B%E6%95%B0%E6%8D%AE)
    - [TODO](#TODO)
- [感谢](#%E6%84%9F%E8%B0%A2)
    - [IoTClient](#IoTClient)

<!-- /TOC -->

# 使用说明
## 运行项目
<img width="1280" height="770" alt="1752646037961" src="https://github.com/user-attachments/assets/7f94d257-400b-44dc-9646-9263c2f8693f" />
将SR_OPC_WEBVIEW\dist 拷贝到 c#运行目录
<img width="1050" height="600" alt="1752646337357" src="https://github.com/user-attachments/assets/32ca1741-6c28-467d-92f3-a4974155dc2c" />

## 添加协议
<img width="1040" height="572" alt="1752646999448" src="https://github.com/user-attachments/assets/f46aeec7-4512-40a7-b87a-34a74657817f" />
添加原始数据，名称为标识，标签为数据的key，地址对应plc或modbus地址，长度对应读取多少位byte。
<img width="1040" height="572" alt="1752647009425" src="https://github.com/user-attachments/assets/15d035d1-2012-4c44-b2f8-7fde8f7e52b0" />
点击生成，根据原始数据生成表格，设定数据格式。

## 添加设备
<img width="1040" height="572" alt="1752646716117" src="https://github.com/user-attachments/assets/9fd47f91-1f4a-47ec-ba8b-b328b07ed8d5" />
添加设备，设置设备协议。

## 查看数据
<img width="1040" height="572" alt="1752647062747" src="https://github.com/user-attachments/assets/df7f3a09-8348-4b7b-ad48-6a728c46753f" />
原始数据
<img width="1040" height="572" alt="1752647071893" src="https://github.com/user-attachments/assets/fc997c1b-24fd-41e9-bcc5-7a4fde3afd1f" />
解析数据
数据通过WebSocket推送，也可以通过webapi请求数据。

## TODO
- 加入其他plc
- 增加写入功能
- APP
  
# 感谢
## IoTClient
IoTClient (https://github.com/zhaopeiym/IoTClient)  
