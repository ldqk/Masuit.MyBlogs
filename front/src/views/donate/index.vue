<template>
<div class="donate-list-container">
  <q-card>
    <q-card-section>
      <!-- 顶部控制栏 -->
      <q-btn-group push>
        <q-btn color="primary" icon="add" @click="addDonateRow"> 添加打赏记录 </q-btn>
        <q-btn color="info" icon="refresh" @click="loadPageData" :loading="loading"> 刷新 </q-btn>
      </q-btn-group>
      <!-- 主表格 行内编辑 -->
      <div class="q-mt-md">
        <vxe-table ref="tableRef" :data="tableData" :loading="loading" stripe border show-header-overflow show-overflow :edit-config="{ trigger: 'manual', mode: 'row' }">
          <!-- 打赏时间列 -->
          <vxe-column field="DonateTime" title="打赏时间" width="160" fixed="left" :edit-render="{ name: 'VxeInput', props: { type: 'date' } }" formatter="formatDate"></vxe-column>
          <!-- 昵称列 -->
          <vxe-column field="NickName" title="昵称" min-width="140" :edit-render="{ name: 'VxeInput' }"></vxe-column>
          <!-- 金额列 -->
          <vxe-column field="Amount" title="金额" width="110" :edit-render="{ name: 'VxeInput' }"></vxe-column>
          <!-- 打赏方式列 -->
          <vxe-column field="Via" title="打赏方式" width="140" :edit-render="{ name: 'VxeInput' }"></vxe-column>
          <!-- Email列 -->
          <vxe-column field="Email" title="Email" min-width="200" :edit-render="{ name: 'VxeInput', props: { type: 'email' } }"></vxe-column>
          <!-- QQ或微信列 -->
          <vxe-column field="QQorWechat" title="QQ或微信" width="160" :edit-render="{ name: 'VxeInput' }"></vxe-column>
          <!-- 操作列 -->
          <vxe-column title="操作" width="130" fixed="right" align="center">
            <template #default="{ row, $table }">
              <div class="q-gutter-xs">
                <template v-if="$table.isEditByRow(row)">
                  <q-btn color="primary" icon="check" size="sm" dense flat :loading="loading" @click="saveRow(row, $table)">
                    <q-tooltip>保存</q-tooltip>
                  </q-btn>
                  <q-btn color="grey" icon="close" size="sm" dense flat @click="cancelEdit(row, $table)">
                    <q-tooltip>取消</q-tooltip>
                  </q-btn>
                </template>
                <template v-else>
                  <q-btn color="info" icon="edit" size="sm" dense flat @click="$table.setEditRow(row)">
                    <q-tooltip>编辑</q-tooltip>
                  </q-btn>
                  <q-btn color="negative" icon="delete" size="sm" dense flat :disable="row.Id === 0 || row.Id == null">
                    <q-tooltip>删除</q-tooltip>
                    <q-popup-proxy transition-show="scale" transition-hide="scale">
                      <q-card>
                        <q-card-section class="row items-center">
                          <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                          <div>
                            <div class="text-h6">确认删除</div>
                            <div class="text-subtitle2">确认删除这条打赏记录吗？</div>
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
              </div>
            </template>
          </vxe-column>
        </vxe-table>
        <!-- 分页组件 -->
        <vxe-pager :current-page="pagination.page" :total="pagination.total" :page-size="pagination.rowsPerPage" @page-change="onPageSizeChange" />
      </div>
    </q-card-section>
  </q-card>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import dayjs from 'dayjs'
import api from '@/axios/AxiosConfig'

interface DonateItem {
  Id: number | null
  NickName: string
  DonateTime: string
  Amount: string | number
  Via: string
  Email: string
  QQorWechat: string
}

interface ApiResponse<T = any> {
  Success: boolean
  Message?: string
  Data?: T
  TotalCount?: number
}

const loading = ref(false)
const deleting = ref(false)
const tableData = ref<DonateItem[]>([])
const tableRef = ref()
const originalMap = ref(new Map<number, DonateItem>())

// 服务端分页
const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  total: 0
})

const formatDate = (val: string) => val ? dayjs(val).format('YYYY-MM-DD HH:mm:ss') : ''
const autoFillTime = (row: DonateItem) => {
  // 如果只有日期没有时间，补全当前时间
  if (row.DonateTime && row.DonateTime.length === 10) {
    row.DonateTime = dayjs(row.DonateTime + ' 00:00:00').format('YYYY-MM-DD HH:mm:ss')
  }
}

const loadPageData = async () => {
  loading.value = true
  try {
    const params = { page: pagination.value.page, size: pagination.value.rowsPerPage }
    const data = await api.post('/donate/getpagedata', null, { params }) as ApiResponse
    if (data) {
      tableData.value = (data.Data || []).map((r: any) => ({ ...r, DonateTime: dayjs(r.DonateTime).format('YYYY-MM-DD HH:mm:ss') }))
      pagination.value.total = data.TotalCount || 0
    }
  } catch (e) {
    toast.error('加载数据失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    loading.value = false
  }
}

const onPageSizeChange = ({ pageSize, currentPage }) => {
  pagination.value.page = currentPage
  pagination.value.rowsPerPage = pageSize
  loadPageData()
}

const addDonateRow = async () => {
  const $table = tableRef.value
  if (!$table) return
  // 防止已有编辑行
  if ($table.getEditRecords && $table.getEditRecords().length > 0) {
    toast.info('请先保存或取消当前编辑行', { autoClose: 2000, position: 'top-center' })
    return
  }
  // 在首行插入
  const record: DonateItem = {
    Id: 0,
    NickName: '',
    DonateTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
    Amount: '',
    Via: '',
    Email: '',
    QQorWechat: ''
  }
  const { row } = await $table.insertAt(record, 0)
  $table.setEditRow(row)
  originalMap.value.set(0, { ...record, Id: 0 })
}

const saveRow = async (row: DonateItem, table: any) => {
  if (!row.NickName || !row.Amount || !row.Via) {
    toast.warning('请填写必填字段：昵称/金额/打赏方式', { autoClose: 2000, position: 'top-center' })
    return
  }
  try {
    const resp = await api.post('/donate/save', row) as ApiResponse
    if (resp?.Success) {
      toast.success(resp.Message || '保存成功', { autoClose: 2000, position: 'top-center' })
      table.clearEdit()
      await loadPageData()
    } else {
      toast.error(resp?.Message || '保存失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (e) {
    toast.error('保存失败', { autoClose: 2000, position: 'top-center' })
  }
}

const cancelEdit = (row: DonateItem, table: any) => {
  if (row.Id == null) {
    // 新增未保存直接移除
    const idx = tableData.value.indexOf(row)
    if (idx > -1) tableData.value.splice(idx, 1)
  } else {
    const origin = originalMap.value.get(row.Id)
    if (origin) Object.assign(row, origin)
  }
  table.clearEdit()
}

const deleteRecord = async (id: number) => {
  if (!id) return
  deleting.value = true
  try {
    const data = await api.post(`/donate/delete/${id}`) as ApiResponse
    if (data?.Success !== false) {
      toast.success(data?.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
      await loadPageData()
    } else {
      toast.error(data?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (e) {
    toast.error('删除失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    deleting.value = false
  }
}

onMounted(() => {
  loadPageData()
})
</script>
<style scoped>
.donate-list-container {
  padding: 16px;
}

.controls-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
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

/* 按钮样式 */
.q-btn {
  border-radius: 6px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .donate-list-container {
    padding: 8px;
  }

  .controls-row {
    flex-direction: column;
    align-items: stretch;
  }

  .row .col-md-4 {
    margin-bottom: 8px;
  }

  /* 移动端表格滚动 */
  :deep(.vxe-table) {
    font-size: 12px;
  }

  :deep(.vxe-table .vxe-body--column) {
    padding: 6px 8px;
  }
}

@media (max-width: 576px) {
  .donate-list-container {
    padding: 4px;
  }
}

/* 行内编辑输入间距优化 */
:deep(.vxe-table .q-field) {
  margin: 0;
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
</style>
