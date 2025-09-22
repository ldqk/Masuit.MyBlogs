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

onBeforeUnmount(() => {
  // 清理滚动事件监听器
  document.querySelector('.q-page-container').removeEventListener('scroll', handleScroll)
})

// 滚动事件处理
const handleScroll = () => {
  if (editor.value?.$el == null) {
    return;
  }
  const toolbar = editor.value.$el.querySelector('.edui-editor-toolbarbox');
  if (editor.value.$el.getBoundingClientRect().top < 84 && editor.value.$el.getBoundingClientRect().bottom > 250) {
    if (!toolbar.style.top) {
      toolbar.style.position = 'fixed'
      toolbar.style.top = '84px'
      toolbar.style.zIndex = '2'
    }
  } else {
    if (toolbar.style.position) {
      toolbar.style.position = ''
      toolbar.style.top = ''
      toolbar.style.zIndex = ''
      toolbar.style.width = editor.value.$el.style.width;
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