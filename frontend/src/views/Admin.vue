<template>
  <div>
    <h1 class="text-2xl font-bold mb-4">管理後台</h1>
    <p class="text-gray-600">歡迎，{{ authStore.user?.username }}！</p>
    <p class="mt-4">這裡是管理後台，未來可以在這裡管理文章、留言等內容。</p>
  </div>
</template>

<script setup>
import { useAuthStore } from '../stores/auth'

const authStore = useAuthStore()
</script>
