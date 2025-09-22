/**
 * 状态接口定义
 */
export interface State {
  tagView: any[]
  breadcrumbs: any[]
  keepAliveList: any[]
}

/**
 * 应用状态
 */
const state: State = {
  tagView: [],
  breadcrumbs: [],
  keepAliveList: []
}

export default state