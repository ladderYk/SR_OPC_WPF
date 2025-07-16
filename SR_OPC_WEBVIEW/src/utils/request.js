import axios from "axios";
import Qs from "qs";
// import { error, getItem, success, toUrl, reload } from ".";
export default { host };
// axios.interceptors.request.use(
//     config => {
//         let cancel;
//         config.cancelToken = new axios.CancelToken(c => cancel = c);
//         stopRepeatRequest(reqList, config.url, cancel, config.url + " 请求被中断")
//         return config;
//     }
// )
axios.interceptors.response.use(
  (response) => {
    // setTimeout(()=>allowRequest(reqList, response.config.url), 1000);
    if (!response) return;
    // 对响应数据做点什么
    return handleRet(response.data);
  },
  (errors) => {
    // 对响应错误做点什么
    if (!errors.response) {
      // error("请求错误");
      console.error("请求错误");
    } else if (errors.response.status !== 200) {
      handleResponse(errors.response);
    }
    return Promise.reject(errors);
  }
);

const handleResponse = (res) => {
  const { status, data } = res;

  switch (status) {
    case 400:
      error(data.msg);
      break;
    case 404:
      error(data);
      break;
    case 401:
      reload();
      break;
    case 500:
    case 504:
      // error("服务器连接错误！");
      break;
    case 403:
      error(data.msg);
      break;
    default:
      break;
  }
};
const handleRet = (response) => {
  if (!response) return;
  return response["data"] ? response.data : response.result;
};

export const get = (url, param = {}) =>
  axios.get(host + url, {
    params: param,
    // headers: {
    //     token: getItem("user") ? getItem("user").token : "",
    // },
  });

export const post = (url, param = {}) =>
  axios.post(host + url, JSON.stringify(param), {});
