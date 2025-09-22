import { State } from './state'

/**
 * Getters 类型定义
 */
export interface Getters {
  getTagView: (state: State) => any[]
  getBreadcrumbs: (state: State) => any[]
  getKeepAliveList: (state: State) => any[]
}

/**
 * 状态获取器
 */
const getters: Getters = {
  getTagView: (state: State): any[] => { return state.tagView },
  getBreadcrumbs: (state: State): any[] => { return state.breadcrumbs },
  getKeepAliveList: (state: State): any[] => { return state.keepAliveList }
}

export default getters