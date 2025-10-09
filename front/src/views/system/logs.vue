<template>
<div class="system-logs-page">
  <div class="text-h4 q-mb-md">系统日志管理</div>
  <q-card>
    <q-card-section>
      <div v-if="files.length === 0" class="text-center q-pa-xl">
        <q-icon name="description" size="4rem" color="grey-5" />
        <div class="text-h6 text-grey-7 q-mt-md">暂无日志文件</div>
      </div>
      <div v-else class="row q-gutter-md">
        <div v-for="file in files" :key="file" class="col-lg-2 col-md-3 col-sm-4 col-xs-6">
          <q-card class="file-card cursor-pointer" @dblclick="viewLog(file)" :class="{ 'file-card-hover': hoveredFile === file }" @mouseenter="hoveredFile = file" @mouseleave="hoveredFile = null">
            <q-card-section class="text-center q-pa-md relative-position">
              <!-- 删除按钮 -->
              <q-btn round dense size="sm" color="negative" icon="delete" class="delete-btn" @click.stop v-show="hoveredFile === file">
                <q-popup-proxy transition-show="scale" transition-hide="scale">
                  <q-card>
                    <q-card-section class="row items-center">
                      <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                      <div>
                        <div class="text-h6">确认操作</div>
                        <div class="text-subtitle2">此操作将彻底删除该日志文件，是否继续？</div>
                      </div>
                    </q-card-section>
                    <q-card-actions align="right">
                      <q-btn flat label="确认" color="negative" v-close-popup @click.stop="deleteLog(file)" />
                      <q-btn flat label="取消" color="primary" v-close-popup />
                    </q-card-actions>
                  </q-card>
                </q-popup-proxy>
              </q-btn>
              <!-- 文件图标 -->
              <q-icon name="description" size="3rem" color="primary" class="q-mb-sm" />
              <!-- 文件名 -->
              <div class="text-body2 text-weight-medium file-name" :title="file"> {{ file }} </div>
            </q-card-section>
          </q-card>
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 日志内容查看对话框 -->
  <q-dialog v-model="showLogDialog" maximized transition-show="slide-up" transition-hide="slide-down">
    <q-card>
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">{{ currentLogFile }}</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="closeLogDialog" />
      </q-card-section>
      <q-card-section class="q-pt-none">
        <q-scroll-area style="height: calc(100vh - 70px)">
          <pre class="log-content">{{ logContent }}</pre>
        </q-scroll-area>
      </q-card-section>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import { useQuasar } from 'quasar'
import api from '../../axios/AxiosConfig'

// 定义接口类型
interface ApiResponse {
  Success: boolean
  Message: string
  Data?: any
}

// 响应式数据
const $q = useQuasar()
const files = ref<string[]>([])
const hoveredFile = ref<string | null>(null)
const showLogDialog = ref(false)
const currentLogFile = ref('')
const logContent = ref('')
const loading = ref(false)

// 获取日志文件列表
const getLogFiles = async () => {
  loading.value = true
  try {
    const response = await api.get('/dashboard/GetLogfiles') as ApiResponse

    if (response?.Success && response.Data) {
      files.value = response.Data
    } else {
      toast.error('获取日志文件列表失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('获取日志文件列表失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error getting log files:', error)
  } finally {
    loading.value = false
  }
}

// 查看日志内容
const viewLog = async (filename: string) => {
  try {
    const response = await api.post('/dashboard/catlog', { filename }) as ApiResponse

    if (response?.Success && response.Data) {
      currentLogFile.value = filename
      logContent.value = response.Data
      showLogDialog.value = true
    } else {
      toast.error('读取日志文件失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('读取日志文件失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error viewing log:', error)
  }
}

// 删除日志文件
const deleteLog = async (filename: string) => {
  const response = await api.post('/dashboard/deleteFile', { filename }) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
    await getLogFiles() // 刷新列表
  } else {
    toast.error(response?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 关闭日志查看对话框
const closeLogDialog = () => {
  showLogDialog.value = false
  currentLogFile.value = ''
  logContent.value = ''
}

// 生命周期钩子
onMounted(() => {
  getLogFiles()
})
</script>
<style scoped lang="scss">
.system-logs-page {
  padding: 20px;
}

.file-card {
  transition: all 0.3s ease;
  border: 1px solid #e0e0e0;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }
}

.file-card-hover {
  border-color: #1976d2;
}

.delete-btn {
  position: absolute;
  top: 8px;
  right: 8px;
  z-index: 10;
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

.log-content {
  font-family: 'Courier New', monospace;
  font-size: 12px;
  line-height: 1.4;
  white-space: pre-wrap;
  word-wrap: break-word;
  background-color: #f5f5f5;
  padding: 16px;
  border-radius: 4px;
  margin: 0;
}

// 响应式设计
@media (max-width: 768px) {
  .system-logs-page {
    padding: 10px;
  }

  .file-card {
    margin-bottom: 16px;
  }
}
</style>