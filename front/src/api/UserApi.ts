import api from '../axios/AxiosConfig'

// API 响应接口
export interface ApiResponse<T = any> {
  data: T
  status: number
  statusText: string
}

/**
 * 获取用户路由配置
 * @returns Promise<ApiResponse<string>>
 */
export function getUserRouter(): Promise<ApiResponse<string>> {
  return api.get('/passport/getuserinfo')
}
