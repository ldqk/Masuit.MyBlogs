// Quasar 组件和插件类型声明
interface QuasarPlugins {
  LoadingBar: any
  Dialog: any
  Notify: any
  Cookies: any
  TouchPan: any
  AppFullscreen: any
}

// 从 Quasar 导入组件和插件
const {
  LoadingBar,
  Dialog,
  Notify,
  Cookies,
  TouchPan,
  AppFullscreen
}: QuasarPlugins = require('quasar')

// 导入样式文件
import 'quasar/src/css/index.sass'
import '@quasar/extras/roboto-font/roboto-font.css'
import '@quasar/extras/material-icons/material-icons.css'
import '@quasar/extras/material-icons-outlined/material-icons-outlined.css'
import '@quasar/extras/material-icons-round/material-icons-round.css'
import '@quasar/extras/material-icons-sharp/material-icons-sharp.css'
import '@quasar/extras/fontawesome-v6/fontawesome-v6.css'

/**
 * Quasar 配置接口
 */
interface QuasarConfig {
  config: Record<string, any>
  plugins: QuasarPlugins
}

/**
 * Quasar 用户选项配置
 */
const quasarUserOptions: QuasarConfig = {
  config: {},
  plugins: {
    LoadingBar,
    Dialog,
    Notify,
    Cookies,
    TouchPan,
    AppFullscreen
  }
}

export default quasarUserOptions
export type { QuasarConfig, QuasarPlugins }