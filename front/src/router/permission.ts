// import { getCurrentInstance } from 'vue'
const { getCurrentInstance }: any = require('vue')
import router from './index'
import LoadingBar from '../components/LoadingBar/LoadingBar'
import { useMainStore } from '../store/index'
import { addTagView, setTagView } from '../components/TagView/TagViewUtils'
import { setBreadcrumbs } from '../components/Breadcrumbs/BreadcrumbsUtils'
import type { RouteLocationNormalized, NavigationGuardNext } from 'vue-router'

router.beforeEach((to: RouteLocationNormalized, from: RouteLocationNormalized, next: NavigationGuardNext) => {
  const store = useMainStore()
  // 处理 TagView 和面包屑导航
  handleTagViewAndBreadcrumbsAndKeepAlive(to, store)
  
  // 直接放行所有路由，无需登录验证
  next()
})

router.afterEach(() => {
  // 使用多个 stop() 来保证 LoadingBar 在动态添加路由后正确关闭
  LoadingBar.stop()
  LoadingBar.stop()
})

export default router

/**
 * 处理 tagView 和 面包屑
 * @param to 目标路由
 * @param store 状态管理实例
 */
function handleTagViewAndBreadcrumbsAndKeepAlive(to: RouteLocationNormalized, store: any): void {
  if (to.name != null) {
    const app = getCurrentInstance()
    document.title = to.meta.title + (app?.appContext?.config?.globalProperties?.$title || '')
    LoadingBar.start()
    
    // 跳过 404 错误页面和其他不需要 TagView 的页面
    if (to.path === '/NoFound404') {
      return
    }
    
    // 判断是否为刷新操作，如果是刷新操作则从 sessionStorage 获取保存的 tagView 信息
    let tagViewOnSS: any[] = []
    const storedTagView = window.sessionStorage.getItem('tagView')
    
    if (storedTagView === null) {
      window.sessionStorage.setItem('tagView', '[]')
    } else {
      tagViewOnSS = JSON.parse(storedTagView)
    }
    
    if (store.getTagView.length === 0 && tagViewOnSS.length !== 0) {
      setTagView(tagViewOnSS)
      store.SET_KEEPALIVE_LIST(tagViewOnSS)
    } else {
      addTagView(to as any)
    }
    
    setBreadcrumbs(to.matched as any, to.query)
    handleKeepAlive(to as any)
  }
}

/**
 * 处理多余的 layout : router-view，让当前组件保持在第一层 index : router-view 之下
 * 这个方法无法过滤用来做嵌套路由的按需加载的 <layout>
 * @param to 目标路由
 */
function handleKeepAlive(to: RouteLocationNormalized): void {
  if (to.matched && to.matched.length > 2) {
    for (let i = 0; i < to.matched.length; i++) {
      const element = to.matched[i]
      if (element.components?.default?.name === 'layout') {
        to.matched.splice(i, 1)
        handleKeepAlive(to)
        break
      }
    }
  }
}

/**
 * 这个方法可以过滤用来做嵌套路由的按需加载的 <layout>
 * @param to 目标路由
 */
// async function handleKeepAlive(to: RouteLocationNormalized): Promise<void> {
//   if (to.matched && to.matched.length > 2) {
//     for (let i = 0; i < to.matched.length; i++) {
//       const element = to.matched[i]
//       if (element.components?.default?.name === 'layout') {
//         to.matched.splice(i, 1)
//         await handleKeepAlive(to)
//       }
//       if (typeof element.components?.default === 'function') {
//         await element.components.default()
//         await handleKeepAlive(to)
//       }
//     }
//   }
// }