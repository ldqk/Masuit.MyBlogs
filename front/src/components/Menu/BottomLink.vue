<template>
<div class="q-py-md q-px-md text-grey-9 ">
  <div class="row items-center q-gutter-x-sm q-gutter-y-xs">
    <div v-for="(button, index) in buttonList" :key="'button-' + index">
      <router-link v-if="button.URL && button.URL.indexOf('http') === -1" :to="button.URL" class="drawer-footer-link"> {{ button.text }} </router-link>
      <a v-else :href="button.URL" target="_blank" class="drawer-footer-link"> {{ button.text }} </a>
    </div>
  </div>
</div>
</template>
<script setup>
import { ref, onMounted, getCurrentInstance } from 'vue'

defineOptions({
  name: 'BottomLink'
})

const buttonList = ref([])

onMounted(() => {
  // 在 Vue 3 中使用 getCurrentInstance 获取全局属性
  const instance = getCurrentInstance()
  buttonList.value = instance.appContext.config.globalProperties.$buttonList
})
</script>
<style lang="css" scoped>
.drawer-footer-link {
  color: inherit;
  text-decoration: none;
  font-weight: 500;
  font-size: .75rem
}

.drawer-footer-link:hover {
  color: #000
}
</style>
