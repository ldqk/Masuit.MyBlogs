<template>
<div class="email-templates-page">
  <div class="text-h4 q-mb-md">模板管理</div>
  <q-card>
    <q-card-section>
      <div v-if="files.length === 0" class="text-center q-pa-xl">
        <q-icon name="email" size="4rem" color="grey-5" />
        <div class="text-h6 text-grey-7 q-mt-md">暂无模板文件</div>
      </div>
      <div v-else class="row q-gutter-md">
        <div v-for="file in files" :key="file.path" class="col-md-2 col-sm-3 col-xs-4">
          <q-card class="file-card cursor-pointer" @dblclick="viewTemplate(file.path)" :class="{ 'file-card-hover': hoveredFile === file.path }" @mouseenter="hoveredFile = file.path" @mouseleave="hoveredFile = null">
            <q-card-section class="text-center q-pa-md">
              <!-- 文件图标 -->
              <q-icon name="email" size="3rem" color="primary" class="q-mb-sm" />
              <!-- 文件名 -->
              <div class="text-body2 text-weight-medium file-name" :title="file.filename"> {{ file.filename }} </div>
            </q-card-section>
          </q-card>
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 邮件模板内容查看/编辑对话框 -->
  <q-dialog v-model="showTemplateDialog" maximized transition-show="slide-up" transition-hide="slide-down">
    <q-card>
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">{{ currentTemplateFile }}</div>
        <q-space />
        <q-btn-group>
          <q-btn color="primary" icon="save" label="保存" @click="saveTemplate" :loading="saving" v-if="isEditing" />
          <q-btn color="secondary" :icon="isEditing ? 'visibility' : 'edit'" :label="isEditing ? '预览' : '编辑'" @click="toggleEditMode" />
          <q-btn icon="close" flat round dense @click="closeTemplateDialog" />
        </q-btn-group>
      </q-card-section>
      <q-card-section class="q-pt-none">
        <div v-if="isEditing">
          <div ref="templateEditor" class="template-editor"></div>
        </div>
        <div v-else>
          <q-scroll-area style="height: 70vh">
            <div class="template-preview" v-html="templateContent"></div>
          </q-scroll-area>
        </div>
      </q-card-section>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, onUnmounted, nextTick } from 'vue'
import { toast } from 'vue3-toastify'
import api from '../../axios/AxiosConfig'
// Monaco Editor 配置
import '../../utils/MonacoConfig'
// @ts-ignore
import * as monaco from 'monaco-editor'

// 定义接口类型
interface FileInfo {
  filename: string
  path: string
}

interface ApiResponse {
  Success: boolean
  Message: string
  Data?: any
}

// 响应式数据
const files = ref<FileInfo[]>([])
const hoveredFile = ref<string | null>(null)
const showTemplateDialog = ref(false)
const currentTemplateFile = ref('')
const currentTemplatePath = ref('')
const templateContent = ref('')
const isEditing = ref(false)
const saving = ref(false)
const loading = ref(false)

// 编辑器引用
const templateEditor = ref()
let monacoEditor: any = null

// 获取邮件模板文件列表
const getTemplateFiles = async () => {
  loading.value = true
  try {
    const response = await api.get('/file/Getfiles?path=\\template') as ApiResponse

    if (response?.Success && response.Data) {
      files.value = response.Data
    } else {
      toast.error('获取邮件模板列表失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('获取邮件模板列表失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error getting template files:', error)
  } finally {
    loading.value = false
  }
}

// 查看邮件模板内容
const viewTemplate = async (filepath: string) => {
  try {
    const response = await api.post('/file/read', { filename: filepath }) as ApiResponse

    if (response?.Success && response.Data) {
      currentTemplateFile.value = filepath.split('\\').pop() || filepath
      currentTemplatePath.value = filepath
      templateContent.value = response.Data
      showTemplateDialog.value = true
      isEditing.value = false
    } else {
      toast.error('读取邮件模板失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('读取邮件模板失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error viewing template:', error)
  }
}

// 切换编辑模式
const toggleEditMode = async () => {
  isEditing.value = !isEditing.value

  if (isEditing.value) {
    // 切换到编辑模式，初始化编辑器
    await nextTick()
    initEditor()
  } else {
    // 切换到预览模式，获取编辑器内容
    if (monacoEditor) {
      templateContent.value = monacoEditor.getValue()
    }
  }
}

// 初始化Monaco编辑器
const initEditor = () => {
  if (templateEditor.value && !monacoEditor) {
    monacoEditor = monaco.editor.create(templateEditor.value, {
      value: templateContent.value,
      language: 'html',
      theme: 'vs',
      minimap: { enabled: false },
      scrollBeyondLastLine: false,
      wordWrap: 'on'
    })
  } else if (monacoEditor) {
    monacoEditor.setValue(templateContent.value)
  }
}

// 保存邮件模板
const saveTemplate = async () => {
  if (!monacoEditor) return

  saving.value = true
  try {
    const content = monacoEditor.getValue()
    const response = await api.post('/file/save', {
      filename: currentTemplatePath.value,
      content
    }) as ApiResponse

    if (response?.Success) {
      templateContent.value = content
      toast.success('邮件模板保存成功', { autoClose: 2000, position: 'top-center' })
    } else {
      toast.error(response?.Message || '保存失败', { autoClose: 2000, position: 'top-center' })
    }
    toggleEditMode()
  } catch (error) {
    toast.error('保存失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error saving template:', error)
  } finally {
    saving.value = false
  }
}

// 关闭模板对话框
const closeTemplateDialog = () => {
  showTemplateDialog.value = false
  currentTemplateFile.value = ''
  currentTemplatePath.value = ''
  templateContent.value = ''
  isEditing.value = false

  if (monacoEditor) {
    monacoEditor.dispose()
    monacoEditor = null
  }
}

// 生命周期钩子
onMounted(() => {
  getTemplateFiles()
})

onUnmounted(() => {
  if (monacoEditor) {
    monacoEditor.dispose()
  }
})
</script>
<style scoped lang="scss">
.email-templates-page {
  padding: 20px;
}

.file-card {
  transition: all 0.3s ease;
  border: 1px solid #e0e0e0;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    border-color: #1976d2;
  }
}

.file-card-hover {
  border-color: #1976d2;
}

.file-name {
  word-wrap: break-word;
  word-break: break-word;
  line-height: 1.2;
  height: 2.4em;
  overflow: hidden;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
}

.template-editor {
  height: 70vh;
  border: 1px solid #ddd;
  border-radius: 4px;
}

.template-preview {
  font-family: Arial, sans-serif;
  line-height: 1.6;
  padding: 16px;
  background-color: #ffffff;
  border-radius: 4px;
  border: 1px solid #e0e0e0;

  :deep(h1),
  :deep(h2),
  :deep(h3) {
    color: #333;
    margin-top: 0;
    margin-bottom: 16px;
  }

  :deep(p) {
    margin: 8px 0;
  }

  :deep(a) {
    color: #1976d2;
    text-decoration: none;

    &:hover {
      text-decoration: underline;
    }
  }

  :deep(img) {
    max-width: 100%;
    height: auto;
  }

  :deep(table) {
    border-collapse: collapse;
    width: 100%;
    margin: 16px 0;

    th,
    td {
      border: 1px solid #ddd;
      padding: 8px;
      text-align: left;
    }

    th {
      background-color: #f5f5f5;
      font-weight: bold;
    }
  }
}

// 响应式设计
@media (max-width: 768px) {
  .email-templates-page {
    padding: 10px;
  }

  .file-card {
    margin-bottom: 16px;
  }
}
</style>