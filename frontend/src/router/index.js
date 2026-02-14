import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'Home',
      component: () => import('../views/Home.vue')
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/Login.vue'),
      meta: { requiresAuth: false }
    },
    {
      path: '/setup-totp',
      name: 'SetupTotp',
      component: () => import('../views/SetupTotp.vue'),
      meta: { requiresAuth: true }
    },
    {
      path: '/admin',
      name: 'Admin',
      component: () => import('../views/Admin.vue'),
      meta: { requiresAuth: true }
    }
  ]
})

// 路由守衛
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()

  // 如果路由需要認證
  if (to.meta.requiresAuth) {
    // 如果有 token 但尚未驗證，先檢查認證狀態
    if (authStore.token && !authStore.isAuthenticated) {
      const isAuthenticated = await authStore.checkAuth()
      if (!isAuthenticated) {
        next({ name: 'Login', query: { redirect: to.fullPath } })
        return
      }
    }
    
    // 如果未認證，跳轉到登入頁
    if (!authStore.isAuthenticated) {
      next({ name: 'Login', query: { redirect: to.fullPath } })
      return
    }
  }

  // 如果已登入且訪問登入頁，跳轉到管理頁面
  if (to.name === 'Login' && authStore.isAuthenticated) {
    next({ name: 'Admin' })
    return
  }

  next()
})

export default router
