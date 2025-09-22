declare module '*.vue' {
  const component: any
  export default component
}

// 全局属性扩展
declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    $PUBLIC_PATH: string
    $title: string
    $SildeBar: boolean
    $baseURL: string
    $timeOut: number
    $Max_KeepAlive: number
    $buttonList: string[]
    $q: any
    $dayjs: typeof import('dayjs')
  }
}

export {}