<template>
<div class="search-analysis-container">
  <q-card>
    <q-card-section>
      <!-- 顶部控制栏 -->
      <div class="row">
        <div class="col-12">
          <div class="row">
            <div class="col">
              <q-btn color="info" icon="refresh" @click="loadPageData" :loading="loading" class="q-mr-md"> 刷新 </q-btn>
            </div>
            <div class="text-right">
              <q-input v-model="searchKeyword" placeholder="全局搜索" outlined dense clearable @update:model-value="loadPageData" class="search-input" debounce="500">
                <template v-slot:prepend>
                  <q-icon name="search" />
                </template>
              </q-input>
            </div>
          </div>
        </div>
      </div>
      <!-- 主表格 -->
      <div class="q-mt-md">
        <vxe-table ref="tableRef" :data="tableData" :loading="loading" stripe border>
          <!-- 关键词列 -->
          <vxe-column field="Keywords" title="关键词">
            <template #default="{ row }">
              <a :href="`/search/${row.Keywords}`" target="_blank" class="keyword-link"> {{ row.Keywords }} </a>
            </template>
          </vxe-column>
          <!-- 搜索时间列 -->
          <vxe-column field="SearchTime" title="搜索时间" width="180">
            <template #default="{ row }"> {{ formatDateTime(row.SearchTime) }} </template>
          </vxe-column>
          <!-- 结果集数量列 -->
          <vxe-column field="ResultCount" title="结果集数量" width="120" />
          <!-- 搜索耗时列 -->
          <vxe-column field="Elapsed" title="搜索耗时" width="120">
            <template #default="{ row }"> {{ formatNumber(row.Elapsed, 2) }} ms </template>
          </vxe-column>
          <!-- 客户端IP列 -->
          <vxe-column field="IP" title="客户端IP" min-width="200">
            <template #default="{ row }">
              <div class="ip-info">
                <a :href="`/tools/ip?ip=${row.IP}`" target="_blank" class="ip-link"> {{ row.IP }} </a>
                <span class="region-info">({{ row.Region }})</span>
                <q-btn color="negative" icon="block" dense flat size="sm" class="q-ml-sm">
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
          <!-- 操作列 -->
          <vxe-column title="操作" width="70" fixed="right">
            <template #default="{ row }">
              <q-btn dense flat color="negative" icon="delete">
                <q-tooltip>删除</q-tooltip>
                <q-popup-proxy transition-show="scale" transition-hide="scale">
                  <q-card>
                    <q-card-section class="row items-center">
                      <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                      <div>
                        <div class="text-h6">确认删除</div>
                        <div class="text-subtitle2">确认删除这条记录吗？</div>
                      </div>
                    </q-card-section>
                    <q-card-actions align="right">
                      <q-btn flat label="确认" color="negative" v-close-popup @click="deleteRecord(row.Id)" />
                      <q-btn flat label="取消" color="primary" v-close-popup />
                    </q-card-actions>
                  </q-card>
                </q-popup-proxy>
              </q-btn>
            </template>
          </vxe-column>
        </vxe-table>
        <!-- 分页组件 -->
        <div class="q-mt-md flex justify-center items-center">
          <q-pagination max-pages="6" v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" direction-links @update:model-value="loadPageData" />
          <q-select v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50, 100, 200]" dense outlined style="width: 80px" class="q-ml-md" @update:model-value="onPageSizeChange" />
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 统计排行榜 -->
  <div class="q-mt-lg">
    <div class="row q-gutter-md">
      <!-- 月度搜索排行 -->
      <div class="col-md" v-if="hotKeyData.month && hotKeyData.month.length > 0">
        <q-card>
          <q-card-section class="text-center">
            <div class="text-h6">月度搜索排行</div>
          </q-card-section>
          <q-card-section>
            <vxe-table :data="hotKeyData.month" :loading="loading" stripe border>
              <vxe-column type="seq" title="序号" width="60" />
              <vxe-column field="Keywords" title="关键词">
                <template #default="{ row }">
                  <a :href="`/search/${row.Keywords}`" target="_blank" class="keyword-link"> {{ row.Keywords }} </a>
                </template>
              </vxe-column>
              <vxe-column field="Count" title="搜索次数" width="120"></vxe-column>
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
      <!-- 本周搜索排行 -->
      <div class="col-md" v-if="hotKeyData.week && hotKeyData.week.length > 0">
        <q-card>
          <q-card-section class="text-center">
            <div class="text-h6">本周搜索排行</div>
          </q-card-section>
          <q-card-section>
            <vxe-table :data="hotKeyData.week" :loading="loading" stripe border>
              <vxe-column type="seq" title="序号" width="60" />
              <vxe-column field="Keywords" title="关键词">
                <template #default="{ row }">
                  <a :href="`/search/${row.Keywords}`" target="_blank" class="keyword-link"> {{ row.Keywords }} </a>
                </template>
              </vxe-column>
              <vxe-column field="Count" title="搜索次数" width="120"></vxe-column>
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
      <!-- 今日搜索排行 -->
      <div class="col-md" v-if="hotKeyData.today && hotKeyData.today.length > 0">
        <q-card>
          <q-card-section class="text-center">
            <div class="text-h6">今日搜索排行</div>
          </q-card-section>
          <q-card-section>
            <vxe-table :data="hotKeyData.today" :loading="loading" stripe border>
              <vxe-column type="seq" title="序号" width="60" />
              <vxe-column field="Keywords" title="关键词">
                <template #default="{ row }">
                  <a :href="`/search/${row.Keywords}`" target="_blank" class="keyword-link"> {{ row.Keywords }} </a>
                </template>
              </vxe-column>
              <vxe-column field="Count" title="搜索次数" width="120"></vxe-column>
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
    </div>
    <!-- 搜索结果为0的热词统计 -->
    <div class="row q-gutter-md q-mt-md">
      <!-- 月度搜索排行（搜索结果为0） -->
      <div class="col-md" v-if="hotKeyData.wish_month && hotKeyData.wish_month.length > 0">
        <q-card>
          <q-card-section class="text-center">
            <div class="text-h6">月度搜索排行</div>
            <div class="text-caption text-grey-6">(搜索结果为0的热词)</div>
          </q-card-section>
          <q-card-section>
            <vxe-table :data="hotKeyData.wish_month" :loading="loading" stripe border>
              <vxe-column type="seq" title="序号" width="60" />
              <vxe-column field="Keywords" title="关键词">
                <template #default="{ row }">
                  <a :href="`/search/${row.Keywords}`" target="_blank" class="keyword-link"> {{ row.Keywords }} </a>
                </template>
              </vxe-column>
              <vxe-column field="Count" title="搜索次数" width="120"></vxe-column>
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
      <!-- 本周搜索排行（搜索结果为0） -->
      <div class="col-md" v-if="hotKeyData.wish_week && hotKeyData.wish_week.length > 0">
        <q-card>
          <q-card-section class="text-center">
            <div class="text-h6">本周搜索排行</div>
            <div class="text-caption text-grey-6">(搜索结果为0的热词)</div>
          </q-card-section>
          <q-card-section>
            <vxe-table :data="hotKeyData.wish_week" :loading="loading" stripe border>
              <vxe-column type="seq" title="序号" width="60" />
              <vxe-column field="Keywords" title="关键词">
                <template #default="{ row }">
                  <a :href="`/search/${row.Keywords}`" target="_blank" class="keyword-link"> {{ row.Keywords }} </a>
                </template>
              </vxe-column>
              <vxe-column field="Count" title="搜索次数" width="120"></vxe-column>
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
      <!-- 今日搜索排行（搜索结果为0） -->
      <div class="col-md" v-if="hotKeyData.wish_today && hotKeyData.wish_today.length > 0">
        <q-card>
          <q-card-section class="text-center">
            <div class="text-h6">今日搜索排行</div>
            <div class="text-caption text-grey-6">(搜索结果为0的热词)</div>
          </q-card-section>
          <q-card-section>
            <vxe-table :data="hotKeyData.wish_today" :loading="loading" stripe border>
              <vxe-column type="seq" title="序号" width="60" />
              <vxe-column field="Keywords" title="关键词">
                <template #default="{ row }">
                  <a :href="`/search/${row.Keywords}`" target="_blank" class="keyword-link"> {{ row.Keywords }} </a>
                </template>
              </vxe-column>
              <vxe-column field="Count" title="搜索次数" width="120"></vxe-column>
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
    </div>
  </div>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import dayjs from 'dayjs'
import api from '@/axios/AxiosConfig'

// 接口定义
interface ApiResponse<T = any> {
  Success: boolean
  Message?: string
  Data?: T
  TotalCount?: number
}

interface SearchRecord {
  Id: number
  Keywords: string
  SearchTime: string
  ResultCount: number
  Elapsed: number
  IP: string
  Region: string
}

interface HotKeyItem {
  Keywords: string
  Count: number
}

interface HotKeyData {
  month: HotKeyItem[]
  week: HotKeyItem[]
  today: HotKeyItem[]
  wish_month: HotKeyItem[]
  wish_week: HotKeyItem[]
  wish_today: HotKeyItem[]
}

// 响应式数据
const loading = ref(false)
const deleting = ref(false)
const tableData = ref<SearchRecord[]>([])
const searchKeyword = ref('')

// 分页数据
const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  total: 0
})

// 热词统计数据
const hotKeyData = ref<HotKeyData>({
  month: [],
  week: [],
  today: [],
  wish_month: [],
  wish_week: [],
  wish_today: []
})

// 表格引用
const tableRef = ref(null)

// 方法
const formatDateTime = (dateTime: string) => {
  if (!dateTime) return ''
  return dayjs(dateTime).format('YYYY-MM-DD HH:mm:ss')
}

const formatNumber = (value: number, precision: number = 0) => {
  if (value === null || value === undefined) return '0'
  return Number(value).toFixed(precision)
}

// 加载热词数据
const loadHotKeyData = async () => {
  try {
    const response = await api.get('/search/HotKey') as ApiResponse<HotKeyData>

    if (response?.Success && response.Data) {
      hotKeyData.value = response.Data
    } else {
      toast.error(response?.Message || '获取热词数据失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('获取热词数据失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 加载搜索记录数据
const loadPageData = async () => {
  loading.value = true
  try {
    const params = {
      page: pagination.value.page,
      size: pagination.value.rowsPerPage,
      search: searchKeyword.value || ''
    }

    const response = await api.post('/search/SearchList', null, { params }) as ApiResponse<SearchRecord[]>

    if (response && response.TotalCount > 0) {
      tableData.value = response.Data || []
      pagination.value.total = response.TotalCount || 0
    } else {
      tableData.value = []
      pagination.value.total = 0
      if (response?.Message) {
        toast.warning(response.Message, { autoClose: 2000, position: 'top-center' })
      }
    }
  } catch (error) {
    toast.error('加载数据失败', { autoClose: 2000, position: 'top-center' })
    tableData.value = []
    pagination.value.total = 0
  } finally {
    loading.value = false
  }
}

// 分页变更
const onPageSizeChange = () => {
  pagination.value.page = 1
  loadPageData()
}

// 加入黑名单
const addToBlackList = async (ip: string) => {
  const data = await api.post(`/system/AddToBlackList`, { ip }) as ApiResponse
  toast.success(data?.Message || '已加入黑名单', { autoClose: 2000, position: 'top-center' })
}

// 执行删除
const deleteRecord = async (id) => {
  deleting.value = true
  try {
    const response = await api.post(`/search/delete/${id}`) as ApiResponse
    if (response?.Success) {
      toast.success(response.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
      await loadPageData()
    } else {
      toast.error(response?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('删除失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    deleting.value = false
  }
}

// 生命周期
onMounted(async () => {
  await loadHotKeyData()
  await loadPageData()
})
</script>
<style scoped>
.search-analysis-container {
  padding: 16px;
}

.controls-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
}

.search-input {
  min-width: 250px;
}

/* 表格样式优化 */
:deep(.vxe-table) {
  font-size: 14px;
}

:deep(.vxe-table .vxe-body--column) {
  padding: 8px 12px;
}

:deep(.vxe-table .vxe-header--column) {
  font-weight: 600;
  background-color: #fafafa;
}

/* 链接样式 */
.keyword-link {
  color: #1976d2;
  text-decoration: none;
  font-weight: 500;
}

.keyword-link:hover {
  color: #1565c0;
  text-decoration: underline;
}

.ip-link {
  color: #1976d2;
  text-decoration: none;
  font-size: 16px;
  font-weight: 600;
}

.ip-link:hover {
  color: #1565c0;
  text-decoration: underline;
}

.region-info {
  color: #666;
  font-size: 12px;
  margin-left: 4px;
}

/* IP信息样式 */
.ip-info {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

/* 按钮样式 */
.q-btn {
  border-radius: 6px;
}

/* 卡片样式 */
:deep(.q-card) {
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.1);
}

:deep(.q-card .q-card-section) {
  padding: 16px 20px;
}

/* 统计表格样式 */
:deep(.q-table) {
  box-shadow: none;
}

:deep(.q-table .q-table__top) {
  padding: 0;
}

:deep(.q-table tbody tr:hover) {
  background-color: #f5f5f5;
}

:deep(.q-table th) {
  font-weight: 600;
  color: #333;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .search-analysis-container {
    padding: 8px;
  }

  .controls-row {
    flex-direction: column;
    align-items: stretch;
  }

  .row .col-md-4,
  .row .col-md-8 {
    margin-bottom: 8px;
  }

  .search-input {
    min-width: auto;
  }

  /* 移动端表格滚动 */
  :deep(.vxe-table) {
    font-size: 12px;
  }

  :deep(.vxe-table .vxe-body--column) {
    padding: 6px 8px;
  }

  /* 移动端IP信息调整 */
  .ip-info {
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
  }

  /* 移动端卡片网格调整 */
  .row .col-md-4 {
    flex: 0 0 100%;
    max-width: 100%;
  }
}

@media (max-width: 576px) {
  .search-analysis-container {
    padding: 4px;
  }

  /* 小屏幕下按钮尺寸调整 */
  :deep(.vxe-table .q-btn) {
    min-width: 28px;
    padding: 4px;
  }

  /* 卡片标题调整 */
  :deep(.q-card .text-h6) {
    font-size: 1.1rem;
  }
}

/* 弹窗样式优化 */
:deep(.q-dialog .q-card) {
  max-width: 90vw;
  border-radius: 12px;
}

/* 分页组件样式 */
:deep(.q-pagination) {
  margin-right: 16px;
}

/* 工具提示样式 */
:deep(.q-tooltip) {
  font-size: 12px;
}

/* 加载状态优化 */
:deep(.vxe-table--loading) {
  opacity: 0.6;
}

/* 表格行悬停效果 */
:deep(.vxe-table .vxe-body--row:hover) {
  background-color: #f5f5f5;
}

/* 统计卡片标题样式 */
:deep(.q-card .text-h6) {
  color: #1976d2;
  font-weight: 600;
}

:deep(.q-card .text-caption) {
  margin-top: 4px;
}

/* 数字格式化样式 */
.number-highlight {
  font-weight: 600;
  color: #1976d2;
}

/* 统计表格序号列样式 */
:deep(.q-table tbody td:first-child) {
  font-weight: 600;
  color: #666;
}
</style>
