<template><iframe ref="counterFrame" :src="globalConfig.baseURL + '/filemanager'" width="100%" frameborder="0" scrolling="yes" @load="handleIframeLoad"></iframe></template>
<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { globalConfig } from '@/config'

const counterFrame = ref(null)

// 处理iframe加载完成
const handleIframeLoad = () => {
  // 由于跨域限制，我们无法访问iframe内容
  // 设置一个固定高度或通过postMessage与iframe通信
  if (counterFrame.value) {
    counterFrame.value.style.height = 'calc(100vh - 95px)'
  }
}
const messageHandler = (event) => {
  // 验证消息来源
  if (event.origin !== window.location.origin && !event.origin.includes('localhost')) {
    return
  }

  // 如果iframe发送高度信息
  if (event.data && event.data.type === 'resize' && event.data.height) {
    if (counterFrame.value) {
      counterFrame.value.style.height = event.data.height + 'px'
    }
  }
}
// 监听来自iframe的消息（如果iframe支持postMessage）
onMounted(() => {
  window.addEventListener('message', messageHandler)
})

onBeforeUnmount(() => {
  window.removeEventListener('message', messageHandler)
})
</script>