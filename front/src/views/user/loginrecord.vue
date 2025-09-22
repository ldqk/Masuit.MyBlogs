<template>
<div class="loginrecord-list-container">
  <!-- 顶部控制栏 -->
  <q-btn-group push>
    <q-btn color="info" icon="refresh" @click="loadPageData" :loading="loading"> 刷新 </q-btn>
  </q-btn-group>
  <!-- 主表格 -->
  <div class="q-mt-md">
    <vxe-table :data="tableData" :loading="loading" stripe border>
      <!-- 登录时间列 -->
      <vxe-column field="LoginTime" title="登录时间" width="200">
        <template #default="{ row }"> {{ dayjs(row.LoginTime).format('YYYY-MM-DD HH:mm:ss') }} </template>
      </vxe-column>
      <!-- IP地址列 -->
      <vxe-column field="IP" title="IP地址" width="250" />
      <!-- 地理位置列 -->
      <vxe-column field="PhysicAddress" title="地理位置" />
    </vxe-table>
    <!-- 分页组件 -->
    <div class="q-mt-md flex justify-center items-center">
      <q-pagination max-pages="6" v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" direction-links @update:model-value="loadPageData" />
      <q-select v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50]" dense outlined style="width: 80px" class="q-ml-md" @update:model-value="onPageSizeChange" />
    </div>
  </div>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import dayjs from 'dayjs'
import api from '@/axios/AxiosConfig'

interface LoginRecord {
  Id: number
  LoginTime: string
  IP: string
  Location: string
  Browser: string
  OperatingSystem: string
  DeviceType: string
  Status: string
}

// 响应式数据
const loading = ref(false)
const tableData = ref<LoginRecord[]>([])
const allRecords = ref<LoginRecord[]>([]) // 用于缓存所有数据

// 分页数据
const pagination = ref({
  page: 1,
  rowsPerPage: 15,
  total: 0
})

// 获取当前用户ID
const getCurrentUserId = () => {
  // 默认用户ID或从localStorage获取
  return localStorage.getItem('userId') || '1'
}

// 加载数据
const loadPageData = async () => {
  loading.value = true
  try {
    const userId = getCurrentUserId()
    if (!userId) {
      toast.error('无法获取用户信息', { autoClose: 2000, position: 'top-center' })
      return
    }

    // 根据AngularJS控制器，调用登录记录接口
    const response = await api.get(`/login/getrecent/${userId}`)

    if (response) {
      // 处理数据格式
      const records = response.Data || []

      // 实现客户端分页（类似NgTableParams的行为）
      const startIndex = (pagination.value.page - 1) * pagination.value.rowsPerPage
      const endIndex = startIndex + pagination.value.rowsPerPage

      tableData.value = records.slice(startIndex, endIndex)
      pagination.value.total = records.length

      // 缓存全部数据用于分页
      allRecords.value = records
    }
  } catch (error) {
    console.error('加载登录记录失败:', error)
    toast.error('加载数据失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    loading.value = false
  }
}

// 分页变更处理
const onPageSizeChange = () => {
  pagination.value.page = 1
  updateTableData()
}

// 更新表格数据（用于客户端分页）
const updateTableData = () => {
  if (allRecords.value.length > 0) {
    const startIndex = (pagination.value.page - 1) * pagination.value.rowsPerPage
    const endIndex = startIndex + pagination.value.rowsPerPage
    tableData.value = allRecords.value.slice(startIndex, endIndex)
  }
}

// 生命周期
onMounted(() => {
  loadPageData()
})
</script>
<style scoped>
.loginrecord-list-container {
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

/* 状态芯片样式 */
.q-chip {
  font-size: 12px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .loginrecord-list-container {
    padding: 8px;
  }

  .controls-row {
    flex-direction: column;
    align-items: stretch;
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
  .loginrecord-list-container {
    padding: 4px;
  }
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
