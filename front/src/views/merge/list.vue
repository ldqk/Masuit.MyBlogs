<template>
<div class="merge-list-container">
  <q-input v-model="searchKeyword" outlined dense placeholder="全局搜索" class="q-mb-md">
    <template v-slot:append>
      <q-btn color="info" icon="search" @click="loadPageData" :loading="loading" label="搜索" />
    </template>
  </q-input>
  <vxe-table ref="tableRef" :data="tableData" :loading="loading" stripe border>
    <!-- 原标题列 -->
    <vxe-column field="PostTitle" title="原标题" min-width="200" fixed="left">
      <template #default="{ row }">
        <a :href="`/${row.PostId}`" target="_blank" class="text-primary"> {{ row.PostTitle }} </a>
      </template>
    </vxe-column>
    <!-- 新标题列 -->
    <vxe-column field="Title" title="新标题" min-width="200">
      <template #default="{ row }">
        <router-link :to="`/merge/compare?id=${row.Id}`" target="_blank" class="text-primary"> {{ row.Title }} </router-link>
      </template>
    </vxe-column>
    <!-- 修改人列 -->
    <vxe-column field="Modifier" title="修改人" width="120">
      <template #default="{ row }">
        <a :href="`/author/${row.Modifier}`" target="_blank" class="text-primary"> {{ row.Modifier }} </a>
      </template>
    </vxe-column>
    <!-- 修改人邮箱列 -->
    <vxe-column field="ModifierEmail" title="修改人邮箱" width="180" />
    <!-- 提交时间列 -->
    <vxe-column field="SubmitTime" title="提交时间" width="160">
      <template #default="{ row }"> {{ formatDate(row.SubmitTime) }} </template>
    </vxe-column>
    <!-- IP列 -->
    <vxe-column field="IP" title="IP" width="200">
      <template #default="{ row }">
        <div class="column q-gutter-xs">
          <a :href="`/tools/ip/${row.IP}`" target="_blank" class="text-primary"> {{ row.IP }} </a>
          <q-btn color="negative" icon="block" dense flat size="sm">
            <q-tooltip class="bg-red">添加到黑名单</q-tooltip>
            <q-popup-proxy transition-show="scale" transition-hide="scale">
              <q-card>
                <q-card-section class="row items-center">
                  <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                  <div>
                    <div class="text-h6">确认操作</div>
                    <div class="text-subtitle2">此操作将标记该用户IP为黑名单，是否继续？</div>
                  </div>
                </q-card-section>
                <q-card-actions align="right">
                  <q-btn flat label="确认" color="negative" v-close-popup @click="addToBlackList(row.IP)" />
                  <q-btn flat label="取消" color="primary" v-close-popup />
                </q-card-actions>
              </q-card>
            </q-popup-proxy>
          </q-btn>
        </div>
      </template>
    </vxe-column>
    <!-- 状态列 -->
    <vxe-column field="MergeState" title="状态" width="100">
      <template #default="{ row }">
        <q-chip :color="getStatusColor(row.MergeState)" text-color="white" :label="getStatusText(row.MergeState)" dense />
      </template>
    </vxe-column>
    <!-- 操作列 -->
    <vxe-column title="操作" width="230" fixed="right">
      <template #default="{ row }">
        <q-btn-group v-if="row.MergeState === 0">
          <!-- 对比按钮 -->
          <q-btn size="sm" color="info" label="对比" dense :to="`/posts/merge/compare?id=${row.Id}`" />
          <!-- 直接合并按钮 -->
          <q-btn size="sm" color="positive" label="直接合并" dense>
            <q-popup-proxy transition-show="scale" transition-hide="scale">
              <q-card>
                <q-card-section class="row items-center">
                  <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                  <div>
                    <div class="text-h6">确认操作</div>
                    <div class="text-subtitle2">此操作将直接合并该修改，是否继续？</div>
                  </div>
                </q-card-section>
                <q-card-actions align="right">
                  <q-btn flat label="确认" color="negative" v-close-popup @click="directMerge(row)" />
                  <q-btn flat label="取消" color="primary" v-close-popup />
                </q-card-actions>
              </q-card>
            </q-popup-proxy>
          </q-btn>
          <!-- 编辑并合并按钮 -->
          <q-btn size="sm" color="info" label="编辑并合并" dense :to="`/posts/merge/edit?id=${row.Id}`" />
          <!-- 拒绝按钮 -->
          <q-btn size="sm" color="negative" label="拒绝" dense>
            <q-popup-proxy transition-show="scale" transition-hide="scale">
              <q-card>
                <q-card-section class="row items-center">
                  <div style="width: 400px;">
                    <div class="text-h6"><q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />拒绝合并理由：</div>
                    <q-input dense autogrow outlined v-model="reason" placeholder="请填写拒绝理由" />
                  </div>
                </q-card-section>
                <q-card-actions align="right">
                  <q-btn flat label="确认" :disabled="!reason" color="negative" v-close-popup @click="rejectMerge(row)" />
                  <q-btn flat label="取消" color="primary" v-close-popup />
                </q-card-actions>
              </q-card>
            </q-popup-proxy>
          </q-btn>
          <!-- 标记恶意修改按钮 -->
          <q-btn size="sm" color="warning" label="标记恶意" dense>
            <q-popup-proxy transition-show="scale" transition-hide="scale">
              <q-card>
                <q-card-section class="row items-center">
                  <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                  <div>
                    <div class="text-h6">确认操作</div>
                    <div class="text-subtitle2">此操作将标记该修改为恶意修改，是否继续？</div>
                  </div>
                </q-card-section>
                <q-card-actions align="right">
                  <q-btn flat label="确认" color="negative" v-close-popup @click="blockMerge(row)" />
                  <q-btn flat label="取消" color="primary" v-close-popup />
                </q-card-actions>
              </q-card>
            </q-popup-proxy>
          </q-btn>
        </q-btn-group>
      </template>
    </vxe-column>
  </vxe-table>
  <!-- 分页组件 -->
  <div class="q-mt-md flex justify-center items-center">
    <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" :max-pages="6" boundary-numbers @update:model-value="loadPageData" />
    <q-select v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50, 100, 200]" dense outlined class="q-ml-md" style="width: 80px" @update:model-value="loadPageData" />
    <span class="q-ml-sm text-caption"> 共 {{ pagination.total }} 条 </span>
  </div>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import { useRouter } from 'vue-router'
import { useQuasar } from 'quasar'
import dayjs from 'dayjs'
import api from '@/axios/AxiosConfig'

// 类型定义
interface ApiResponse {
  Data?: any[]
  TotalCount?: number
  Message?: string
  Success?: boolean
}

// 响应式数据
const loading = ref(false)
const tableData = ref([])
const searchKeyword = ref('')

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

// 获取状态颜色
const getStatusColor = (state: number) => {
  const colors = {
    0: 'warning', // 待合并
    1: 'positive', // 已合并
    2: 'negative', // 已拒绝
    3: 'negative' // 恶意修改
  }
  return colors[state] || 'grey'
}

// 获取状态文本
const getStatusText = (state: number) => {
  const texts = {
    0: '待合并',
    1: '已合并',
    2: '已拒绝',
    3: '已被标记为恶意修改'
  }
  return texts[state] || '未知'
}

// 加载分页数据
const loadPageData = async () => {
  loading.value = true
  const params = {
    page: pagination.value.page,
    size: pagination.value.rowsPerPage,
    kw: searchKeyword.value || ''
  }

  const data = await api.get('/merge', { params }) as ApiResponse
  if (data) {
    tableData.value = data.Data || []
    pagination.value.total = data.TotalCount || 0
  }
  loading.value = false
}

// 直接合并
const directMerge = async (row: any) => {
  const data = await api.post(`/merge/${row.Id}`) as ApiResponse
  toast.success(data?.Message || '合并成功', { autoClose: 2000, position: 'top-center' })
  await loadPageData()
}
const reason = ref('')
// 拒绝合并
const rejectMerge = async (row: any) => {
  const data = await api.post(`/merge/reject/${row.Id}`, { reason: reason.value }) as ApiResponse
  toast.success(data?.Message || '已拒绝合并', { autoClose: 2000, position: 'top-center' })
  await loadPageData()
}

// 标记恶意修改
const blockMerge = async (row: any) => {
  const data = await api.post(`/merge/block/${row.Id}`) as ApiResponse
  toast.success(data?.Message || '已标记为恶意修改', { autoClose: 2000, position: 'top-center' })
  await loadPageData()
}

// 加入黑名单
const addToBlackList = async (ip: string) => {
  const data = await api.post(`/system/AddToBlackList`, { ip }) as ApiResponse
  toast.success(data?.Message || '已加入黑名单', { autoClose: 2000, position: 'top-center' })
}

// 生命周期钩子
onMounted(() => {
  loadPageData()
})
</script>
<style scoped>
.merge-list-container {
  padding: 16px;
}

.text-primary {
  color: #1976d2;
  text-decoration: none;
}

.text-primary:hover {
  text-decoration: underline;
}
</style>