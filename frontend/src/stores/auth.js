import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '../services/api'

export const useAuthStore = defineStore('auth', () => {
  const isAuthenticated = ref(false)
  const user = ref(null)
  const token = ref(localStorage.getItem('token') || null)

  // 初始化時檢查認證狀態
  if (token.value) {
    checkAuth()
  }

  const login = async (username, code) => {
    try {
      const response = await api.post('/Auth/login', { 
        username: username.trim(), 
        code: code.trim() 
      })
      
      if (response.data && response.data.token) {
        token.value = response.data.token
        user.value = response.data.user
        isAuthenticated.value = true
        localStorage.setItem('token', token.value)
        return { success: true }
      } else {
        return {
          success: false,
          message: '登入回應格式錯誤'
        }
      }
    } catch (error) {
      const errorMessage = error.response?.data?.message || 
                         error.message || 
                         '登入失敗，請檢查網路連線'
      
      return {
        success: false,
        message: errorMessage
      }
    }
  }

  const logout = async () => {
    try {
      // 嘗試調用後端登出 API（可選）
      if (token.value) {
        await api.post('/Auth/logout').catch(() => {
          // 忽略登出 API 錯誤，繼續執行登出流程
        })
      }
    } finally {
      token.value = null
      user.value = null
      isAuthenticated.value = false
      localStorage.removeItem('token')
    }
  }

  const checkAuth = async () => {
    if (!token.value) {
      isAuthenticated.value = false
      user.value = null
      return false
    }

    try {
      const response = await api.get('/Auth/me')
      if (response.data) {
        user.value = response.data
        isAuthenticated.value = true
        return true
      } else {
        logout()
        return false
      }
    } catch (error) {
      // Token 無效或過期
      logout()
      return false
    }
  }

  // 計算屬性：使用者名稱
  const username = computed(() => user.value?.username || null)

  return {
    isAuthenticated,
    user,
    token,
    username,
    login,
    logout,
    checkAuth
  }
})
