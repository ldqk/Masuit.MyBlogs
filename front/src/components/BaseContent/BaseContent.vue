<template>
<div class="main-content">
  <q-scroll-area ref="scrollArea" :thumb-style="thumbStyle" :visible="false" style="height: 100%">
    <slot />
  </q-scroll-area>
</div>
</template>
<script setup>
import { ref, onMounted, onActivated, onDeactivated, onUnmounted } from 'vue'
import { useRoute } from 'vue-router'
import { thumbStyle } from './ThumbStyle'

// 定义 props
defineProps(['position'])

// 响应式数据
const scrollArea = ref(null)
// 标记当前 BaseContent 所在路由的页面
let BasePath = ''

// 当前路由
const route = useRoute()

// 滚动到指定位置
const ScrollToPosition = (e) => {
  scrollArea.value.setScrollPosition(e, 300)
}

// 获取位置，在使用前请做好节流或防抖处理
const getPosition = () => {
  return scrollArea.value.getScrollPosition()
}

// 组件挂载时
onMounted(() => {
  BasePath = route.path

  // 确保每个 BaseContent 有唯一的 BasePath
  Object.freeze(BasePath)

  // 如果页面被刷新，则从 sessionStorage 读取当前页面的滚动位置，
  // 可以打开浏览器窗口，看看 sessionStorage 有啥
  const t = window.sessionStorage.getItem(route.path)
  if (t) {
    const toPosition = JSON.parse(t)
    ScrollToPosition(toPosition.listScrollTop)
  }
})

/**
 * 当组件被 keep-alive 缓存时，切出路由会触发 deactivated 方法
 * 此时 BasePath 作为 key ，将滚动位置保存的 sessionStorage 中
 */
onDeactivated(() => {
  // console.log(`切换（from）：${BasePath}`)
  window.sessionStorage.setItem(BasePath, JSON.stringify({ listScrollTop: getPosition() }))
})

/**
 * 当组件被 keep-alive 缓存时，切回路由会触发 activated 方法
 * 此时从 sessionStorage 中获取滚动位置
 */
onActivated(() => {
  // console.log(`切换（to）：${route.path}`)
  const t = window.sessionStorage.getItem(route.path)
  if (t) {
    const toPosition = JSON.parse(t)
    ScrollToPosition(toPosition.listScrollTop)
  }
})

/**
 * 如果组件被关闭，则清除对应的 sessionStorage
 */
onUnmounted(() => {
  // console.log(`销毁：${BasePath}`)
  sessionStorage.removeItem(BasePath)
})

// 暴露方法给父组件使用
defineExpose({
  ScrollToPosition,
  getPosition
})
</script>
