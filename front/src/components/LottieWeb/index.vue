<template>
<div ref="lottieBox"></div>
</template>
<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import lottie from 'lottie-web'

// Props
const props = defineProps(['animationData', 'path', 'loop', 'animationSpeed'])

// Emits
const emit = defineEmits(['isLottieFinish'])

// 响应式数据
const lottieBox = ref(null)
let lottieInstance = null

// 方法
const stop = () => {
  if (lottieInstance) {
    lottieInstance.stop()
  }
}

const play = () => {
  if (lottieInstance) {
    lottieInstance.play()
  }
}

const pause = () => {
  if (lottieInstance) {
    lottieInstance.pause()
  }
}

const onSpeedChange = () => {
  if (lottieInstance) {
    lottieInstance.setSpeed(props.animationSpeed)
  }
}

const isLottieFinish = () => {
  // lottieInstance.removeEventListener('data_ready', isLottieFinish)
  emit('isLottieFinish', true)
}

const initLottie = () => {
  lottieInstance = lottie.loadAnimation({
    container: lottieBox.value,
    renderer: 'svg',
    loop: props.loop || true,
    animationData: props.animationData,
    // 如果需要用到路径请求，请使用 path ，lottie 如果 animationData 为空 ，则自动选择 path
    path: props.path
  })

  lottieInstance.addEventListener('data_ready', isLottieFinish, { once: true })
}

// 生命周期
onMounted(() => {
  initLottie()
})

onBeforeUnmount(() => {
  if (lottieInstance) {
    lottieInstance.destroy()
    lottieInstance = null
  }
})

// 监听器
watch(() => props.animationSpeed, () => {
  onSpeedChange()
})

// 暴露方法
defineExpose({
  stop,
  play,
  pause
})
</script>
