export const addDeviceType = (form) => {
  if (chrome.webview) {
    const DeviceType = chrome.webview.hostObjects.DeviceType;
    DeviceType.addDeviceType(JSON.stringify(form)).then();
  }
  return Promise.reject("错误");

};

export const editDeviceType = (form) => {
  if (chrome.webview) {
    const DeviceType = chrome.webview.hostObjects.DeviceType;
    DeviceType.editDeviceType(JSON.stringify(form)).then();
  }
  return Promise.reject("错误");

};

export const getDeviceType = () => {
  if (chrome.webview) {
    const DeviceType = chrome.webview.hostObjects.DeviceType;
    return DeviceType.getDeviceType().then(handleRet);
  }
  return Promise.reject("错误");
};


export const addDevice = (form) => {
  if (chrome.webview) {
    const Device = chrome.webview.hostObjects.Device;
    Device.addDevice(JSON.stringify(form)).then();
  }
  return Promise.reject("错误");

};

export const editDevice = (form) => {
  if (chrome.webview) {
    const Device = chrome.webview.hostObjects.Device;
    Device.editDevice(JSON.stringify(form)).then();
  }
  return Promise.reject("错误");
};

export const getDeviceList = () => {
  if (chrome.webview) {
    const Device = chrome.webview.hostObjects.Device;
    return Device.getDeviceList().then(handleRet);
  }
  return Promise.reject("错误");
};

const handleRet = (strResponse) => {
  var response = JSON.parse(strResponse);
  if (!response) return;
  return response["data"] ? response.data : response.result;
};
