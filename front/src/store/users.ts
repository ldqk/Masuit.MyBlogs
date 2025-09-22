import api from '@/axios/AxiosConfig'
import globalConfig from '@/config'
import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUserStore = defineStore('user', () => {
  // 用户信息状态
  const userInfo = ref<any | null>()
  const isLoggedIn = ref(false)

  // 获取用户信息
  const getUserInfo = async () => {
    if (userInfo.value) {
      return userInfo.value
    }

    try {
      const response = await api.get('/passport/getuserinfo')
      if (response.Data) {
        userInfo.value = response.Data
        isLoggedIn.value = true
        return response.Data
      }
      window.location.href = globalConfig.baseURL + '/passport/login?redirect=' + encodeURIComponent(window.location.pathname)
    } catch (error) {
      console.error('获取用户信息失败:', error)
      userInfo.value = null
      isLoggedIn.value = false
      throw error
    }
  }

  // 设置用户信息
  const setUserInfo = (user) => {
    userInfo.value = user
    isLoggedIn.value = true
  }

  // 清除用户信息
  const clearUserInfo = () => {
    userInfo.value = null
    isLoggedIn.value = false
  }

  // 检查是否已登录
  const checkLoginStatus = () => {
    return isLoggedIn.value && userInfo.value !== null
  }

  return {
    getUserInfo,
    setUserInfo,
    clearUserInfo,
    checkLoginStatus
  }
})
