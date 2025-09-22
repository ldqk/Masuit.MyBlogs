import { useMainStore } from '../../store/index'
import router from '../../router'
import { getFirst } from '../../utils/CloneUtils'

/**
 * TagView 对象接口
 */
interface TagView {
  fullPath: string
  name: string | null | undefined
  title: string
  icon?: string
  keepAlive: boolean
}

/**
 * 路由对象接口
 */
interface Route {
  fullPath: string
  name?: string | null
  meta: {
    title: string
    icon?: string
    keepAlive?: boolean
  }
  query: Record<string, any>
}

/**
 * 移除操作参数接口
 */
interface RemovePayload {
  side: 'left' | 'right' | 'others'
  index: number
}

/**
 * State 接口
 */
interface State {
  tagView: TagView[]
}

/**
 * 构造 tag-view 的元信息，如果符合条件( 不是公共路由 )就提交到 store，生成 tagView 元素
 * @param to 目标路由
 */
export function addTagView(to: Route): void {
  const store = useMainStore()
  // 构造临时 tagView 对象
  const t: TagView = {
    fullPath: to.fullPath,
    name: to.name,
    title: to.meta.title,
    icon: to.meta.icon,
    keepAlive: to.meta.keepAlive || false
  }
  
  const firstQueryValue = getFirst(to.query)
  if (firstQueryValue !== undefined) {
    t.title += '：' + firstQueryValue
  }
  
  if (t.title !== null && t.title !== undefined && t.fullPath !== '/' && t.fullPath.indexOf('#') === -1) {
    store.ADD_TAG_VIEW(t)
  }
}

/**
 * 设置 TagView
 * @param tagView TagView 数组
 */
export function setTagView(tagView: TagView[]): void {
  const store = useMainStore()
  store.SET_TAG_VIEW(tagView)
}

/**
 * 只移除一个 tagView
 * @param state 状态对象
 * @param payload 要移除的索引
 */
export function removeATagView(state: State, payload: number): void {
  // 记录被移除的路由
  const removedTagView = state.tagView[payload].fullPath
  state.tagView.splice(payload, 1)
  
  // 如果移除后， tagView 为空
  if (state.tagView.length === 0) {
    window.sessionStorage.setItem('tagView', '[]')
    router.push('/')
  } else {
    // 如果移除的是最后一个 tagView 则路由跳转移除后的最后一个 tagView
    if (payload === state.tagView.length && window.location.href.indexOf(removedTagView) !== -1) {
      router.push(state.tagView[payload - 1].fullPath)
      return
    }
    // 如果移除的是第一个 tagView 则路由跳转移除后的第一个 tagView
    if (payload === 0 && window.location.href.indexOf(removedTagView) !== -1) {
      router.push(state.tagView[0].fullPath)
      return
    }
    if (window.location.href.indexOf(removedTagView) !== -1) {
      router.push(state.tagView[payload - 1].fullPath)
    }
  }
}

/**
 * 移除某一侧 tagView
 * @param state 状态对象
 * @param payload 移除参数
 */
export function removeOneSide(state: State, payload: RemovePayload): void {
  switch (payload.side) {
    case 'right':
      state.tagView = state.tagView.slice(0, payload.index + 1)
      if (state.tagView.length === 1) {
        router.push(state.tagView[0].fullPath)
      }
      if (state.tagView.length === payload.index + 1) {
        router.push(state.tagView[payload.index].fullPath)
      }
      break
    case 'left':
      state.tagView = state.tagView.slice(payload.index, state.tagView.length)
      if (state.tagView.length === 1) {
        router.push(state.tagView[0].fullPath)
      }
      if (state.tagView.length <= payload.index) {
        router.push(state.tagView[0].fullPath)
      }
      break
    case 'others':
      state.tagView = state.tagView.splice(payload.index, 1)
      router.push(state.tagView[0].fullPath)
      break
    default:
      break
  }
}

export type { TagView, Route, RemovePayload, State }