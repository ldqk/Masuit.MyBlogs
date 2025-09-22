<template>
<div class="msgs-page">
  <!-- 顶部控制栏 -->
  <q-card class="q-mb-md">
    <q-card-section class="q-py-md">
      <div class="row items-center justify-between">
        <div class="text-h5">消息中心</div>
        <div class="q-gutter-sm">
          <q-btn color="info" icon="refresh" label="刷新" @click="loadPageData" :loading="loading" dense />
          <q-btn color="positive" icon="check" label="全部已读" @click="markAllAsRead" :loading="markingRead" :disable="messages.length === 0" dense />
          <q-btn color="negative" icon="delete" label="清除已读" :loading="clearing" :disable="messages.length === 0" dense>
            <q-popup-proxy breakpoint="" transition-show="scale" transition-hide="scale">
              <q-card>
                <q-card-section class="row items-center">
                  <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                  <div>
                    <div class="text-h6">确认操作</div>
                    <div class="text-subtitle2">此操作将清除所有已读消息，是否继续？</div>
                  </div>
                </q-card-section>
                <q-card-actions align="right">
                  <q-btn flat label="确认" color="negative" v-close-popup @click="clearReadMessages" />
                  <q-btn flat label="取消" color="primary" v-close-popup />
                </q-card-actions>
              </q-card>
            </q-popup-proxy>
          </q-btn>
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 消息列表 -->
  <q-card>
    <q-list separator>
      <q-item v-for="message in messages" :key="message.Id" clickable :class="{ 'bg-grey-2': message.Read }">
        <!-- 选择框 -->
        <q-item-section side>
          <q-checkbox v-model="message.Read" @update:model-value="toggleMessageRead(message)" :loading="message.updating" />
        </q-item-section>
        <!-- 消息内容 -->
        <q-item-section @click="openMessage(message)">
          <q-item-label class="text-subtitle1 text-weight-medium"> {{ message.Title }} </q-item-label>
          <q-item-label caption lines="2">
            <div v-html="message.Content"></div>
          </q-item-label>
          <q-item-label caption class="q-mt-xs">
            <div class="row items-center q-gutter-md">
              <span :class="message.Read ? 'text-positive' : 'text-negative'" class="text-weight-medium"> {{ message.Read ? '已读' : '未读' }} </span>
              <span class="text-grey-6"> {{ formatDate(message.Time) }} </span>
            </div>
          </q-item-label>
        </q-item-section>
        <!-- 操作按钮 -->
        <q-item-section side>
          <q-btn-group>
            <q-btn flat color="primary" icon="open_in_new" dense :disable="!message.Link" @click.stop="openMessage(message)" />
            <q-btn flat color="negative" icon="delete" dense>
              <q-tooltip>删除</q-tooltip>
              <q-popup-proxy transition-show="scale" transition-hide="scale">
                <q-card>
                  <q-card-section class="row items-center">
                    <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                    <div>
                      <div class="text-h6">确认删除</div>
                      <div class="text-subtitle2">确认删除这条消息吗？</div>
                    </div>
                  </q-card-section>
                  <q-card-actions align="right">
                    <q-btn flat label="确认" color="negative" v-close-popup @click="deleteMessage(message)" />
                    <q-btn flat label="取消" color="primary" v-close-popup />
                  </q-card-actions>
                </q-card>
              </q-popup-proxy>
            </q-btn>
          </q-btn-group>
        </q-item-section>
      </q-item>
      <!-- 空状态 -->
      <q-item v-if="!loading && messages.length === 0">
        <q-item-section class="text-center q-py-xl">
          <q-icon name="inbox" size="4rem" color="grey-4" />
          <div class="text-grey-6 q-mt-md">暂无消息</div>
        </q-item-section>
      </q-item>
      <!-- 加载状态 -->
      <q-item v-if="loading">
        <q-item-section class="text-center q-py-xl">
          <q-spinner-dots size="2rem" color="primary" />
          <div class="text-grey-6 q-mt-md">加载中...</div>
        </q-item-section>
      </q-item>
    </q-list>
  </q-card>
  <!-- 分页组件 -->
  <div class="q-mt-md flex justify-center items-center" v-if="messages.length > 0">
    <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" :max-pages="6" boundary-numbers @update:model-value="loadPageData" />
    <q-select v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50, 100, 200]" dense outlined class="q-ml-md" style="width: 80px" @update:model-value="onPageSizeChange" />
    <span class="q-ml-sm text-caption">共 {{ pagination.total }} 条</span>
  </div>
</div>
</template>
<script setup>
import { ref, reactive, onMounted } from 'vue'
import dayjs from 'dayjs'
import api from '@/axios/AxiosConfig'
import { toast } from 'vue3-toastify'
import globalConfig from '../../config';

// 响应式数据
const loading = ref(false)
const markingRead = ref(false)
const clearing = ref(false)
const messages = ref([])

// 分页配置
const pagination = reactive({
  page: 1,
  rowsPerPage: 10,
  total: 0
})

// 获取页面数据
const loadPageData = async () => {
  try {
    loading.value = true
    const response = await api.get(`/msg/GetInternalMsgs?page=${pagination.page}&size=${pagination.rowsPerPage}`)
    messages.value = response.Data.map(msg => ({
      ...msg,
      updating: false // 为每个消息添加更新状态
    }))
    pagination.total = response.TotalCount
  } catch (error) {
    toast.error('获取消息列表失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    loading.value = false
  }
}

// 格式化日期
const formatDate = (dateStr) => {
  if (!dateStr) return ''
  try {
    return dayjs(dateStr).format('YYYY-MM-DD HH:mm:ss')
  } catch (error) {
    return dateStr
  }
}

// 打开消息链接
const openMessage = async (message) => {
  // 标记消息为已读
  if (!message.Read) {
    await toggleMessageRead(message)
  }

  // 打开链接
  if (message.Link) {
    window.open(globalConfig.baseURL + message.Link, '_blank')
  }
}

// 切换消息已读状态
const toggleMessageRead = async (message) => {
  try {
    message.updating = true
    const endpoint = message.Read ? `/msg/read/${message.Id}` : `/msg/unread/${message.Id}`
    const response = await api.post(endpoint)

    if (response) {
      toast.success(message.Read ? '标记已读成功' : '标记未读成功', { autoClose: 2000, position: 'top-center' })
    } else {
      // 恢复原状态
      toast.error(response.Message || '操作失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    console.error('切换消息状态失败:', error)
    // 恢复原状态
    message.Read = !message.Read
    toast.error('操作失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    message.updating = false
  }
}

// 全部标记为已读
const markAllAsRead = async () => {
  try {
    const unreadMessages = messages.value.filter(m => !m.Read)
    if (unreadMessages.length === 0) {
      toast.info('没有未读消息', { autoClose: 2000, position: 'top-center' })
      return
    }

    markingRead.value = true

    // 获取最大 ID
    const maxId = Math.max(...messages.value.map(m => m.Id))
    const response = await api.post(`/msg/MarkRead/${maxId}`)

    if (response.Success) {
      // 重新加载数据
      await loadPageData()
      toast.success('全部消息已标记为已读', { autoClose: 2000, position: 'top-center' })
    } else {
      toast.error(response.Message || '标记失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    console.error('标记全部已读失败:', error)
    toast.error('标记失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    markingRead.value = false
  }
}

// 清除已读消息
const clearReadMessages = async () => {
  try {
    clearing.value = true
    const response = await api.post('/msg/ClearMsgs')
    if (response.Success) {
      await loadPageData()
      toast.success('已读消息清除成功', { autoClose: 2000, position: 'top-center' })
    } else {
      toast.error(response.Message || '清除失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    console.error('清除消息失败:', error)
    toast.error('清除失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    clearing.value = false
  }
}

// 删除单条消息
const deleteMessage = async (message) => {
  try {
    if (result) {
      const response = await api.post(`/msg/delete/${message.Id}`)
      if (response.Success) {
        // 从列表中移除
        const index = messages.value.findIndex(m => m.Id === message.Id)
        if (index > -1) {
          messages.value.splice(index, 1)
          pagination.total--
        }

        toast.success('删除成功', { autoClose: 2000, position: 'top-center' })

        // 如果当前页没有数据了，回到上一页
        if (messages.value.length === 0 && pagination.page > 1) {
          pagination.page--
          await loadPageData()
        }
      } else {
        toast.error(response.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
      }
    }
  } catch (error) {
    console.error('删除消息失败:', error)
    toast.error('删除失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 页面大小变化
const onPageSizeChange = () => {
  pagination.page = 1
  loadPageData()
}

// 组件挂载时加载数据
onMounted(() => {
  loadPageData()
})
</script>
<style scoped>
.msgs-page {
  padding: 16px;
}

.q-item {
  transition: all 0.2s ease;
}

.q-item:hover {
  background-color: rgba(25, 118, 210, 0.04) !important;
}

.q-item.bg-grey-2 {
  opacity: 0.7;
}

.q-item-label {
  line-height: 1.4;
}

/* 限制HTML内容的样式 */
.q-item-label div {
  max-height: 3em;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
}

/* 消息内容样式 */
.q-item-label div * {
  font-size: inherit !important;
  line-height: inherit !important;
  color: inherit !important;
}

/* 选择框样式 */
.q-checkbox {
  min-width: 24px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .msgs-page {
    padding: 8px;
  }

  .q-card-section .row {
    flex-direction: column;
    gap: 12px;
  }

  .q-card-section .row>div:last-child {
    justify-content: center;
  }
}
</style>
