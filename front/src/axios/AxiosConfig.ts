import Axios, {
  AxiosResponse,
  AxiosError,
  InternalAxiosRequestConfig,
} from "axios";
import qs from "qs";
import { globalConfig } from "../config";
import { toast } from "vue3-toastify";

/**
 * axios 初始化
 */
const api = Axios.create({
  baseURL: globalConfig.baseURL, // 请求基地址，根据环境动态配置
  timeout: globalConfig.timeOut, // 超时时间
});

// 请求拦截器
api.interceptors.request.use(
  (config: InternalAxiosRequestConfig & { type?: "FORM-DATA" | "FORM" }) => {
    const token = sessionStorage.getItem("access_token");
    if (token) {
      config.headers.Authorization = "Bearer " + token;
    }

    if ((config as any).type) {
      switch ((config as any).type) {
        case "FORM-DATA":
          config.transformRequest = [
            (data: any) => {
              return "args=" + JSON.stringify(data);
            },
          ];
          break;
        case "FORM":
          config.headers["Content-Type"] = "application/x-www-form-urlencoded";
          config.data = qs.stringify(config.data);
          break;
        default:
          break;
      }
    }
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  },
);

// 响应拦截器
api.interceptors.response.use(
  (response: AxiosResponse) => {
    if (typeof response.data == "object" && Object.keys(response.data).includes("Success") && response.data.Success !== true) {
      toast.error(response.data.Message || `请求失败:${response.request.responseURL}`, { autoClose: 3000, position: "top-center", });
    }
    return response.data;
  },
  (error: AxiosError) => {
    if (error.code === "ECONNABORTED" || (error.message && error.message.indexOf("timeout") !== -1) || error.message === "Network Error") {
      toast.error("网络异常", { autoClose: 3000, position: "top-center" });
      return Promise.reject(error);
    }

    if (error.response) {
      switch (error.response.status) {
        case 400:
          toast.error("请求错误:" + (error.response.data as any).Message || "(400)", { autoClose: 3000, position: "top-center" });
          break;
        case 403:
          toast.error("拒绝访问(403)", { autoClose: 3000, position: "top-center", });
          break;
        case 404:
          toast.error("资源不存在(404)", { autoClose: 3000, position: "top-center" });
          break;
        case 408:
          toast.error("请求超时(408)", { autoClose: 3000, position: "top-center" });
          break;
        case 500:
          toast.error("服务器错误(500)", { autoClose: 3000, position: "top-center", });
          break;
        case 501:
          toast.error("服务未实现(501)", { autoClose: 3000, position: "top-center" });
          break;
        case 502:
          toast.error("网络错误(502)", { autoClose: 3000, position: "top-center" });
          break;
        case 503:
          toast.error("服务不可用(503)", { autoClose: 3000, position: "top-center" });
          break;
        case 504:
          toast.error("网络超时(504)", { autoClose: 3000, position: "top-center" });
          break;
        case 505:
          toast.error("HTTP版本不受支持(505)", { autoClose: 3000, position: "top-center" });
          break;
        default:
          break;
      }
    }

    return Promise.reject(error);
  },
);

export default api;
