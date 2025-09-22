import VMdEditor from '@kangc/v-md-editor'
import '@kangc/v-md-editor/lib/style/base-editor.css'
import vuepressTheme from '@kangc/v-md-editor/lib/theme/vuepress.js'

VMdEditor.use(vuepressTheme)

// 导出VMdEditor供main.js使用
export default VMdEditor
