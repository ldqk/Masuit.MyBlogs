<template>
<div class="values-page">
  <!-- 顶部操作栏 -->
  <div class="row q-mb-md">
    <div class="col text-h6"> 系统变量管理 </div>
    <div class="col text-right">
      <q-btn-group>
        <q-btn color="primary" icon="refresh" label="刷新" @click="loadVariables" :loading="loading" />
        <q-btn color="positive" icon="add" label="添加变量" @click="showAddDialog" />
      </q-btn-group>
    </div>
  </div>
  <!-- 温馨提示 -->
  <q-banner class="bg-warning text-white q-mb-md" rounded>
    <template #avatar>
      <q-icon name="warning" />
    </template>
    <div class="text-body2">
      <p class="q-mb-xs"><strong>温馨提示：</strong></p>
      <p class="q-mb-xs">1. 变量支持关联引用，请注意不要造成循环引用！</p>
      <p class="q-mb-xs">2. 关联引用层级不要太多，避免造成递归查找时的性能问题！</p>
      <p class="q-mb-none">3. 请不要使用如下系统变量名作为变量名：browser(浏览器版本)、os(操作系统)、clientip(客户端ip)、location(客户端地理位置)、network(客户端网络信息)</p>
    </div>
  </q-banner>
  <!-- 变量列表 -->
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6 q-mb-md">
        <q-icon name="settings" class="q-mr-sm" /> 变量列表 <q-chip v-if="variables.length > 0" color="primary" text-color="white" :label="`共 ${variables.length} 个变量`" class="q-ml-sm" />
      </div>
      <!-- 变量为空提示 -->
      <div v-if="variables.length === 0 && !loading" class="text-center q-pa-xl text-grey-6">
        <q-icon name="settings_outline" size="64px" class="q-mb-md" />
        <div class="text-h6">暂无系统变量</div>
        <div class="text-body2 q-mt-sm">点击上方"添加变量"按钮创建第一个变量</div>
      </div>
      <!-- 变量表格 -->
      <div v-else>
        <!-- 分页控制栏 -->
        <div class="row items-center justify-between q-mb-md">
          <div class="col-auto">
            <span class="text-body2 text-grey-7"> 共 {{ variables.length }} 条记录，当前显示第 {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, variables.length) }} 条 </span>
          </div>
          <div class="col-auto">
            <q-select v-model="pageSize" :options="pageSizeOptions" dense outlined label="每页显示" style="min-width: 120px" @update:model-value="onPageSizeChange" />
          </div>
        </div>
        <!-- 变量表格 -->
        <vxe-table :data="paginatedVariables" border stripe>
          <vxe-column field="Key" title="变量名" width="200" :filters="filterOptions" :filter-render="{ name: 'input' }" />
          <vxe-column field="Value" title="变量值" :filters="filterOptions" :filter-render="{ name: 'input' }">
            <template #default="{ row }">
              <div class="variable-value" v-html="row.Value"></div>
            </template>
          </vxe-column>
          <vxe-column title="操作" width="100" align="center" fixed="right">
            <template #default="{ row }">
              <q-btn flat dense color="green" icon="edit" @click="editVariable(row)" />
            </template>
          </vxe-column>
        </vxe-table>
        <!-- 分页器 -->
        <div class="row justify-center q-mt-md" v-if="totalPages > 1">
          <q-pagination v-model="currentPage" :max="totalPages" :max-pages="6" direction-links boundary-links color="primary" @update:model-value="onPageChange" />
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 添加/编辑变量对话框 -->
  <q-dialog v-model="showVariableDialog" persistent>
    <q-card style="min-width: 800px; max-width: 90vw;">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">{{ isEditing ? '修改变量' : '添加变量' }}</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="closeVariableDialog" />
      </q-card-section>
      <q-card-section>
        <div class="q-gutter-md">
          <!-- 变量名 -->
          <q-input v-model="currentVariable.Key" label="变量名" hint="以字母开头，请勿使用系统变量名：browser、os、clientip、location、network" dense outlined required :disable="isEditing" :rules="[validateVariableName]" />
          <!-- 编辑器类型切换 -->
          <div class="row items-center">
            <div class="col-auto text-subtitle2">编辑器类型：</div>
            <div class="col-auto">
              <q-toggle v-model="useMarkdownEditor" :label="useMarkdownEditor ? 'Markdown编辑器' : 'UEditor富文本编辑器'" color="primary" />
            </div>
          </div>
          <!-- 变量值编辑器 -->
          <div>
            <div class="text-subtitle2 q-mb-sm">变量值：</div>
            <!-- UEditor富文本编辑器 -->
            <div v-if="!useMarkdownEditor" class="editor-container">
              <vue-ueditor-wrap v-model="currentVariable.Value" :config="ueditorConfig" editor-id="variable-ueditor" />
            </div>
            <!-- Markdown编辑器 -->
            <div v-else class="editor-container">
              <v-md-editor v-model="currentVariable.Value" height="300px" placeholder="请输入专题描述（支持 Markdown 语法）" :toolbar-config="markdownToolbarConfig" />
            </div>
          </div>
        </div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" @click="closeVariableDialog" />
        <q-btn color="primary" :label="isEditing ? '保存修改' : '确认添加'" @click="saveVariable" :loading="saving" :disable="!currentVariable.Key" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'

// 定义接口类型
interface Variable {
  Key: string
  Value: string
  Id?: number
}

interface ApiResponse {
  Success: boolean
  Message: string
  Data?: any
}

// 响应式数据
const variables = ref<Variable[]>([])
const loading = ref(false)
const saving = ref(false)
const filterOptions = ref([{ data: '' }])
// 分页相关数据
const currentPage = ref(1)
const pageSize = ref(10)
const pageSizeOptions = [
  { label: '5条/页', value: 5 },
  { label: '10条/页', value: 10 },
  { label: '20条/页', value: 20 },
  { label: '50条/页', value: 50 }
]

// 对话框状态
const showVariableDialog = ref(false)
const isEditing = ref(false)

// 当前变量数据
const currentVariable = ref<Variable>({
  Key: '',
  Value: ''
})

// 编辑器相关状态
const useMarkdownEditor = ref(false)

// UEditor配置
const ueditorConfig = {
  autoHeightEnabled: false,
  initialFrameHeight: 300,
  initialFrameWidth: '100%',
  zIndex: 99,
  toolbars: [[
    'fullscreen', 'source', '|', 'undo', 'redo', '|',
    'bold', 'italic', 'underline', 'fontborder', 'strikethrough', 'superscript', 'subscript', 'removeformat', 'formatmatch', 'autotypeset', 'blockquote', 'pasteplain', '|', 'forecolor', 'backcolor', 'insertorderedlist', 'insertunorderedlist', 'selectall', 'cleardoc', '|',
    'rowspacingtop', 'rowspacingbottom', 'lineheight', '|',
    'customstyle', 'paragraph', 'fontfamily', 'fontsize', '|',
    'directionalityltr', 'directionalityrtl', 'indent', '|',
    'justifyleft', 'justifycenter', 'justifyright', 'justifyjustify', '|', 'touppercase', 'tolowercase', '|',
    'link', 'unlink', 'anchor', '|', 'imagenone', 'imageleft', 'imageright', 'imagecenter', '|',
    'simpleupload', 'insertimage', 'emotion', 'scrawl', 'insertvideo', 'music', 'attachment', 'map', 'gmap', 'insertframe', 'insertcode', 'webapp', 'pagebreak', 'template', 'background', '|',
    'horizontal', 'date', 'time', 'spechars', 'snapscreen', 'wordimage', '|',
    'inserttable', 'deletetable', 'insertparagraphbeforetable', 'insertrow', 'deleterow', 'insertcol', 'deletecol', 'mergecells', 'mergeright', 'mergedown', 'splittocells', 'splittorows', 'splittocols', 'charts', '|',
    'print', 'preview', 'searchreplace', 'drafts', 'help'
  ]]
}

// 系统保留变量名
const systemVariables = ['browser', 'os', 'clientip', 'location', 'network']

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

// 计算属性
const totalPages = computed(() => {
  return Math.ceil(variables.value.length / pageSize.value)
})

const paginatedVariables = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return variables.value.slice(start, end)
})

// 分页方法
const onPageChange = (page: number) => {
  currentPage.value = page
}

const onPageSizeChange = (newPageSize: number) => {
  pageSize.value = newPageSize
  currentPage.value = 1 // 重置到第一页
}

// 验证变量名
const validateVariableName = (val: string): boolean | string => {
  if (!val) {
    return '请输入变量名'
  }
  if (!/^[a-zA-Z]/.test(val)) {
    return '变量名必须以字母开头'
  }
  if (systemVariables.includes(val.toLowerCase())) {
    return '不能使用系统保留变量名'
  }
  // 检查是否与现有变量重名（编辑时除外）
  if (!isEditing.value && variables.value.some(v => v.Key === val)) {
    return '变量名已存在'
  }
  return true
}

// 加载变量列表
const loadVariables = async () => {
  loading.value = true
  try {
    const response = await api.get('/values/list') as ApiResponse
    if (response?.Data && Array.isArray(response.Data)) {
      variables.value = response.Data
    } else {
      variables.value = []
    }
  } catch (error) {
    toast.error('加载变量列表失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error loading variables:', error)
    variables.value = []
  } finally {
    loading.value = false
  }
}

// 显示添加对话框
const showAddDialog = () => {
  currentVariable.value = {
    Key: '',
    Value: ''
  }
  isEditing.value = false
  useMarkdownEditor.value = false // 默认使用UEditor
  showVariableDialog.value = true
}

// 编辑变量
const editVariable = (variable: Variable) => {
  currentVariable.value = { ...variable }
  isEditing.value = true
  showVariableDialog.value = true
}

// 关闭对话框
const closeVariableDialog = () => {
  showVariableDialog.value = false
}

// 保存变量
const saveVariable = async () => {
  // 验证变量名
  const nameValidation = validateVariableName(currentVariable.value.Key)
  if (nameValidation !== true) {
    toast.warning(nameValidation as string, { autoClose: 2000, position: 'top-center' })
    return
  }

  if (!currentVariable.value.Key) {
    toast.warning('请输入变量名', { autoClose: 2000, position: 'top-center' })
    return
  }

  saving.value = true
  try {
    const response = await api.post('/values', currentVariable.value) as ApiResponse
    if (response?.Success !== false) {
      toast.success(response?.Message || '保存成功', { autoClose: 2000, position: 'top-center' })
      closeVariableDialog()
      // 延迟刷新列表
      setTimeout(() => {
        loadVariables()
      }, 500)
    } else {
      toast.error(response?.Message || '保存失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('保存失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error saving variable:', error)
  } finally {
    saving.value = false
  }
}

// 生命周期钩子
onMounted(() => {
  loadVariables()
})
</script>
<style scoped lang="scss">
.values-page {
  padding: 20px;
}

.variable-value {
  max-height: 200px;
  overflow-y: auto;
  word-wrap: break-word;
  line-height: 1.5;
}

.editor-container {
  border: 1px solid #ddd;
  border-radius: 4px;
  overflow: hidden;
}

.q-banner {
  border-left: 4px solid #ff9800;
}

// 响应式设计
@media (max-width: 768px) {
  .values-page {
    padding: 10px;
  }

  .q-btn-group {
    flex-direction: column;

    .q-btn {
      margin-bottom: 8px;
    }
  }

  .q-table {
    font-size: 12px;
  }

  .variable-value {
    max-height: 100px;
    font-size: 11px;
  }
}
</style>