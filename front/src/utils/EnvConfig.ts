/**
 * 环境配置测试工具
 * 用于验证不同环境下的API配置是否正确
 */

console.log('=== 环境配置信息 ===')
console.log('NODE_ENV:', process.env.NODE_ENV)
console.log('VUE_APP_API_BASE_URL:', process.env.VUE_APP_API_BASE_URL)
console.log('VUE_APP_ENV:', process.env.VUE_APP_ENV)
console.log('VUE_APP_TITLE:', process.env.VUE_APP_TITLE)

export function getApiBaseUrl(): string {
  return process.env.VUE_APP_API_BASE_URL || ''
}

export function getCurrentEnv(): string {
  return process.env.VUE_APP_ENV || process.env.NODE_ENV || 'unknown'
}

export function logApiConfig(): void {
  console.log('=== API配置 ===')
  console.log('当前环境:', getCurrentEnv())
  console.log('API基础URL:', getApiBaseUrl())
  
  if (getCurrentEnv() === 'development') {
    console.log('开发环境: API请求将通过代理转发到 https://127.0.0.1:5001/')
    console.log('实际请求路径: /api/* -> https://127.0.0.1:5001/*')
  } else if (getCurrentEnv() === 'production') {
    console.log('生产环境: API请求使用相对路径，与前端同域')
    console.log('实际请求路径: /* -> 当前域名/*')
  }
}