<template><q-layout :view="viewStyle" class="full-height">
  <!-- 头部 -->
  <q-header class="q-py-xs bg-white text-grey-8" height-hint="48" style="box-shadow: rgba(0, 0, 0, 0.1) 0px 2px 12px 0px; padding-bottom: 2px">
    <!-- 状态栏 -->
    <q-toolbar style="margin-top: -5px;">
      <q-btn flat dense round aria-label="Menu" :icon="leftDrawerOpen === true ? 'menu_open' : 'menu'" @click="leftDrawerOpen = !leftDrawerOpen" />
      <!-- toolbar - title -->
      <toolbar-title />
      <!-- 面包屑 -->
      <breadcrumbs v-if="$q.screen.gt.sm" />
      <q-space />
      <!-- 右侧元素 -->
      <toolbar-item-right />
    </q-toolbar>
    <q-separator color="grey-3" />
    <!-- TAGVIEW -->
    <tag-view />
  </q-header>
  <!-- 侧滑菜单 -->
  <q-drawer v-model="leftDrawerOpen" show-if-above content-class="bg-white" :width="240">
    <base-menu />
  </q-drawer>
  <!-- 内容路由 -->
  <q-page-container class="app-main full-height">
    <router-view v-slot="{ Component }">
      <transition name="fade-transform" mode="out-in">
        <keep-alive :max="maxKeepAlive" :include="keepAliveList">
          <component :is="Component" :key="$route.fullPath" />
        </keep-alive>
      </transition>
    </router-view>
  </q-page-container>
</q-layout></template>
<script setup>
import { ref, computed, watch, getCurrentInstance } from 'vue'
import BaseMenu from '../components/Menu/BaseMenu'
import TagView from '../components/TagView/TagView'
import Breadcrumbs from '../components/Breadcrumbs/Breadcrumbs'
import ToolbarTitle from '../components/Toolbar/ToolbarTitle'
import ToolbarItemRight from '../components/Toolbar/ToolbarItemRight'
import { useMainStore } from '../store/index'

defineOptions({
  name: 'index'
})

// Store & Route & Instance
const store = useMainStore()
const instance = getCurrentInstance()

// 响应式数据
const viewStyle = ref(instance.appContext.config.globalProperties.$SildeBar)
const leftDrawerOpen = ref(false)
const maxKeepAlive = ref(instance.appContext.config.globalProperties.$Max_KeepAlive)
const keepAliveList = ref(store.getKeepAliveList)

// 计算属性
const getKeepAliveList = computed(() => store.getKeepAliveList)

// 监听器
watch(getKeepAliveList, (n) => {
  keepAliveList.value = n
})
</script>
