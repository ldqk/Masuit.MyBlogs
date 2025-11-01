<template>
<div class="notice-write-page">
  <q-form @submit="submit">
    <!-- 标题 -->
    <q-card flat bordered>
      <q-card-section>
        <div class="text-h6">公告标题</div>
        <q-input dense v-model="notice.Title" placeholder="请输入公告标题" outlined required :rules="[val => !!val || '请输入公告标题']" />
        <div class="text-h6">公告内容</div>
        <vue-ueditor-wrap ref="editor" v-model="notice.Content" :config="{ initialFrameHeight: 250 }" />
      </q-card-section>
    </q-card>
    <!-- 设置区域 -->
    <q-card flat bordered>
      <q-card-section>
        <div class="text-h6 q-mb-md">公告设置</div>
        <!-- 时间范围选择 -->
        <div class="row q-gutter-md items-center">
          <div class="col">
            <q-input dense v-model="timeRangeString" label="生效时间段" outlined readonly>
              <template v-slot:prepend>
                <q-icon name="schedule" />
              </template>
              <template v-slot:append>
                <q-icon name="event" class="cursor-pointer">
                  <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                    <q-date v-model="timeRange" range :options="dateOptions" today-btn>
                      <div class="row items-center justify-end">
                        <q-btn v-close-popup label="确定" color="primary" flat />
                      </div>
                    </q-date>
                  </q-popup-proxy>
                </q-icon>
                <q-btn flat round dense icon="clear" @click.stop="clearTimeRange" v-if="timeRangeString" />
              </template>
            </q-input>
          </div>
          <!-- 弹窗设置 -->
          <div class="col">
            <q-checkbox v-model="notice.StrongAlert" label="是否弹窗显示" color="primary" />
          </div>
          <div class="col text-right">
            <q-btn-group>
              <q-btn type="submit" color="primary" icon="send" :loading="loading" :disable="!notice.Title || !notice.Content"> {{ notice.Id ? '更新公告' : '发布公告' }} </q-btn>
              <q-btn color="grey" icon="arrow_back" @click="goBack" outline> 返回列表 </q-btn>
            </q-btn-group>
          </div>
        </div>
      </q-card-section>
    </q-card>
  </q-form>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'
import { init } from 'echarts'

// 定义接口类型
interface Notice {
  Id?: number
  Title: string
  Content: string
  StartTime?: string
  EndTime?: string
  StrongAlert: boolean
}

interface ApiResponse {
  Success: boolean
  Message: string
  Data?: Notice
}

// 响应式数据
const route = useRoute()
const router = useRouter()
const editor = ref()
const loading = ref(false)

// 表单数据
const notice = ref<Notice>({
  Title: '',
  Content: '',
  StrongAlert: false
})

// 时间相关
const timeRange = ref({ from: '', to: '' })
const timeRangeString = computed(() => {
  if (timeRange.value.from && timeRange.value.to) {
    const start = timeRange.value.from
    const end = timeRange.value.to
    return `${start} - ${end}`
  }
  return ''
})

// 加载公告数据
const loadNotice = async (id: number) => {
  loading.value = true
  const response = await api.get(`/notice/get/${id}`) as ApiResponse

  if (response?.Success && response.Data) {
    notice.value = response.Data

    // 处理时间范围显示
    if (response.Data.StartTime && response.Data.EndTime) {
      timeRange.value = { from: response.Data.StartTime, to: response.Data.EndTime }
    }
  } else {
    toast.error('获取公告数据失败', { autoClose: 2000, position: 'top-center' })
  }
  loading.value = false

}

// 清除时间范围
const clearTimeRange = () => {
  timeRange.value = { from: '', to: '' }
  notice.value.StartTime = undefined
  notice.value.EndTime = undefined
}

// 限制只能选择未来日期
const dateOptions = (date: string): boolean => {
  const today = new Date()
  today.setHours(0, 0, 0, 0) // 设置为今天的开始时间
  const selectedDate = new Date(date)
  return selectedDate >= today
}

// 提交表单
const submit = async () => {
  if (!notice.value.Title || !notice.value.Content) {
    toast.error('请填写完整信息', { autoClose: 2000, position: 'top-center' })
    return
  }

  loading.value = true
  const url = notice.value.Id ? '/notice/edit' : '/notice/write'
  notice.value.StartTime = timeRange.value.from || undefined
  notice.value.EndTime = timeRange.value.to || undefined
  const response = await api.post(url, notice.value) as ApiResponse

  if (response?.Success) {
    toast.success(response.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
    router.push('/notice/list')
  } else {
    toast.error(response?.Message || '操作失败', { autoClose: 2000, position: 'top-center' })
  }
  loading.value = false
}

// 返回列表
const goBack = () => {
  router.push('/notice/list')
}

// 生命周期钩子
onMounted(() => {
  // 检查是否有编辑 ID
  const id = route.query.id
  if (id) {
    loadNotice(Number(id))
  }
  document.querySelector('.q-page-container').addEventListener('scroll', handleScroll)
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
<style scoped lang="scss">
.notice-write-page {
  padding: 20px;
  margin: 0 auto;
}

.editor-container {
  min-height: 400px;

  :deep(.edui-default) {
    line-height: normal;
  }
}

.q-card {
  margin-bottom: 16px;
}

// 响应式设计
@media (max-width: 768px) {
  .notice-write-page {
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