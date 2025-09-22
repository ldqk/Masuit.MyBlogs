import { createRouter, createWebHistory } from 'vue-router'
import constantRoutes from './constantRoutes'
import asyncRoutes from './asyncRoutes'

// 合并所有路由
const allRoutes = [...constantRoutes, ...asyncRoutes]

// 重置路由方法
export function resetRouter(): void {
  // Vue Router 4 中使用 removeRoute 和 addRoute 方法重置路由
  router.getRoutes().forEach((route: any) => {
    if (route.name) {
      router.removeRoute(route.name)
    }
  })
  allRoutes.forEach(route => {
    router.addRoute(route as any)
  })
}

// 定义创建路由方法，方便重置路由时调用
const createRouterInstance = () => createRouter({
  history: createWebHistory(process.env.BASE_URL || '/'),
  routes: allRoutes as any
})

const router = createRouterInstance()

export default router