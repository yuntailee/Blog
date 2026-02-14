import axios from 'axios'
import { useRouter } from 'vue-router'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json'
  },
  timeout: 10000 // 10 秒超時
})

// Request interceptor - 自動附加 token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor - 處理錯誤
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // 處理 401 未授權錯誤
    if (error.response?.status === 401) {
      localStorage.removeItem('token')
      // 只在非登入頁面時才跳轉
      if (window.location.pathname !== '/login') {
        window.location.href = '/login?redirect=' + encodeURIComponent(window.location.pathname)
      }
    }
    
    // 處理網路錯誤
    if (!error.response) {
      console.error('Network error:', error.message)
    }
    
    return Promise.reject(error)
  }
)

export default api
