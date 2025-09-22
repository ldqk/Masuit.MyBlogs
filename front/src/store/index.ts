import { createPinia, defineStore } from 'pinia'
import state, { State } from './state'
import getters from './getters'
import mutations from './mutations'
import actions from './actions'

/**
 * 创建主 Pinia store
 */
export const useMainStore = defineStore('main', {
  state: (): State => ({ ...state }),
  getters: {
    getTagView: (state: State) => state.tagView,
    getBreadcrumbs: (state: State) => state.breadcrumbs,
    getKeepAliveList: (state: State) => state.keepAliveList
  },
  actions: {
    ...actions,
    // 合并 mutations 到 actions 中（移除 this 参数类型）
    ADD_TAG_VIEW(payload: any) {
      return mutations.ADD_TAG_VIEW.call(this, payload)
    },
    SET_TAG_VIEW(payload: any[]) {
      return mutations.SET_TAG_VIEW.call(this, payload)
    },
    REMOVE_TAG_VIEW(payload?: number | any) {
      return mutations.REMOVE_TAG_VIEW.call(this, payload)
    },
    SET_BREADCRUMBS(payload: any[]) {
      return mutations.SET_BREADCRUMBS.call(this, payload)
    },
    SET_KEEPALIVE_LIST(payload: any[]) {
      return mutations.SET_KEEPALIVE_LIST.call(this, payload)
    }
  }
})

/**
 * 创建 Pinia 实例
 */
export default createPinia()