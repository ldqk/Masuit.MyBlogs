<template>
<div class="row" :style="{ margin: (!$q.screen.gt.sm ? '' : '0px 15px 0px 15px') }">
  <q-tabs align="left" active-color="primary" class="bg-white col-12" dense swipeable inline-label indicator-color="transparent" :outside-arrows="$q.platform.is.electron ? true : false" :breakpoint="0">
    <q-route-tab class="tagView" to="/" no-caps content-class="tagView-q-router-tab" :style="isWeChart ? ' line-height: normal' : ''">
      <div class="flex items-center">
        <q-icon size="1.3rem" name="home" />
        <div class="line-limit-length" style="margin: 0px 5px 0px 5px;">主页</div>
      </div>
    </q-route-tab>
    <q-route-tab v-for="(v, i) in tagView" :key="v.fullPath + i" class="tagView" :to="v.fullPath" no-caps content-class="tagView-q-router-tab" :style="isWeChart ? ' line-height: normal' : ''">
      <div class="flex items-center">
        <q-icon size="1.3rem" v-if="v.icon" :name="v.icon" />
        <div class="line-limit-length">{{ v.title }}</div>
        <q-icon class="tagView-remove-icon" style="display: inline-flex" name="close" @click.prevent.stop="removeAtagView(i)" />
        <q-menu touch-position context-menu>
          <q-list dense>
            <q-item clickable v-close-popup>
              <q-item-section @click="removeOthersTagView(i)"> 关闭其他 </q-item-section>
            </q-item>
            <q-item clickable v-close-popup>
              <q-item-section @click="removeRightTagView(i)"> 关闭右侧 </q-item-section>
            </q-item>
            <q-item clickable v-close-popup>
              <q-item-section @click="removeLeftTagView(i)"> 关闭左侧 </q-item-section>
            </q-item>
            <q-item clickable v-close-popup>
              <q-item-section @click="removeAllTagView"> 关闭所有 </q-item-section>
            </q-item>
          </q-list>
        </q-menu>
      </div>
    </q-route-tab>
  </q-tabs>
</div>
</template>
<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useMainStore } from '../../store/index'

// Store
const store = useMainStore()

// 响应式数据
const tagView = ref([])

// 计算属性
const getTagView = computed(() => store.getTagView)

/**
 * 如果是微信浏览器，则添加 line-height: normal 样式
 */
const isWeChart = computed(() => {
  return navigator.userAgent.toLowerCase().indexOf('micromessenger') !== -1
})

// 监听器
watch(getTagView, (n) => {
  tagView.value = n
  store.SET_KEEPALIVE_LIST(tagView.value)
  window.sessionStorage.setItem('tagView', JSON.stringify(tagView.value))
})

// 生命周期
onMounted(() => {
  tagView.value = store.getTagView
})

// 方法
const removeAllTagView = () => {
  store.REMOVE_TAG_VIEW()
}

const removeAtagView = (i) => {
  store.REMOVE_TAG_VIEW(i)
}

const removeLeftTagView = (i) => {
  store.REMOVE_TAG_VIEW({ side: 'left', index: i })
}

const removeRightTagView = (i) => {
  store.REMOVE_TAG_VIEW({ side: 'right', index: i })
}

const removeOthersTagView = (i) => {
  store.REMOVE_TAG_VIEW({ side: 'others', index: i })
}
</script>
<style lang="css">
/* 重置 quasar 内部 tab 样式 */
.tagView-q-router-tab {
  min-width: 40px !important;
}
</style>
<style lang="css" scoped>
.tagView {
  margin: 1.5px 3px 0 3px;
  min-height: 20px;
  padding: 0 8px;
  background: white;
  transition: all .5s;
  border-radius: 0;
  height: 31px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.tagView-remove-icon {
  font-size: 1.0rem;
  border-radius: .2rem;
  opacity: 0.58;
  transition: all .3s;
}

.tagView-remove-icon:hover {
  opacity: 1;
}

.line-limit-length {
  margin: 0px 5px 0px 7px;
  overflow: hidden;
  max-width: 180px;
  white-space: nowrap;
  text-overflow: ellipsis;
}
</style>
