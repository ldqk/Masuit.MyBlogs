<template>
<div class="notice-list-page">
  <q-card>
    <q-card-section>
      <!-- 顶部控制栏 -->
      <div class="row q-mb-md">
        <div class="col">
          <q-btn-group>
            <q-btn color="info" icon="refresh" label="刷新" @click="loadPageData" :loading="loading" />
            <q-btn color="primary" icon="add" label="新建公告" to="/notice/write" />
          </q-btn-group>
        </div>
        <div class="text-right">
          <q-input v-model="searchKeyword" outlined dense placeholder="全局搜索">
            <template v-slot:append>
              <q-btn dense color="info" icon="search" @click="loadPageData" :loading="loading" label="搜索" />
            </template>
          </q-input>
        </div>
      </div>
      <!-- 主表格 -->
      <div style="height: calc(100vh - 240px);">
        <vxe-table ref="tableRef" :data="tableData" :loading="loading" stripe border height="100%" class="limited-row-height">
          <!-- 标题列 -->
          <vxe-column field="Title" title="标题" min-width="200" fixed="left">
            <template #default="{ row }">
              <a :href="`/notice/${row.Id}`" target="_blank" class="text-primary"> {{ row.Title }} </a>
            </template>
          </vxe-column>
          <!-- 内容列 -->
          <vxe-column field="Content" title="内容" min-width="300">
            <template #default="{ row }">
              <a :href="`/notice/${row.Id}`" target="_blank" class="text-primary">
                <div class="notice-content" v-html="row.Content"></div>
              </a>
            </template>
          </vxe-column>
          <!-- 发表时间列 -->
          <vxe-column field="PostDate" title="发表时间" width="140" sortable>
            <template #default="{ row }"> {{ dayjs(row.PostDate).format('YYYY-MM-DD HH:mm') }} </template>
          </vxe-column>
          <!-- 修改时间列 -->
          <vxe-column field="ModifyDate" title="修改时间" width="140" sortable>
            <template #default="{ row }"> {{ dayjs(row.ModifyDate).format('YYYY-MM-DD HH:mm') }} </template>
          </vxe-column>
          <!-- 浏览次数列 -->
          <vxe-column field="ViewCount" title="浏览次数" width="100" align="center">
            <template #default="{ row }"> {{ row.ViewCount || 0 }} </template>
          </vxe-column>
          <!-- 开始时间列 -->
          <vxe-column field="StartTime" title="开始时间" width="100">
            <template #default="{ row }"> {{ row.StartTime ? dayjs(row.StartTime).format('YYYY-MM-DD') : '-' }} </template>
          </vxe-column>
          <!-- 结束时间列 -->
          <vxe-column field="EndTime" title="结束时间" width="100">
            <template #default="{ row }"> {{ row.EndTime ? dayjs(row.EndTime).format('YYYY-MM-DD') : '-' }} </template>
          </vxe-column>
          <!-- 状态列 -->
          <vxe-column field="NoticeStatus" title="状态" width="110" align="center">
            <template #default="{ row }">
              <q-badge :color="getStatusColor(row.NoticeStatus)" :label="getStatusText(row.NoticeStatus)" />
            </template>
          </vxe-column>
          <!-- 切换状态列 -->
          <vxe-column title="状态切换" width="80" align="center">
            <template #default="{ row }">
              <q-toggle :model-value="row.NoticeStatus === 1" @update:model-value="changeState(row)" color="primary" :disable="loading" />
            </template>
          </vxe-column>
          <!-- 操作列 -->
          <vxe-column title="操作" width="80" align="center" fixed="right">
            <template #default="{ row }">
              <q-btn dense flat size="md" color="primary" icon="edit" :to="`/notice/write?id=${row.Id}`" :disable="loading">
                <q-tooltip>编辑</q-tooltip>
              </q-btn>
              <q-btn dense flat size="md" color="negative" icon="delete" :disable="loading">
                <q-tooltip>删除</q-tooltip>
                <q-popup-proxy transition-show="scale" transition-hide="scale">
                  <q-card>
                    <q-card-section class="row items-center">
                      <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                      <div>
                        <div class="text-h6">确认删除</div>
                        <div class="text-subtitle2">确认删除这条公告吗？<br />{{ row.Title }}</div>
                      </div>
                    </q-card-section>
                    <q-card-actions align="right">
                      <q-btn flat label="确认" color="negative" v-close-popup @click="deleteNotice(row)" />
                      <q-btn flat label="取消" color="primary" v-close-popup />
                    </q-card-actions>
                  </q-card>
                </q-popup-proxy>
              </q-btn>
            </template>
          </vxe-column>
        </vxe-table>
      </div>
      <!-- 分页组件 -->
      <div class="row justify-center q-mt-md">
        <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.size)" :max-pages="6" direction-links boundary-numbers @update:model-value="onPageChange" />
        <q-select class="q-ml-md" v-model="pagination.size" :options="[10, 20, 50, 100]" dense outlined label="每页显示" style="width: 80px" @update:model-value="onPageSizeChange" />
        <span class="q-ml-sm text-caption">共 {{ pagination.total }} 条</span>
      </div>
    </q-card-section>
  </q-card>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'
import dayjs from 'dayjs'

// 定义接口类型
interface Notice {
  Id: number
  Title: string
  Content: string
  PostDate: string
  ModifyDate: string
  ViewCount: number
  StartTime: string | null
  EndTime: string | null
  NoticeStatus: number // 0=未生效, 1=正常, 2=已失效或已下架
  StrongAlert: boolean
}

interface ApiResponse {
  Success: boolean
  Message: string
  TotalCount?: number
  Data?: Notice[]
}

// 响应式数据
const searchKeyword = ref('')
const tableRef = ref()
const loading = ref(false)
const tableData = ref<Notice[]>([])
const pagination = ref({
  page: 1,
  size: 10,
  total: 0
})

// 获取状态颜色
const getStatusColor = (status: number): string => {
  switch (status) {
    case 0: return 'grey' // 未生效
    case 1: return 'green' // 正常
    case 2: return 'red' // 已失效
    default: return 'grey'
  }
}

// 获取状态文本
const getStatusText = (status: number): string => {
  const statusMap = ['未生效', '正常', '已失效或下架']
  return statusMap[status] || '未知'
}

// 加载分页数据
const loadPageData = async () => {
  loading.value = true
  try {
    const response = await api.post(`/notice/getpagedata?page=${pagination.value.page}&size=${pagination.value.size}&keywords=${searchKeyword.value}`) as ApiResponse

    if (response?.Data) {
      tableData.value = response.Data
      pagination.value.total = response.TotalCount || 0
    } else {
      toast.error('获取数据失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('获取数据失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error loading notice data:', error)
  } finally {
    loading.value = false
  }
}

// 页码变化
const onPageChange = (page: number) => {
  pagination.value.page = page
  loadPageData()
}

// 页大小变化
const onPageSizeChange = (size: number) => {
  pagination.value.size = size
  pagination.value.page = 1
  loadPageData()
}

// 删除公告
const deleteNotice = async (row: Notice) => {
  const response = await api.get(`/notice/delete/${row.Id}`) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
    await loadPageData()
  } else {
    toast.error(response?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 切换状态
const changeState = async (row: Notice) => {
  const response = await api.get(`/notice/ChangeState/${row.Id}`) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '状态修改成功', { autoClose: 2000, position: 'top-center' })
    // 切换状态：1 <-> 2
    row.NoticeStatus = 3 - row.NoticeStatus
  } else {
    toast.error(response?.Message || '状态修改失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 生命周期钩子
onMounted(() => {
  loadPageData()
})
</script>
<style scoped lang="scss">
.notice-list-page {
  padding: 20px;
}

.text-primary {
  color: #1976d2;
  text-decoration: none;

  &:hover {
    text-decoration: underline;
  }
}

.notice-content {
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
      }
    }
  }
}
</style>