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
