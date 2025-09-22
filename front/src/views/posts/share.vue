<template>
<div class="share-page">
  <q-card>
    <q-card-section>
      <!-- 顶部控制栏 -->
      <div class="q-mb-md">
        <q-btn-group>
          <q-btn color="negative" icon="add" label="添加分享" @click="showAddDialog = true" :loading="loading" />
          <q-btn color="info" icon="refresh" label="刷新" @click="loadData" :loading="loading" />
        </q-btn-group>
      </div>
      <!-- 主表格 -->
      <vxe-table ref="tableRef" :data="tableData" :loading="loading" stripe border height="600" class="limited-row-height">
        <!-- 标题列 -->
        <vxe-column field="Title" title="标题"></vxe-column>
        <!-- 链接列 -->
        <vxe-column field="Link" title="链接">
          <template #default="{ row }">
            <a :href="row.Link" target="_blank" class="text-primary"> {{ row.Link }} </a>
          </template>
        </vxe-column>
        <!-- 排序列 -->
        <vxe-column field="Sort" title="排序" width="70" sortable>
          <template #default="{ row }"> {{ row.Sort }} </template>
        </vxe-column>
        <!-- 操作列 -->
        <vxe-column title="操作" width="80">
          <template #default="{ row }">
            <!-- 编辑按钮 -->
            <q-btn flat size="md" color="primary" icon="edit" dense @click="editShare(row)">
              <q-tooltip class="bg-primary">编辑</q-tooltip>
            </q-btn>
            <!-- 删除按钮 -->
            <q-btn flat size="md" color="negative" icon="delete" dense>
              <q-tooltip class="bg-negative">删除</q-tooltip>
              <q-popup-proxy transition-show="scale" transition-hide="scale">
                <q-card>
                  <q-card-section class="row items-center">
                    <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                    <div>
                      <div class="text-h6">确认删除</div>
                      <div class="text-subtitle2">确认删除分享【{{ row.Title }}】吗？</div>
                    </div>
                  </q-card-section>
                  <q-card-actions align="right">
                    <q-btn label="确认" color="negative" v-close-popup @click="deleteShare(row)" />
                    <q-btn label="取消" color="primary" v-close-popup />
                  </q-card-actions>
                </q-card>
              </q-popup-proxy>
            </q-btn>
          </template>
        </vxe-column>
      </vxe-table>
    </q-card-section>
  </q-card>
  <!-- 添加/编辑分享对话框 -->
  <q-dialog v-model="showAddDialog" persistent>
    <q-card style="min-width: 600px;">
      <q-card-section>
        <div class="text-h6">{{ isEditing ? '编辑分享' : '添加分享' }}</div>
      </q-card-section>
      <q-card-section>
        <q-input dense autogrow v-model="currentShare.Title" outlined label="标题" :rules="[val => !!val || '标题不能为空']" ref="titleInputRef" />
        <q-input dense autogrow v-model="currentShare.Link" outlined label="URL" :rules="[val => !!val || 'URL不能为空', val => isValidUrl(val) || '请输入有效的URL']" />
        <q-input v-model.number="currentShare.Sort" outlined type="number" label="排序号" hint="数字越小排序越靠前" :rules="[val => val >= 0 || '排序号不能为负数']" />
      </q-card-section>
      <q-card-actions align="right">
        <q-btn label="确定" color="primary" @click="submitShare" :loading="submitting" />
        <q-btn label="取消" color="negative" @click="closeDialog" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <q-inner-loading :showing="loading">
    <q-spinner color="primary" size="56px" />
  </q-inner-loading>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'

// 类型定义
interface Share {
  Id?: number
  Title: string
  Link: string
  Sort: number
}

interface ApiResponse {
  Data?: any[]
  TotalCount?: number
  Message?: string
  Success?: boolean
}

// 响应式数据
const loading = ref(false)
const submitting = ref(false)
const tableData = ref<Share[]>([])
const showAddDialog = ref(false)
const isEditing = ref(false)

// 当前编辑的分享
const currentShare = ref<Share>({
  Title: '',
  Link: '',
  Sort: 0
})

// 表格和输入框引用
const tableRef = ref(null)
const titleInputRef = ref(null)

// URL 验证函数
const isValidUrl = (url: string) => {
  try {
    const validUrl = new URL(url)
    return !!validUrl
  } catch {
    return false
  }
}

// 加载数据
const loadData = async () => {
  loading.value = true
  const response = await api.get('/share') as ApiResponse
  if (response) {
    tableData.value = response.Data || []
    // 按排序号升序排列
    tableData.value.sort((a, b) => (a.Sort || 0) - (b.Sort || 0))
  }
  loading.value = false

}

// 删除分享
const deleteShare = async (row: Share) => {
  const response = await api.delete(`/share/${row.Id}`) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
    await loadData()
  } else {
    toast.error(response?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 编辑分享
const editShare = (row: Share) => {
  currentShare.value = { ...row }
  isEditing.value = true
  showAddDialog.value = true

  nextTick(() => {
    titleInputRef.value?.focus()
  })
}

// 提交分享（添加或编辑）
const submitShare = async () => {
  // 验证必填字段
  if (!currentShare.value.Title.trim()) {
    toast.error('标题不能为空', { autoClose: 2000, position: 'top-center' })
    return
  }

  submitting.value = true
  const response = await api.post(isEditing.value ? `/share/update` : '/share/add', currentShare.value) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '保存成功', { autoClose: 2000, position: 'top-center' })
    closeDialog()
    await loadData()
  } else {
    toast.error(response?.Message || '保存失败', { autoClose: 2000, position: 'top-center' })
  }
  submitting.value = false
}

// 关闭对话框
const closeDialog = () => {
  showAddDialog.value = false
  isEditing.value = false
  currentShare.value = {
    Title: '',
    Link: '',
    Sort: 0
  }
}

// 生命周期钩子
onMounted(() => {
  loadData()
})
</script>
<style scoped lang="scss">
.share-page {
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