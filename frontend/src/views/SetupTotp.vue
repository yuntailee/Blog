<template>
  <div class="max-w-md mx-auto mt-8">
    <h1 class="text-2xl font-bold mb-6">設定 Google Authenticator</h1>

    <div v-if="!setupData" class="text-center">
      <button
        @click="loadSetupData"
        class="bg-blue-500 text-white py-2 px-4 rounded-md hover:bg-blue-600"
      >
        開始設定
      </button>
    </div>

    <div v-else class="space-y-4">
      <div class="text-center">
        <p class="mb-4">請使用 Google Authenticator 掃描以下 QR Code：</p>
        <div ref="qrCodeRef" class="flex justify-center mb-4"></div>
        <p class="text-sm text-gray-600 mb-4">
          或手動輸入 Secret：<code class="bg-gray-100 px-2 py-1 rounded">{{ setupData.secret }}</code>
        </p>
      </div>

      <div>
        <label for="verifyCode" class="block text-sm font-medium mb-1">
          輸入驗證碼以確認設定
        </label>
        <input
          id="verifyCode"
          v-model="verifyCode"
          type="text"
          maxlength="6"
          pattern="[0-9]{6}"
          placeholder="000000"
          class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div v-if="error" class="text-red-600 text-sm">{{ error }}</div>
      <div v-if="success" class="text-green-600 text-sm">{{ success }}</div>

      <button
        @click="verifySetup"
        :disabled="loading || !verifyCode"
        class="w-full bg-blue-500 text-white py-2 px-4 rounded-md hover:bg-blue-600 disabled:opacity-50"
      >
        {{ loading ? '驗證中...' : '驗證並完成設定' }}
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import QRCode from 'qrcode'
import api from '../services/api'

const router = useRouter()
const setupData = ref(null)
const verifyCode = ref('')
const loading = ref(false)
const error = ref('')
const success = ref('')
const qrCodeRef = ref(null)

const loadSetupData = async () => {
  try {
    const response = await api.get('/auth/setup')
    setupData.value = response.data

    // Generate QR Code
    if (qrCodeRef.value && response.data.qrCodeUri) {
      QRCode.toCanvas(qrCodeRef.value, response.data.qrCodeUri, {
        width: 256,
        margin: 2
      }, (err) => {
        if (err) {
          console.error('QR Code generation error:', err)
          error.value = '無法生成 QR Code'
        }
      })
    }
  } catch (err) {
    error.value = err.response?.data?.message || '載入設定資料失敗'
  }
}

const verifySetup = async () => {
  if (!verifyCode.value || verifyCode.value.length !== 6) {
    error.value = '請輸入 6 位數驗證碼'
    return
  }

  loading.value = true
  error.value = ''
  success.value = ''

  try {
    await api.post('/auth/verify-setup', {
      secret: setupData.value.secret,
      code: verifyCode.value
    })
    success.value = 'TOTP 設定完成！'
    setTimeout(() => {
      router.push('/admin')
    }, 2000)
  } catch (err) {
    error.value = err.response?.data?.message || '驗證失敗'
  } finally {
    loading.value = false
  }
}
</script>
