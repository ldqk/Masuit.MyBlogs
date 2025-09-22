const { markRaw }: any = require("vue");
import { AppRouteRecord, RouteMeta } from "./constantRoutes";

// Layout 组件通过require导入
const Layout = require("../components/Layout/Layout.vue");

// 标记 Layout 组件为非响应式，避免性能问题
const LayoutComponent = markRaw(Layout.default || Layout);

/**
 * 扩展路由元信息接口
 */
interface ExtendedRouteMeta extends RouteMeta {
  itemLabel?: string;
  isOpen?: boolean;
}

/**
 * 异步路由记录接口
 */
interface AsyncRouteRecord extends Omit<AppRouteRecord, "meta" | "children"> {
  meta?: ExtendedRouteMeta;
  children?: AsyncRouteRecord[];
}

/**
 * 需要授权访问的路由
 */
const asyncRoutesChildren: AsyncRouteRecord[] = [
  {
    path: "/",
    name: "Home",
    meta: {
      roles: [],
      title: "概览",
      icon: "home",
      keepAlive: true,
    },
    component: () => import("@/views/home/Home.vue"),
  },
  {
    path: "/posts",
    name: "posts",
    meta: {
      roles: [],
      title: "文章管理",
      // itemLabel: 'SOME LABEL',
      icon: "library_music",
      // isOpen: true
    },
    component: LayoutComponent,
    children: [
      {
        path: "list",
        name: "list",
        meta: {
          roles: [],
          title: "文章列表",
          icon: "article",
          keepAlive: true,
        },
        component: () => import("@/views/posts/list.vue"),
      },
      {
        path: "write",
        name: "write",
        meta: {
          roles: [],
          title: "写文章",
          icon: "edit_note",
          keepAlive: true,
        },
        component: () => import("@/views/posts/write.vue"),
      },
      {
        path: "pending",
        name: "pending",
        meta: {
          roles: [],
          title: "待审核文章",
          icon: "hourglass_empty",
          keepAlive: true,
        },
        component: () => import("@/views/posts/post-pending.vue"),
      },
      {
        path: "merge",
        name: "merge",
        meta: {
          roles: [],
          title: "文章合并",
          icon: "extension",
          keepAlive: true,
        },
        component: () => import("@/views/merge/list.vue"),
      },
      {
        path: "merge/edit",
        name: "合并编辑",
        meta: {
          roles: [],
          title: "文章合并编辑",
          icon: "extension",
          keepAlive: true,
          isHidden: true,
        },
        component: () => import("@/views/merge/edit.vue"),
      },
      {
        path: "merge/compare",
        name: "合并对比",
        meta: {
          roles: [],
          title: "文章合并对比",
          icon: "extension",
          keepAlive: true,
          isHidden: true,
        },
        component: () => import("@/views/merge/compare.vue"),
      },
      {
        path: "category",
        name: "文章分类",
        meta: {
          roles: [],
          title: "文章分类管理",
          icon: "category",
          keepAlive: true,
        },
        component: () => import("@/views/posts/category.vue"),
      },
      {
        path: "seminar",
        name: "文章专题",
        meta: {
          roles: [],
          title: "文章专题管理",
          icon: "topic",
          keepAlive: true,
        },
        component: () => import("@/views/posts/seminar.vue"),
      },
      {
        path: "share",
        name: "快速分享",
        meta: {
          roles: [],
          title: "快速分享",
          icon: "share",
          keepAlive: true,
        },
        component: () => import("@/views/posts/share.vue"),
      },

    ],
  },
  {
    path: "/audit",
    name: "audit",
    meta: {
      roles: [],
      title: "审核管理",
      icon: "gavel",
    },
    component: LayoutComponent,children: [
      {
        path: "comments",
        name: "评论审核",
        meta: {
          roles: [],
          title: "评论审核",
          icon: "comment",
          keepAlive: true,
        },
        component: () => import("@/views/audit/comments.vue"),
      },
      {
        path: "msgs",
        name: "留言审核",
        meta: {
          roles: [],
          title: "留言审核",
          icon: "message",
          keepAlive: true,
        },
        component: () => import("@/views/audit/msgs.vue"),
      }
    ],
  },
  {
    path: "/notice",
    name: "notice",
    meta: {
      roles: [],
      title: "公告管理",
      icon: "notifications",
      isOpen: false,
    },
    component: LayoutComponent,
    children: [
      {
        path: "list",
        name: "notice-list",
        meta: {
          roles: ["admin", "editor"],
          title: "公告列表",
          icon: "list",
          keepAlive: true,
        },
        component: () => import("@/views/notice/list.vue"),
      },
      {
        path: "write",
        name: "notice-write",
        meta: {
          roles: ["admin", "editor"],
          title: "编辑公告",
          icon: "edit",
          keepAlive: false,
        },
        component: () => import("@/views/notice/write.vue"),
      },
    ],
  },
  {
    path: "/misc",
    name: "misc",
    meta: {
      roles: [],
      title: "杂项管理",
      icon: "miscellaneous_services",
      isOpen: false,
    },
    component: LayoutComponent,
    children: [
      {
        path: "list",
        name: "misc-list",
        meta: {
          roles: ["admin", "editor"],
          title: "杂项列表",
          icon: "list",
          keepAlive: true,
        },
        component: () => import("@/views/misc/list.vue"),
      },
      {
        path: "write",
        name: "misc-write",
        meta: {
          roles: ["admin", "editor"],
          title: "新建杂项页",
          icon: "edit",
          keepAlive: false,
        },
        component: () => import("@/views/misc/write.vue"),
      },
    ],
  },
  {
    path: "/filemanager",
    name: "文件管理器",
    meta: {
      roles: [],
      title: "文件管理器",
      icon: "folder",
      keepAlive: true,
    },
    component: () => import("@/views/home/files.vue"),
  },
  {
    path: "/system",
    name: "system",
    meta: {
      roles: [],
      title: "系统管理",
      icon: "settings",
      isOpen: false,
    },
    component: LayoutComponent,
    children: [
      {
        path: "settings",
        name: "system-settings",
        meta: {
          roles: ["admin"],
          title: "系统设置",
          icon: "tune",
          keepAlive: true,
        },
        component: () => import("@/views/system/settings.vue"),
      },
      {
        path: "menus",
        name: "菜单管理",
        meta: {
          roles: ["admin"],
          title: "菜单管理",
          icon: "menu",
          keepAlive: true,
        },
        component: () => import("@/views/menus/index.vue"),
      },
      {
        path: "firewall",
        name: "system-firewall",
        meta: {
          roles: ["admin"],
          title: "防火墙管理",
          icon: "security",
          keepAlive: true,
        },
        component: () => import("@/views/system/firewall.vue"),
      },
      {
        path: "links",
        name: "友情链接",
        meta: {
          roles: ["admin"],
          title: "友情链接",
          icon: "link",
          keepAlive: true,
        },
        component: () => import("@/views/system/links.vue"),
      },
      {
        path: "logs",
        name: "system-logs",
        meta: {
          roles: ["admin"],
          title: "系统日志",
          icon: "description",
          keepAlive: true,
        },
        component: () => import("@/views/system/logs.vue"),
      },
      {
        path: "email-templates",
        name: "system-email-templates",
        meta: {
          roles: ["admin"],
          title: "邮件/页面模板",
          icon: "email",
          keepAlive: true,
        },
        component: () => import("@/views/system/email-templates.vue"),
      },
      {
        path: "sendbox",
        name: "system-sendbox",
        meta: {
          roles: ["admin"],
          title: "邮件记录",
          icon: "send",
          keepAlive: true,
        },
        component: () => import("@/views/system/sendbox.vue"),
      },
      {
        path: "values",
        name: "模板变量",
        meta: {
          roles: ["admin"],
          title: "模板变量",
          icon: "view_list",
          keepAlive: true,
        },
        component: () => import("@/views/system/values.vue"),
      },
    ],
  },
  {
    path: "/partners",
    name: "广告管理",
    meta: {
      roles: [],
      title: "广告管理",
      icon: "campaign",
      keepAlive: true,
    },
    component: () => import("@/views/advertisement/index.vue"),
  },
  {
    path: "/donate",
    name: "打赏管理",
    meta: {
      roles: [],
      title: "打赏管理",
      icon: "monetization_on",
      keepAlive: true,
    },
    component: () => import("@/views/donate/index.vue"),
  },
  {
    path: "/users",
    name: "用户管理",
    meta: {
      roles: [],
      title: "用户管理",
      icon: "people",
      keepAlive: true,
    },
    component: () => import("@/views/user/index.vue"),
  },
  {
    path: "/search",
    name: "搜索记录分析",
    meta: {
      roles: [],
      title: "搜索记录分析",
      icon: "search",
      keepAlive: true,
    },
    component: () => import("@/views/search/index.vue"),
  },
  {
    path: "/task-center",
    name: "定时任务监控",
    meta: {
      roles: [],
      title: "定时任务监控",
      icon: "schedule",
      keepAlive: true,
    },
    component: () => import("@/views/jobs/index.vue"),
  },
  {
    path: "/msgs",
    name: "站内消息",
    meta: {
      roles: [],
      title: "站内消息",
      icon: "message",
      keepAlive: true,
      isHidden: true,
    },
    component: () => import("@/views/msgs/index.vue"),
  },
  {
    path: "/loginrecord",
    name: "登录记录",
    meta: {
      roles: [],
      title: "登录记录",
      icon: "history",
      keepAlive: true,
      isHidden: true,
    },
    component: () => import("@/views/user/loginrecord.vue"),
  },
  {
    path: "/:pathMatch(.*)*", // Vue Router 4 的通配符语法
    redirect: "/NoFound404",
    meta: {
      roles: ["admin", "test"],
      isHidden: true,
    },
  },
];

const asyncRoutes: AsyncRouteRecord[] = [
  {
    path: "/",
    name: "index",
    redirect: "/",
    component: () => import("@/views/Index.vue"),
    children: asyncRoutesChildren,
  },
];

export default asyncRoutes;
export { asyncRoutesChildren };
export type { AsyncRouteRecord, ExtendedRouteMeta };
