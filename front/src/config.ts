// Vue 3 全局配置接口
export interface GlobalConfig {
  title: string;
  SildeBar: string;
  baseURL: string;
  timeOut: number;
  Max_KeepAlive: number;
}

// Vue 3 全局配置
export const globalConfig: GlobalConfig = {
  // 浏览器 title
  title: " | Vue Quasar",

  // 侧边栏风格
  SildeBar: "hHh lpR fFf", // 风格二：lHh lpR fFf

  // API基础地址，根据环境变量动态配置
  // 开发环境: /api (通过代理访问 https://127.0.0.1:5001/)
  // 生产环境: / (相对路径，与前端同域)
  baseURL: process.env.VUE_APP_API_BASE_URL || "",

  // 请求超时时间
  timeOut: 30000,

  // 组件最大缓存数
  Max_KeepAlive: 10,
};

export default globalConfig;
