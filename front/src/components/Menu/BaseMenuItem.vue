<!--
   动态菜单 item 递归实现
   myRouter ： 菜单列表
   initLevel ： 菜单初始化缩进等级
   bgColorLevel ：菜单背景色
   basePath : 上级菜单
-->
<template>
<div>
  <template v-for="(item, index) in props.myRouter">
    <template v-if="item.meta.isHidden !== true">
      <q-item-label v-if="item.meta.itemLabel" header class="text-weight-bold text-uppercase" :key="item.meta.itemLabel"> {{ item.meta.itemLabel }} </q-item-label>
      <!-- 没有孩子 -->
      <q-item v-if="!item.children" clickable v-ripple :key="'item-' + index" :exact="item.path === '/'" :class="baseItemClass" :inset-level="props.initLevel" :style="isWeChart ? ' line-height: normal' : ''" active-class="baseItemActive" :to="handleLink(props.basePath, item.path)" @click="externalLink(props.basePath, item.path)">
        <q-item-section avatar>
          <q-icon :name="item.meta.icon" />
        </q-item-section>
        <q-item-section> {{ item.meta.title }} </q-item-section>
      </q-item>
      <!-- 有孩子 -->
      <q-expansion-item v-else :duration="props.duration" :class="baseItemClassWithNoChildren(item.path)" :default-opened="item.meta.isOpen" :header-inset-level="props.initLevel" :key="'expand-' + index" :icon="item.meta.icon" :label="item.meta.title" :style="isWeChart ? ' line-height: normal' : ''">
        <!-- 菜单项缩进 + 0.2 ; 背景色深度 + 1 ; 如果上级菜单路径存在，则拼接上级菜单路径 -->
        <base-menu-item :my-router="item.children" :init-level="props.initLevel + 0.2" :bg-color-level="props.bgColorLevel + 1" :bg-color="props.bgColor" :base-path="props.basePath === undefined ? item.path : props.basePath + '/' + item.path" />
      </q-expansion-item>
    </template>
  </template>
</div>
</template>
<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'

defineOptions({
  name: 'base-menu-item'
})

// Props
const props = defineProps(['myRouter', 'initLevel', 'bgColor', 'bgColorLevel', 'duration', 'basePath'])

// 路由
const route = useRoute()

// 计算属性
const baseItemClass = computed(() => props.bgColor + '-' + props.bgColorLevel + ' base-menu-item')

/**
 * 处理子菜单被激活的样式，同时修改父菜单样式
 */
const baseItemClassWithNoChildren = computed(() => {
  return (path) => {
    return route.fullPath.startsWith(path) ? 'baseRootItemActive base-menu-item' + baseItemClass.value : baseItemClass.value
  }
})

/**
 * 如果是微信浏览器，则添加 line-height: normal 样式
 */
const isWeChart = computed(() => {
  return navigator.userAgent.toLowerCase().indexOf('micromessenger') !== -1
})

/**
 * 处理内部链接
 * @param basePath
 * @param itemPath
 */
const handleLink = (basePath, itemPath) => {
  const link = basePath === undefined ? itemPath : basePath + '/' + itemPath
  if (link.indexOf('http') !== -1) {
    return '#'
  }
  return link
}

/**
 * 处理外部链接
 * @param basePath
 * @param itemPath
 * @returns {boolean}
 */
const externalLink = (basePath, itemPath) => {
  const link = basePath === undefined ? itemPath : basePath + '/' + itemPath
  const i = link.indexOf('http')
  if (i !== -1) {
    const a = document.createElement('a')
    a.setAttribute('href', link.slice(i))
    a.setAttribute('target', '_blank')
    a.click()
    return false
  }
}
</script>
<style lang="css" scoped>
/* item 颜色 */
.base-menu-item {
  color: #2c3e50 !important;
}

/* item 被激活时父菜单的样式 */
.baseRootItemActive {
  color: #1976d2 !important;
}

/* item 被激活时的样式 */
.baseItemActive {
  color: #1976d2 !important;
  background: rgba(25, 118, 210, 0.0618);
  transition: all .618s;
}

.baseItemActive:after {
  content: '';
  position: absolute;
  width: 3px;
  height: 100%;
  background: #1976d2 !important;
  top: 0;
  right: 0;
}
</style>
