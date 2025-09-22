<template>
<div class="messages-page">
  <q-card>
    <q-card-section>
      <!-- 顶部控制栏 -->
      <div class="q-mb-md">
        <q-btn color="info" icon="refresh" label="刷新" @click="loadPageData" :loading="loading" />
      </div>
      <!-- 主表格 -->
      <vxe-table ref="tableRef" :data="tableData" :loading="loading" stripe border class="limited-row-height">
        <!-- 昵称列 -->
        <vxe-column field="NickName" title="昵称" width="120" fixed="left"></vxe-column>
        <!-- 内容列 -->
        <vxe-column field="Content" title="内容" min-width="300">
          <template #default="{ row }">
            <a :href="`/msg?cid=${row.Id}#comment`" target="_blank" class="text-primary">
              <div class="message-content" v-html="row.Content"></div>
            </a>
          </template>
        </vxe-column>
        <!-- 留言时间列 -->
        <vxe-column field="PostDate" title="留言时间" width="180" sortable>
          <template #default="{ row }"> {{ formatDate(row.PostDate) }} </template>
        </vxe-column>
        <!-- 邮箱列 -->
        <vxe-column field="Email" title="邮箱" width="200"></vxe-column>
        <!-- 操作系统列 -->
        <vxe-column field="OperatingSystem" title="操作系统" width="140"></vxe-column>
        <!-- 浏览器列 -->
        <vxe-column field="Browser" title="浏览器" width="140"></vxe-column>
        <!-- 操作列 -->
        <vxe-column title="操作" width="80" fixed="right">
          <template #default="{ row }">
            <q-btn-group push>
              <!-- 删除按钮 -->
              <q-btn size="md" color="negative" icon="delete" dense>
                <q-popup-proxy transition-show="scale" transition-hide="scale">
                  <q-card>
                    <q-card-section class="row items-center">
                      <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                      <div>
                        <div class="text-h6">确认删除</div>
                        <div class="text-subtitle2">确认删除这条留言吗？</div>
                      </div>
                    </q-card-section>
                    <q-card-actions align="right">
                      <q-btn flat label="确认" color="negative" v-close-popup @click="deleteMessage(row)" />
                      <q-btn flat label="取消" color="primary" v-close-popup />
                    </q-card-actions>
                  </q-card>
                </q-popup-proxy>
                <q-tooltip>删除</q-tooltip>
              </q-btn>
              <!-- 通过审核按钮 -->
              <q-btn size="md" color="positive" icon="check" dense @click="passMessage(row)">
                <q-tooltip>通过审核</q-tooltip>
              </q-btn>
            </q-btn-group>
          </template>
        </vxe-column>
      </vxe-table>
      <!-- 分页组件 -->
      <div class="q-mt-md flex justify-center items-center">
        <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" :max-pages="6" boundary-numbers @update:model-value="loadPageData" />
        <q-select v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50, 100, 200]" dense outlined class="q-ml-md" style="width: 80px" @update:model-value="onPageSizeChange" />
        <span class="q-ml-sm text-caption">共 {{ pagination.total }} 条</span>
      </div>
    </q-card-section>
  </q-card>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import { useRouter } from 'vue-router'
import dayjs from 'dayjs'
import api from '@/axios/AxiosConfig'

// 类型定义
interface Message {
  Id: number
  NickName: string
  Title: string
  Content: string
  PostDate: string
  Email: string
  OperatingSystem: string
  Browser: string
}

interface ApiResponse {
  Data?: any[]
  TotalCount?: number
  Message?: string
  Success?: boolean
}

// 路由
const router = useRouter()

// 响应式数据
const loading = ref(false)
const tableData = ref<Message[]>([])

// 分页数据
const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  total: 0
})

// 表格引用
const tableRef = ref(null)

// 方法
const formatDate = (date: string) => {
  return dayjs(date).format('YYYY-MM-DD HH:mm:ss')
}

// 页面大小变化处理
const onPageSizeChange = () => {
  pagination.value.page = 1
  loadPageData()
}

// 加载分页数据
const loadPageData = async () => {
  loading.value = true
  try {
    const response = await api.get(`/msg/GetPendingMsgs?page=${pagination.value.page}&size=${pagination.value.rowsPerPage}`) as ApiResponse

    if (response) {
      tableData.value = response.Data || []
      pagination.value.total = response.TotalCount || 0
    }
  } catch (error) {
    toast.error('加载数据失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error loading data:', error)
  } finally {
    loading.value = false
  }
}

// 删除留言
const deleteMessage = async (row: Message) => {
  try {
    const response = await api.post(`/msg/delete/${row.Id}`) as ApiResponse

    if (response?.Success) {
      toast.success(response.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
      await loadPageData()
    } else {
      toast.error(response?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('删除失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error deleting message:', error)
  }
}

// 通过审核
const passMessage = async (row: Message) => {
  try {
    const response = await api.post('/msg/pass/' + row.Id) as ApiResponse

    if (response?.Success) {
      toast.success(response.Message || '审核通过', { autoClose: 2000, position: 'top-center' })
      await loadPageData()
    } else {
      toast.error(response?.Message || '审核失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('审核失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error passing message:', error)
  }
}

// 生命周期钩子
onMounted(() => {
  loadPageData()
})
</script>
<style scoped lang="scss">
.messages-page {
  padding: 20px;
}

.text-truncate {
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.text-primary {
  color: #1976d2;
  text-decoration: none;

  &:hover {
    text-decoration: underline;
  }
}

.message-content {
  max-height: 200px;
  overflow-y: auto;
  word-wrap: break-word;
  word-break: break-word;
  line-height: 1.4;

  // 处理 HTML 内容样式
  :deep(p) {
    margin: 4px 0;
  }

  :deep(br) {
    line-height: 1.2;
  }

  :deep(img) {
    max-width: 100%;
    height: auto;
  }

  :deep(a) {
    color: #1976d2;
    text-decoration: none;

    &:hover {
      text-decoration: underline;
    }
  }

  :deep(code) {
    background: #f5f5f5;
    padding: 2px 4px;
    border-radius: 3px;
    font-family: monospace;
  }

  :deep(blockquote) {
    margin: 8px 0;
    padding: 8px 12px;
    border-left: 3px solid #ddd;
    background: #f9f9f9;
  }
}

// 限制表格行高
.limited-row-height {
  :deep(.vxe-table--body-wrapper) {
    .vxe-body--row {
      max-height: 300px;
    }

    .vxe-body--column {
      max-height: 300px;
      overflow: hidden;

      .vxe-cell {
        max-height: 300px;
        overflow-y: auto;
        word-wrap: break-word;
        word-break: break-word;
      }
    }
  }
}
</style>