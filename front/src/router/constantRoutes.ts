import { RouteRecordRaw } from 'vue-router'

/**
 * 路由元信息接口
 */
interface RouteMeta {
  title?: string
  icon?: string
  isHidden?: boolean
  roles?: string[]
  keepAlive?: boolean
}

/**
 * 扩展的路由记录接口
 */
interface AppRouteRecord extends Omit<RouteRecordRaw, 'meta' | 'children'> {
  meta?: RouteMeta
  children?: AppRouteRecord[]
}

/**
 * 公共路由
 */
const constantRoutes: AppRouteRecord[] = [
  {
    path: '/NoFound404',
    name: 'NoFound404',
    meta: {
      title: '404',
      icon: 'sentiment_dissatisfied',
      isHidden: true
    },
    component: () => import('@/components/404/NoFound404.vue')
  }
]

export default constantRoutes
export type { AppRouteRecord, RouteMeta }