import { useMainStore } from '../../store/index'
import deepClone, { getFirst } from '../../utils/CloneUtils'

/**
 * 路由匹配对象接口
 */
interface RouteMatched {
  meta: RouteMeta
}

/**
 * 路由元信息接口
 */
interface RouteMeta {
  title: string
  icon?: string
  [key: string]: any
}

/**
 * 获取 matched 中的路径 title，并生成面包屑
 * @param matched 路由匹配数组
 * @param query 查询参数
 */
export function setBreadcrumbs(matched: RouteMatched[], query: Record<string, any>): void {
  const store = useMainStore()
  const temp: RouteMeta[] = []
  
  for (let i = 0; i < matched.length; i++) {
    temp.push(deepClone(matched[i].meta))
  }
  
  const last = temp.length - 1
  // 如果有 query 则取第一个参数附加在 title 上
  if (Object.keys(query).length && temp[last]) {
    const firstQueryValue = getFirst(query)
    if (firstQueryValue) {
      temp[last].title += '：' + firstQueryValue
    }
  }
  
  store.SET_BREADCRUMBS(temp)
}

export type { RouteMatched, RouteMeta }