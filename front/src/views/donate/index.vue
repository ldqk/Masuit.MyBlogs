<template>
<div class="donate-list-container">
  <q-card>
    <q-card-section>
      <!-- 顶部控制栏 -->
      <q-btn-group push>
        <q-btn color="primary" icon="add" @click="openEditDialog()"> 添加打赏记录 </q-btn>
        <q-btn color="info" icon="refresh" @click="loadPageData" :loading="loading"> 刷新 </q-btn>
      </q-btn-group>
      <!-- 主表格 -->
      <div class="q-mt-md">
        <vxe-table :data="tableData" :loading="loading" stripe border>
          <!-- 打赏时间列 -->
          <vxe-column field="DonateTime" title="打赏时间" width="140" fixed="left">
            <template #default="{ row }"> {{ dayjs(row.DonateTime).format('YYYY-MM-DD') }} </template>
          </vxe-column>
          <!-- 昵称列 -->
          <vxe-column field="NickName" title="昵称" min-width="120" />
          <!-- 金额列 -->
          <vxe-column field="Amount" title="金额" width="100" />
          <!-- 打赏方式列 -->
          <vxe-column field="Via" title="打赏方式" width="120" />
          <!-- Email列 -->
          <vxe-column field="Email" title="Email" min-width="180" />
          <!-- QQ或微信列 -->
          <vxe-column field="QQorWechat" title="QQ或微信" width="140" />
          <!-- 操作列 -->
          <vxe-column title="操作" width="110" fixed="right">
            <template #default="{ row }">
              <q-btn dense flat color="primary" icon="edit" @click="openEditDialog(row)" class="q-mr-xs">
                <q-tooltip>编辑</q-tooltip>
              </q-btn>
              <q-btn dense flat color="negative" icon="delete">
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
          </vxe-column>
        </vxe-table>
        <!-- 分页组件 -->
        <div class="q-mt-md flex justify-center items-center">
          <q-pagination max-pages="6" v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" direction-links @update:model-value="loadPageData" />
          <q-select v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50, 100]" dense outlined style="width: 80px" class="q-ml-md" @update:model-value="onPageSizeChange" />
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 编辑弹窗 -->
  <q-dialog v-model="showEditDialog" persistent>
    <q-card style="min-width: 500px">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">{{ editingItem?.Id ? '编辑打赏记录' : '添加打赏记录' }}</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="closeEditDialog" />
      </q-card-section>
      <q-card-section>
        <q-input v-model="editingItem.NickName" label="昵称" outlined :rules="[val => !!val || '请输入昵称']" />
        <q-input v-model="editingItem.DonateTime" label="打赏时间" outlined readonly :rules="[val => !!val || '请选择打赏时间']">
          <template v-slot:append>
            <q-icon name="event" class="cursor-pointer">
              <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                <q-date v-model="editingItem.DonateTime" mask="YYYY-MM-DD" v-close-popup></q-date>
              </q-popup-proxy>
            </q-icon>
          </template>
        </q-input>
        <q-input v-model="editingItem.Amount" label="打赏金额" outlined type="number" step="0.01" :rules="[val => !!val || '请输入金额']" />
        <q-input v-model="editingItem.Via" label="打赏方式" outlined :rules="[val => !!val || '请输入打赏方式']" />
        <q-input v-model="editingItem.Email" label="Email" outlined type="email" />
        <q-input v-model="editingItem.QQorWechat" label="QQ或微信" outlined />
        <div class="row justify-end q-gutter-sm">
          <q-btn label="取消" color="grey" @click="closeEditDialog" />
          <q-btn label="保存" type="submit" color="primary" :loading="saving" @click="saveRecord" />
        </div>
      </q-card-section>
    </q-card>
  </q-dialog>
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

// 响应式数据
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)
const tableData = ref([])

// 分页数据
const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  total: 0
})

// 弹窗控制
const showEditDialog = ref(false)

// 表单数据
const editingItem = ref({
  Id: 0,
  NickName: '',
  DonateTime: '',
  Amount: '',
  Via: '',
  Email: '',
  QQorWechat: ''
})

// 加载数据
const loadPageData = async () => {
  loading.value = true
  try {
    const params = {
      page: pagination.value.page,
      size: pagination.value.rowsPerPage
    }

    const data = await api.post('/donate/getpagedata', null, { params }) as ApiResponse

    if (data) {
      tableData.value = data.Data || []
      pagination.value.total = data.TotalCount || 0
    }
  } catch (error) {
    toast.error('加载数据失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    loading.value = false
  }
}

// 分页变更
const onPageSizeChange = () => {
  pagination.value.page = 1
  loadPageData()
}

// 打开编辑弹窗
const openEditDialog = (item = null) => {
  if (item) {
    // 编辑模式
    editingItem.value = { ...item }
    // 格式化日期为 YYYY-MM-DD 格式
    if (item.DonateTime) {
      editingItem.value.DonateTime = dayjs(item.DonateTime).format('YYYY-MM-DD HH:mm:ss')
    }
  } else {
    // 新增模式
    editingItem.value = {
      Id: null,
      NickName: '',
      DonateTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      Amount: '',
      Via: '',
      Email: '',
      QQorWechat: ''
    }
  }
  showEditDialog.value = true
}

// 关闭编辑弹窗
const closeEditDialog = () => {
  showEditDialog.value = false
  editingItem.value = {
    Id: null,
    NickName: '',
    DonateTime: '',
    Amount: '',
    Via: '',
    Email: '',
    QQorWechat: ''
  }
}

// 保存记录
const saveRecord = async () => {
  saving.value = true
  try {
    const data = await api.post('/donate/save', editingItem.value) as ApiResponse
    if (data?.Success) {
      toast.success(data.Message || '保存成功', { autoClose: 2000, position: 'top-center' })
      closeEditDialog()
      await loadPageData()
    } else {
      toast.error(data?.Message || '保存失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('保存失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    saving.value = false
  }
}

// 执行删除
const deleteRecord = async (id) => {
  deleting.value = true
  try {
    const data = await api.post(`/donate/delete/${id}`) as ApiResponse
    toast.success(data?.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
    await loadPageData()
  } catch (error) {
    toast.error('删除失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    deleting.value = false
  }
}

// 生命周期
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

/* 弹窗样式优化 */
:deep(.q-dialog .q-card) {
  max-width: 90vw;
}

/* 表单输入框样式 */
:deep(.q-field) {
  margin-bottom: 16px;
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
