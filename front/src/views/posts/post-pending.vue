<template><base-content class="q-mt-md">
  <div class="list-container">
    <q-card>
      <q-card-section>
        <!-- 顶部控制栏 -->
        <div class="q-mb-md">
          <q-input v-model="searchKeyword" outlined dense placeholder="全局搜索">
            <template v-slot:append>
              <q-btn color="info" icon="search" @click="loadPageData" :loading="loading" label="搜索" />
            </template>
          </q-input>
        </div>
        <!-- 主表格 -->
        <vxe-table ref="tableRef" :data="tableData" :loading="loading" stripe border>
          <!-- 标题列 -->
          <vxe-column field="Title" title="标题" min-width="200" fixed="left">
            <template #default="{ row }">
              <a :href="`/${row.Id}`" target="_blank" class="text-primary"> {{ row.Title }} </a>
            </template>
          </vxe-column>
          <!-- 作者列 -->
          <vxe-column field="Author" title="作者" width="120">
            <template #default="{ row }">
              <a :href="`/author/${row.Author}`" target="_blank" class="text-primary"> {{ row.Author }} </a>
            </template>
          </vxe-column>
          <!-- 发表时间列 -->
          <vxe-column field="PostDate" title="发表">
            <template #default="{ row }"> {{ formatDate(row.PostDate) }} </template>
          </vxe-column>
          <!-- 作者邮箱列 -->
          <vxe-column field="Email" title="作者邮箱" width="180">
            <template #default="{ row }">
              <div class="column q-gutter-xs">
                <span>{{ row.Email }}</span>
                <q-btn size="sm" color="secondary" label="标记为恶意提交" dense>
                  <q-popup-proxy transition-show="scale" transition-hide="scale">
                    <q-card>
                      <q-card-section class="row items-center">
                        <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                        <div>
                          <div class="text-h6">确认操作</div>
                          <div class="text-subtitle2">此操作将标记该用户为恶意提交，是否继续？</div>
                        </div>
                      </q-card-section>
                      <q-card-actions align="right">
                        <q-btn flat label="确认" color="negative" v-close-popup @click="addToBlock(row);" />
                        <q-btn flat label="取消" color="primary" v-close-popup />
                      </q-card-actions>
                    </q-card>
                  </q-popup-proxy>
                </q-btn>
              </div>
            </template>
          </vxe-column>
          <!-- 提交IP列 -->
          <vxe-column field="IP" title="提交IP" width="150">
            <template #default="{ row }">
              <div class="column q-gutter-xs">
                <span>{{ row.IP }}</span>
                <q-btn color="negative" icon="block" dense flat size="sm" class="q-ml-sm">
                  <q-tooltip>添加到黑名单</q-tooltip>
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
          <!-- 操作列 -->
          <vxe-column title="操作" width="110" fixed="right">
            <template #default="{ row }">
              <q-btn-group push>
                <!-- 删除按钮 -->
                <q-btn size="md" color="negative" icon="delete" dense>
                  <q-popup-proxy transition-show="scale" transition-hide="scale">
                    <q-card>
                      <q-card-section class="row items-center">
                        <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                        <div>
                          <div class="text-h6">确认操作</div>
                          <div class="text-subtitle2">此操作将彻底删除该文章，是否继续？</div>
                        </div>
                      </q-card-section>
                      <q-card-actions align="right">
                        <q-btn flat label="确认" color="negative" v-close-popup @click="deletePost(row)" />
                        <q-btn flat label="取消" color="primary" v-close-popup />
                      </q-card-actions>
                    </q-card>
                  </q-popup-proxy>
                </q-btn>
                <!-- 通过审核按钮 -->
                <q-btn v-if="row.Status === '审核中'" size="md" color="positive" icon="check" dense>
                  <q-popup-proxy transition-show="scale" transition-hide="scale">
                    <q-card>
                      <q-card-section class="row items-center">
                        <q-icon name="warning" color="green" size="2rem" class="q-mr-sm" />
                        <div>
                          <div class="text-h6">确认操作</div>
                          <div class="text-subtitle2">此操作将通过该文章的审核，是否继续？</div>
                        </div>
                      </q-card-section>
                      <q-card-actions align="right">
                        <q-btn flat label="确认" color="positive" v-close-popup @click="passPost(row)" />
                        <q-btn flat label="取消" color="primary" v-close-popup />
                      </q-card-actions>
                    </q-card>
                  </q-popup-proxy>
                </q-btn>
                <!-- 编辑按钮 -->
                <q-btn size="md" color="primary" icon="edit" dense :to="`/posts/write?id=${row.Id}`" />
              </q-btn-group>
            </template>
          </vxe-column>
        </vxe-table>
        <!-- 分页组件 -->
        <div class="q-mt-md flex justify-center items-center">
          <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" :max-pages="6" boundary-numbers @update:model-value="loadPageData" />
          <q-select v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50, 100, 200]" dense outlined class="q-ml-md" style="width: 80px" @update:model-value="onPageSizeChange" />
          <span class="q-ml-sm text-caption"> 共 {{ pagination.total }} 条 </span>
        </div>
      </q-card-section>
    </q-card>
  </div>
</base-content></template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import dayjs from 'dayjs'
import api from '@/axios/AxiosConfig'
import BaseContent from '@/components/BaseContent/BaseContent.vue'

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

// 页面大小变化处理
const onPageSizeChange = () => {
  pagination.value.page = 1
  loadPageData()
}

// 加载分页数据
const loadPageData = async () => {
  loading.value = true
  try {
    const requestData = {
      page: pagination.value.page,
      size: pagination.value.rowsPerPage,
      search: searchKeyword.value || ''
    }

    const data = await api.post('/post/GetPending', requestData) as ApiResponse

    if (data) {
      tableData.value = data.Data || []
      pagination.value.total = data.TotalCount || 0
    }
  } catch (error) {
    toast.error('加载数据失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error loading data:', error)
  } finally {
    loading.value = false
  }
}

// 删除文章
const deletePost = async (row: any) => {
  const data = await api.post(`/post/truncate/${row.Id}`) as ApiResponse
  toast.success(data?.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
  await loadPageData()
}

// 通过审核
const passPost = async (row: any) => {
  const data = await api.post('/post/pass/' + row.Id) as ApiResponse
  toast.success(data?.Message || '审核通过', { autoClose: 2000, position: 'top-center' })
  await loadPageData()
}

// 标记为恶意提交
const addToBlock = async (row: any) => {
  const data = await api.post('/post/block/' + row.Id) as ApiResponse
  toast.success(data?.Message || '已标记为恶意提交', { autoClose: 2000, position: 'top-center' })
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
.list-container {
  padding: 16px;
}
</style>