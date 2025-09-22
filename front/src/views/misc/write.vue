<template>
<div class="misc-write-page">
  <q-form @submit="submit">
    <!-- 标题 -->
    <q-card flat bordered>
      <q-card-section>
        <div class="text-h6">杂项标题</div>
        <q-input dense v-model="misc.Title" placeholder="请输入杂项标题" outlined required :rules="[val => !!val || '请输入杂项标题']" />
        <div class="text-h6">杂项内容</div>
        <vue-ueditor-wrap ref="editorRef" v-model="misc.Content" />
        <div class="row q-gutter-md justify-end">
          <q-btn type="submit" color="primary" icon="send" :loading="loading" :disable="!misc.Title || !misc.Content"> {{ misc.Id ? '更新杂项' : '发布杂项' }} </q-btn>
          <q-btn color="grey" icon="arrow_back" @click="goBack" outline> 返回列表 </q-btn>
        </div>
      </q-card-section>
    </q-card>
  </q-form>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'

// 定义接口类型
interface Misc {
  Id?: number
  Title: string
  Content: string
}

interface ApiResponse {
  Success: boolean
  Message: string
  Data?: Misc
}

// 响应式数据
const route = useRoute()
const router = useRouter()
const editorRef = ref()
const loading = ref(false)

// 表单数据
const misc = ref<Misc>({
  Title: '',
  Content: ''
})

// 加载杂项数据
const loadMisc = async (id: number) => {
  loading.value = true
  const response = await api.get(`/misc/get/${id}`) as ApiResponse
  if (response?.Success && response.Data) {
    misc.value = response.Data
  } else {
    toast.error('获取杂项数据失败', { autoClose: 2000, position: 'top-center' })
  }
  loading.value = false
}

// 提交表单
const submit = async () => {
  if (!misc.value.Title || !misc.value.Content) {
    toast.error('请填写完整信息', { autoClose: 2000, position: 'top-center' })
    return
  }

  loading.value = true
  const url = misc.value.Id ? '/misc/edit' : '/misc/write'
  const submitData = misc.value.Id ? misc.value : {
    Title: misc.value.Title,
    Content: misc.value.Content
  }

  const response = await api.post(url, submitData) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
    router.push('/misc/list')
  } else {
    toast.error(response?.Message || '操作失败', { autoClose: 2000, position: 'top-center' })
  }
  loading.value = false

}

// 返回列表
const goBack = () => {
  router.push('/misc/list')
}

// 生命周期钩子
onMounted(() => {
  // 检查是否有编辑 ID
  const id = route.query.id
  if (id) {
    loadMisc(Number(id))
  }

  // 添加滚动事件监听
  const container = document.querySelector('.q-page-container')
  if (container) {
    container.addEventListener('scroll', handleScroll)
  }
})

// 组件卸载
onUnmounted(() => {
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
  if (editorRef.value?.$el == null) {
    return;
  }
  const toolbar = editorRef.value.$el.querySelector('.edui-editor-toolbarbox');
  if (toolbar) {
    const editorRect = editorRef.value.$el.getBoundingClientRect()
    const shouldFix = editorRect.top < 84 && editorRect.bottom > 300

    // 只在状态真正改变时才操作DOM
    if (shouldFix && !editorToolbarFixed) {
      // 固定工具栏
      const editorWidth = editorRef.value.$el.offsetWidth
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
<style scoped lang="scss">
.misc-write-page {
  padding: 20px;
  margin: 0 auto;
}

.editor-container {
  min-height: 700px;

  :deep(.edui-default) {
    line-height: normal;
  }
}

.q-card {
  margin-bottom: 16px;
}

// 响应式设计
@media (max-width: 768px) {
  .misc-write-page {
    padding: 10px;
  }

  .row.q-gutter-md {

    .col-md-6,
    .col-md-4 {
      margin-bottom: 16px;
    }
  }
}
</style>