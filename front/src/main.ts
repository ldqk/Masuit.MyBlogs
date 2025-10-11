const { createApp }: any = require("vue");
import { Quasar } from "quasar";
import quasarUserOptions from "./quasar";
import App from "./App.vue";
import store from "./store";

// 前端根据角色处理路由
import router from "./router/permission";

// 后端返回动态路由
// import router from './router/permissionWithDynamicRouter'

// 系统全局配置
import { globalConfig } from "./config";

// 第三方组件
import "animate.css";

// vxe-table
import VxeUIAll from "vxe-pc-ui";
import "vxe-pc-ui/es/style.css";
import XEUtils from 'xe-utils'

import VxeUITable from "vxe-table";
import "vxe-table/es/style.css";

// dayjs
import dayjs from "dayjs";
import "dayjs/locale/zh-cn";
import relativeTime from "dayjs/plugin/relativeTime";
import utc from "dayjs/plugin/utc";
import timezone from "dayjs/plugin/timezone";

// 配置dayjs
dayjs.locale("zh-cn");
dayjs.extend(relativeTime);
dayjs.extend(utc);
dayjs.extend(timezone);

// markdown
import VMdEditor from "./components/Markdown/Markdown";

// vue-ueditor-wrap
import VueUeditorWrap from "vue-ueditor-wrap";

// 全局属性已在 shims-vue.d.ts 中定义

declare global {
  interface Window {
    _hmt: any[];
  }
}

const app = createApp(App);

// 设置全局属性
app.config.globalProperties.$PUBLIC_PATH = process.env.BASE_URL || "/";
app.config.globalProperties.$title = globalConfig.title;
app.config.globalProperties.$SildeBar = globalConfig.SildeBar;
app.config.globalProperties.$baseURL = globalConfig.baseURL;
app.config.globalProperties.$timeOut = globalConfig.timeOut;
app.config.globalProperties.$Max_KeepAlive = globalConfig.Max_KeepAlive;
app.config.globalProperties.$dayjs = dayjs;
VxeUIAll.formats.add('formatDate', {
  cellFormatMethod({ cellValue }, format?: string) {
    return XEUtils.toDateString(cellValue, format || 'yyyy-MM-dd HH:mm:ss')
  }
})
// 注册全局组件
app
  .use(Quasar, quasarUserOptions)
  .use(VueUeditorWrap)
  .use(store)
  .use(router)
  .use(VMdEditor)
  .use(VxeUIAll)
  .use(VxeUITable)
  .mount("#app");

export default app;
