// Quasar LoadingBar 类型声明
interface LoadingBarOptions {
  color?: string
  size?: string
  position?: 'top' | 'bottom' | 'left' | 'right'
}

interface LoadingBarInterface {
  start: () => void
  stop: () => void
  setDefaults: (options: LoadingBarOptions) => void
}

// 从 Quasar 获取 LoadingBar
const { LoadingBar }: { LoadingBar: LoadingBarInterface } = require('quasar')

// 设置默认配置
LoadingBar.setDefaults({
  color: 'my-loadingBar-color',
  size: '2.3px',
  position: 'top'
})

export default LoadingBar
export type { LoadingBarInterface, LoadingBarOptions }