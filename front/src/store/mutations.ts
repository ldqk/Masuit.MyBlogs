import { State } from './state'
import router from '../router'
import { removeATagView, removeOneSide, RemovePayload } from '../components/TagView/TagViewUtils'

/**
 * Mutations 类型定义
 */
export interface Mutations {
  ADD_TAG_VIEW: (this: State, payload: any) => void
  SET_TAG_VIEW: (this: State, payload: any[]) => void
  REMOVE_TAG_VIEW: (this: State, payload?: number | RemovePayload) => void
  SET_BREADCRUMBS: (this: State, payload: any[]) => void
  SET_KEEPALIVE_LIST: (this: State, payload: any[]) => any[]
}

/**
 * 状态变更器
 */
const mutations: Mutations = {
  // 新增 tagView
  ADD_TAG_VIEW(this: State, payload: any): void {
    const size = this.tagView.length
    // 首次进入或刷新页面时，当前路由不是根路由
    if (!size && payload.fullPath !== '/') {
      this.tagView.push(payload)
      return
    }
    // 为了避免 tagView 重复添加。 构建一个以 fullPath 为标识的数组 t[]
    const t: string[] = []
    for (let i = 0; i < size; i++) {
      t.push(this.tagView[i].fullPath)
    }
    if (t.indexOf(payload.fullPath) === -1) {
      this.tagView.push(payload)
    }
  },

  SET_TAG_VIEW(this: State, payload: any[]): void {
    this.tagView = payload
  },

  /**
   * 移除 tagView
   * case 'undefined' : 移除所有 tagView
   * case 'object' : 移除某一侧 tagView
   * default '要删除元素的下标 i ' : 移除某一个 tagView
   *          如果移除的是第一个 tagView，则跳转到当前的第一个 tagView
   *          如果移除的是最后一个 tagView，则跳转到当前的最后一个 tagView
   * @param payload 移除参数
   */
  REMOVE_TAG_VIEW(this: State, payload?: number | RemovePayload): void {
    switch (typeof payload) {
      case 'undefined':
        this.tagView = []
        window.sessionStorage.setItem('tagView', '[]')
        router.push('/')
        break
      case 'object':
        removeOneSide(this, payload)
        break
      default:
        removeATagView(this, payload as number)
    }
  },

  // 设置面包屑
  SET_BREADCRUMBS(this: State, payload: any[]): void {
    this.breadcrumbs = payload
  },

  /**
   * 设置缓存列表
   * @param payload tagView[]
   */
  SET_KEEPALIVE_LIST(this: State, payload: any[]): any[] {
    this.keepAliveList = []
    for (let i = 0; i < payload.length; i++) {
      if (payload[i].keepAlive) {
        this.keepAliveList.push(payload[i].name)
      }
    }
    // 如果需要缓存首页，如下方所示，在方法最后 push 对应的路由组件名称即可
    // this.keepAliveList.push('home')
    return this.keepAliveList
  }

}

export default mutations