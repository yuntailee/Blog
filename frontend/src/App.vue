<template>
  <div id="app">
    <nav class="bg-gray-800 text-white p-4">
      <div class="container mx-auto flex justify-between items-center">
        <router-link to="/" class="text-xl font-bold">部落格</router-link>
        <div class="space-x-4">
          <router-link v-if="!authStore.isAuthenticated" to="/login" class="hover:text-gray-300">
            登入
          </router-link>
          <template v-else>
            <span>{{ authStore.user?.username }}</span>
            <button @click="handleLogout" class="hover:text-gray-300">登出</button>
          </template>
        </div>
      </div>
    </nav>
    <main class="container mx-auto p-4">
      <router-view />
    </main>
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useAuthStore } from './stores/auth'

const authStore = useAuthStore()

onMounted(() => {
  authStore.checkAuth()
})

const handleLogout = async () => {
  await authStore.logout()
  // 登出後跳轉到首頁
  window.location.href = '/'
}
</script>
