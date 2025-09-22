const { markRaw }: any = require('vue')
import Layout from '../components/Layout/Layout.vue'
import { AsyncRouteRecord } from './asyncRoutes'

// 标记 Layout 组件为非响应式，避免性能问题
const LayoutComponent = markRaw(Layout)

/**
 * 用于根据用户角色，生成过滤后的路由
 * 构造符合权限的路由，将不符合权限的路由过滤 （广度优先）
 * @param router 未过滤的路由 因为最外层有一层用来做布局的 {index} 路由，{index} 路由不参与权限过滤，所以传的是 index[0].children
 * @param t 暂存变量
 * @returns 过滤后的路由
 */
export default function constructionRouters(router: AsyncRouteRecord[], t?: AsyncRouteRecord[]): AsyncRouteRecord[] {
  // 移除权限验证，直接返回所有路由
  t = [...router]
  for (const item of t) {
    if (item.children) {
      item.children = constructionRouters(item.children)
    }
  }
  return t
}

/**
 * 将后端传入的 router 字符串转化为 vue-router 可用的对象
 * @param jsonRouter 后台路由
 * @param t 占存变量，用于返回值（不需要传参）
 */
export function handleJsonRouterToAsyncRouter(jsonRouter: any[], t?: any[]): any[] {
  t = jsonRouter.map(item => {
    if (item.component === 'Layout') {
      item.component = LayoutComponent
    } else {
      // 组件懒加载
      item.component = handleComponent(item.component)
    }
    return item
  })
  for (const item of t) {
    if (item.children) {
      item.children = handleJsonRouterToAsyncRouter(item.children)
    }
  }
  return t
}

function handleComponent(component: string): () => Promise<any> {
  // 路由懒加载
  return () => import('@/' + component)
}

/**
 * 将 vue 路由转换为 json 字符串
 * 将 asyncRouters 的 roles 初始化为空，同时处理 Component 的懒加载：
 * component: () => import('../views/home/Home') 转换为 component: 'views/home/Home'
 * @param asyncRouters 异步路由数组
 * @returns 处理后的 asyncRouters JSON 字符串
 */
export async function handleAsyncRouterToJson(asyncRouters: any[]): Promise<any> {
  // 先把路由树中的每个节点的 roles 重置为 []，即每个人都可以访问
  for (const item of asyncRouters) {
    if (item.meta) {
      item.meta.roles = []
    }
    if (item.children) {
      await handleAsyncRouterToJson(item.children)
    }
    // 当遍历到通配符路由时，说明遍历完成，接着去处理 Component 的懒加载
    if (item.path === '/:pathMatch(.*)*' || item.path === '*') {
      return await handleAsyncRouterComponentToJson(asyncRouters)
    }
  }
}

/**
 * 处理 Component 的懒加载
 * component: () => import('../views/home/Home') 转换为 component: 'views/home/Home'
 * @param asyncRouters 异步路由数组
 * @returns 处理后的路由数组
 */
export async function handleAsyncRouterComponentToJson(asyncRouters: any[]): Promise<any[]> {
  for (const item of asyncRouters) {
    // 如果 component 是懒加载，则执行它
    if (typeof item.component === 'function') {
      try {
        const c = await item.component()
        // 使用正则表达式将地址中的 src 替换
        if (c.default?.__file) {
          item.component = c.default.__file.replace(/src\//, '')
        }
      } catch (error) {
        console.warn('Failed to load component:', error)
        item.component = 'UnknownComponent'
      }
    } else if (item.component && typeof item.component === 'object' && item.component.name) {
      item.component = item.component.name
    }
    if (item.children) {
      await handleAsyncRouterComponentToJson(item.children)
    }
    // 当遍历到通配符路由时，说明遍历完成
    if (item.path === '/:pathMatch(.*)*' || item.path === '*') {
      // 去除 404 路由，在新增路由时自动添加
      asyncRouters.pop()
      return asyncRouters
    }
  }
  return asyncRouters
}

/**
 * 用于添加新角色和对应的路由
 * 使用 selectedRouter 过滤 baseRouter 中的路由，得到新的角色路由（广度优先）
 * @param baseRouter 基础路由
 * @param selectedRouter 被选中的路由标识数组
 * @param t 暂存变量
 * @returns 处理后的角色路由
 */
export function handleBaseRouterToRolesRouter(baseRouter: any[], selectedRouter: string[], t?: any[]): any[] {
  t = baseRouter.filter(item => {
    return selectedRouter.some(s => {
      return s === item.name
    })
  })
  for (const item of t) {
    if (item.children) {
      item.children = handleBaseRouterToRolesRouter(item.children, selectedRouter)
    }
  }
  return t
}