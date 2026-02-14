<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900">
          登入您的帳號
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600">
          使用 Google Authenticator 驗證碼登入
        </p>
      </div>
      <form @submit.prevent="handleLogin" class="mt-8 space-y-6 bg-white p-8 rounded-lg shadow-md">
        <div class="space-y-4">
          <div>
            <label for="username" class="block text-sm font-medium text-gray-700 mb-1">
              使用者名稱
            </label>
            <input
              id="username"
              v-model="username"
              type="text"
              required
              autocomplete="username"
              @blur="handleUsernameBlur"
              class="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm"
              placeholder="請輸入使用者名稱"
            />
          </div>
          <div>
            <label for="code" class="block text-sm font-medium text-gray-700 mb-1">
              TOTP 驗證碼
            </label>
            <input
              id="code"
              v-model="code"
              type="text"
              required
              autocomplete="one-time-code"
              maxlength="6"
              pattern="[0-9]{6}"
              placeholder="000000"
              class="appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm text-center text-2xl tracking-widest"
              @input="formatCode"
            />
            <p class="mt-2 text-xs text-gray-500">
              請輸入 Google Authenticator 中的 6 位數驗證碼
            </p>
          </div>
        </div>

        <div v-if="error" class="rounded-md bg-red-50 p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <p class="text-sm text-red-800">{{ error }}</p>
            </div>
          </div>
        </div>

        <div v-if="needsSetup" class="rounded-md bg-yellow-50 p-4">
          <div class="flex">
            <div class="flex-shrink-0">
              <svg class="h-5 w-5 text-yellow-400" viewBox="0 0 20 20" fill="currentColor">
                <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
              </svg>
            </div>
            <div class="ml-3">
              <p class="text-sm text-yellow-800">
                您尚未設定 TOTP。請先
                <router-link to="/setup-totp" class="font-medium text-yellow-900 underline hover:text-yellow-700">
                  設定 TOTP
                </router-link>
              </p>
            </div>
          </div>
        </div>

        <div>
          <button
            type="submit"
            :disabled="loading || !username || !code || code.length !== 6"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
          >
            <span v-if="loading" class="absolute left-0 inset-y-0 flex items-center pl-3">
              <svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            </span>
            {{ loading ? '登入中...' : '登入' }}
          </button>
        </div>

        <div class="text-center">
          <p class="text-xs text-gray-500">
            還沒有帳號？請聯繫管理員建立帳號
          </p>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const username = ref('')
const code = ref('')
const loading = ref(false)
const error = ref('')
const needsSetup = ref(false)

// 格式化驗證碼輸入（只允許數字）
const formatCode = (event) => {
  const value = event.target.value.replace(/\D/g, '')
  code.value = value.slice(0, 6)
  event.target.value = code.value
}

// 自動聚焦到驗證碼輸入框（當使用者名稱輸入完成時）
const handleUsernameBlur = () => {
  if (username.value && code.value.length === 0) {
    document.getElementById('code')?.focus()
  }
}

const handleLogin = async () => {
  // 驗證輸入
  if (!username.value.trim()) {
    error.value = '請輸入使用者名稱'
    return
  }

  if (!code.value || code.value.length !== 6) {
    error.value = '請輸入 6 位數驗證碼'
    return
  }

  loading.value = true
  error.value = ''
  needsSetup.value = false

  try {
    const result = await authStore.login(username.value.trim(), code.value)

    if (result.success) {
      // 登入成功，跳轉到目標頁面
      const redirect = route.query.redirect || '/admin'
      router.push(redirect)
    } else {
      error.value = result.message
      if (result.message.includes('TOTP not set up') || result.message.includes('TOTP')) {
        needsSetup.value = true
      }
      // 清空驗證碼，讓使用者重新輸入
      code.value = ''
      document.getElementById('code')?.focus()
    }
  } catch (err) {
    error.value = '登入時發生錯誤，請稍後再試'
    console.error('Login error:', err)
  } finally {
    loading.value = false
  }
}

// 頁面載入時檢查是否已登入
onMounted(() => {
  if (authStore.isAuthenticated) {
    router.push(route.query.redirect || '/admin')
  } else {
    // 檢查是否有 token 但未驗證
    authStore.checkAuth().then(() => {
      if (authStore.isAuthenticated) {
        router.push(route.query.redirect || '/admin')
      }
    })
  }
})
</script>
