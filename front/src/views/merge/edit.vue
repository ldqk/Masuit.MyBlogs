<template>
<div class="merge-edit-container">
  <div class="text-h5">编辑并合并文章</div>
  <!-- 文章标题 -->
  <q-input v-model="post.Title" label="文章标题" outlined dense :rules="[val => !!val || '请输入文章标题']" lazy-rules />
  <!-- 文章内容 -->
  <div>
    <label class="text-subtitle2 q-mb-sm block">文章内容：</label>
    <vue-ueditor-wrap ref="editor" v-model="post.Content" />
  </div>
  <!-- 操作按钮 -->
  <div class="q-mt-md">
    <q-btn type="submit" color="primary" label="确认合并" :loading="loading" size="lg" @click="onSubmit" />
  </div>
  <q-inner-loading :showing="loading">
    <q-spinner color="primary" size="100px" />
  </q-inner-loading>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { toast } from 'vue3-toastify'
import { useRouter, useRoute } from 'vue-router'
import api from '@/axios/AxiosConfig'

const editor = ref(null);
// 类型定义
interface ApiResponse {
  Data?: any
  Message?: string
  Success?: boolean
}

interface PostData {
  Id: string
  Title: string
  Content: string
}

// 路由实例
const router = useRouter()
const route = useRoute()

// 响应式数据
const loading = ref(false)
const post = ref<PostData>({
  Id: '',
  Title: '',
  Content: ''
})

// 方法
const loadPostData = async () => {
  loading.value = true
  const mergeId = route.query.id as string
  if (!mergeId) {
    toast.error('缺少合并ID参数', { autoClose: 2000, position: 'top-center' })
    return
  }

  const data = await api.get(`/merge/${mergeId}`) as ApiResponse
  if (data?.Data) {
    post.value = data.Data
  }
  loading.value = false
}

// 提交表单
const onSubmit = async () => {
  if (!post.value.Title.trim()) {
    toast.error('请输入文章标题', { autoClose: 2000, position: 'top-center' })
    return
  }

  if (!post.value.Content.trim()) {
    toast.error('请输入文章内容', { autoClose: 2000, position: 'top-center' })
    return
  }

  loading.value = true
  const data = await api.post('/merge', post.value) as ApiResponse
  toast.success(data?.Message || '合并成功', { autoClose: 2000, position: 'top-center' })
  router.push('/posts/merge')
}

// 生命周期钩子
onMounted(async () => {
  await loadPostData()
  // 添加滚动事件监听器
  document.querySelector('.q-page-container').addEventListener('scroll', handleScroll)
})

// 组件卸载
onBeforeUnmount(() => {
  if (scrollDebounceTimer) {
    clearTimeout(scrollDebounceTimer)
  }
  // 清理滚动事件监听器
  document.querySelector('.q-page-container').removeEventListener('scroll', handleScroll)
})

let scrollDebounceTimer: any = null
let editorToolbarFixed = false
// 滚动事件处理
const handleScroll = () => {
  // 清除之前的防抖定时器
  if (scrollDebounceTimer) {
    clearTimeout(scrollDebounceTimer)
  }

  // 使用防抖，减少频繁执行
  scrollDebounceTimer = setTimeout(() => {
    handleScrollLogic()
  }, 100)
}

// 实际的滚动处理逻辑
const handleScrollLogic = () => {
  if (editor.value?.$el == null) {
    return;
  }
  const toolbar = editor.value.$el.querySelector('.edui-editor-toolbarbox');
  if (toolbar) {
    const editorRect = editor.value.$el.getBoundingClientRect()
    const shouldFix = editorRect.top < 84 && editorRect.bottom > 300

    // 只在状态真正改变时才操作DOM
    if (shouldFix && !editorToolbarFixed) {
      // 固定工具栏
      const editorWidth = editor.value.$el.offsetWidth
      toolbar.style.cssText = `
          position: fixed !important;
          top: 84px !important;
          z-index: 1000 !important;
          width: ${editorWidth}px !important;
          transition: all 0.5s ease !important;
        `
      editorToolbarFixed = true
    } else if (!shouldFix && editorToolbarFixed) {
      // 取消固定
      toolbar.style.cssText = `
          position: static !important;
          top: auto !important;
          z-index: auto !important;
          width: auto !important;
          transition: all 0.5s ease !important;
        `
      editorToolbarFixed = false
    }
  }
}
</script>
<style scoped>
.merge-edit-container {
  padding: 16px;
}

.editor-container {
  position: relative;
  border-radius: 4px;
}

.block {
  display: block;
}

@media (max-width: 768px) {
  .merge-edit-container {
    padding: 8px;
  }
}
</style>