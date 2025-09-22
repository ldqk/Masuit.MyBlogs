<template>
<div class="seminar-page">
  <q-card>
    <q-card-section>
      <!-- 顶部控制栏 -->
      <div class="q-mb-md">
        <q-btn-group>
          <q-btn color="negative" icon="add" label="添加专题" @click="showAddDialog = true" :loading="loading" />
          <q-btn color="info" icon="refresh" label="刷新" @click="loadPageData" :loading="loading" />
        </q-btn-group>
      </div>
      <!-- 主表格 -->
      <vxe-table ref="tableRef" :data="tableData" :loading="loading" stripe border class="limited-row-height">
        <!-- 标题列 -->
        <vxe-column field="Title" title="标题" min-width="200" fixed="left">
          <template #default="{ row }">
            <a :href="`/special/${row.Id}`" target="_blank" class="text-primary"> {{ row.Title }} </a>
          </template>
        </vxe-column>
        <!-- 子标题列 -->
        <vxe-column field="SubTitle" title="子标题" min-width="180">
          <template #default="{ row }"> {{ row.SubTitle }} </template>
        </vxe-column>
        <!-- 描述列 -->
        <vxe-column field="Description" title="描述" min-width="250">
          <template #default="{ row }">
            <div v-html="row.Description"></div>
          </template>
        </vxe-column>
        <!-- 操作列 -->
        <vxe-column title="操作" width="80" fixed="right">
          <template #default="{ row }">
            <!-- 编辑按钮 -->
            <q-btn flat size="md" color="primary" icon="edit" dense @click="editSeminar(row)">
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
                      <div class="text-subtitle2">确认删除专题【{{ row.Title }}】吗？</div>
                    </div>
                  </q-card-section>
                  <q-card-actions align="right">
                    <q-btn flat label="确认" color="negative" v-close-popup @click="deleteSeminar(row)" />
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
        <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" :max-pages="6" boundary-numbers @update:model-value="loadPageData" />
        <q-select v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50, 100, 200]" dense outlined class="q-ml-md" style="width: 80px" @update:model-value="loadPageData" />
        <span class="q-ml-sm text-caption">共 {{ pagination.total }} 条</span>
      </div>
    </q-card-section>
  </q-card>
  <!-- 添加/编辑专题对话框 -->
  <q-dialog v-model="showAddDialog" persistent>
    <q-card style="min-width: 80vw;">
      <q-card-section>
        <div class="text-h6">{{ isEditing ? '编辑专题' : '添加专题' }}</div>
      </q-card-section>
      <q-card-section class="column q-gutter-md">
        <q-input dense v-model="currentSeminar.Title" outlined label="专题名称" :rules="[val => !!val || '专题名称不能为空']" ref="titleInputRef" />
        <q-input dense v-model="currentSeminar.SubTitle" outlined label="子标题" />
        <v-md-editor v-model="currentSeminar.Description" height="300px" placeholder="请输入专题描述（支持 Markdown 语法）" :toolbar-config="markdownToolbarConfig" />
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" color="primary" @click="closeDialog" />
        <q-btn label="确定" color="positive" @click="submitSeminar" :loading="submitting" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, nextTick } from 'vue'
import { toast } from 'vue3-toastify'
import dayjs from 'dayjs'
import api from '@/axios/AxiosConfig'

// 类型定义
interface Seminar {
  Id?: number
  Title: string
  SubTitle: string
  Description: string
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
const tableData = ref<Seminar[]>([])
const showAddDialog = ref(false)
const isEditing = ref(false)

// 当前编辑的专题
const currentSeminar = ref<Seminar>({
  Title: '',
  SubTitle: '',
  Description: ''
})

// 分页数据
const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  total: 0
})

// 表格和输入框引用
const tableRef = ref(null)
const titleInputRef = ref(null)

// Markdown 编辑器配置
const markdownToolbarConfig = {
  toolbars: [
    'bold',
    'italic',
    'strikethrough',
    '|',
    'title',
    'quote',
    'unorderedList',
    'orderedList',
    'task',
    '|',
    'codeRow',
    'code',
    'link',
    'image',
    'table',
    '|',
    'hr',
    'br',
    '|',
    'preview',
    'fullscreen'
  ]
}

// 加载分页数据
const loadPageData = async () => {
  loading.value = true
  const response = await api.get(`/seminar/getpagedata?page=${pagination.value.page}&size=${pagination.value.rowsPerPage}`) as ApiResponse
  if (response) {
    tableData.value = response.Data || []
    pagination.value.total = response.TotalCount || 0
  }
  loading.value = false
}

// 删除专题
const deleteSeminar = async (row: Seminar) => {
  const response = await api.post(`/seminar/delete/${row.Id}`) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
    await loadPageData()
  } else {
    toast.error(response?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 编辑专题
const editSeminar = (row: Seminar) => {
  currentSeminar.value = { ...row }
  isEditing.value = true
  showAddDialog.value = true

  nextTick(() => {
    titleInputRef.value?.focus()
  })
}

// 提交专题（添加或编辑）
const submitSeminar = async () => {
  // 验证必填字段
  if (!currentSeminar.value.Title.trim()) {
    toast.error('专题名称不能为空', { autoClose: 2000, position: 'top-center' })
    return
  }

  submitting.value = true
  const response = await api.post('/Seminar/save', currentSeminar.value) as ApiResponse

  if (response?.Success) {
    toast.success(response.Message || '保存成功', { autoClose: 2000, position: 'top-center' })
    closeDialog()
    await loadPageData()
  } else {
    toast.error(response?.Message || '保存失败', { autoClose: 2000, position: 'top-center' })
  }
  submitting.value = false
}

// 关闭对话框
const closeDialog = () => {
  showAddDialog.value = false
  isEditing.value = false
  currentSeminar.value = {
    Title: '',
    SubTitle: '',
    Description: ''
  }
}

// 生命周期钩子
onMounted(() => {
  loadPageData()
})
</script>
<style scoped lang="scss">
.seminar-page {
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

.markdown-preview {
  :deep(.v-md-editor-preview) {
    background: transparent;
    padding: 0;

    .v-md-editor__content {
      font-size: 13px;
      line-height: 1.4;

      h1,
      h2,
      h3,
      h4,
      h5,
      h6 {
        font-size: 14px;
        margin: 4px 0;
      }

      p {
        margin: 2px 0;
      }

      ul,
      ol {
        margin: 2px 0;
        padding-left: 16px;
      }

      blockquote {
        margin: 2px 0;
        padding: 4px 8px;
        border-left: 3px solid #ddd;
        background: #f9f9f9;
      }

      code {
        background: #f5f5f5;
        padding: 1px 4px;
        border-radius: 2px;
        font-size: 12px;
      }

      pre {
        background: #f5f5f5;
        padding: 8px;
        border-radius: 4px;
        overflow: hidden;

        code {
          background: transparent;
          padding: 0;
        }
      }
    }
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