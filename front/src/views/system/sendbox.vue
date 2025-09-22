<template>
<div class="sendbox-page">
  <!-- 顶部操作栏 -->
  <div class="row q-mb-md">
    <div class="col text-h6"> 发件箱管理 </div>
    <div class="col text-right">
      <q-btn-group>
        <q-btn color="primary" icon="refresh" label="刷新" @click="loadMails" :loading="loading" />
        <q-btn color="positive" icon="add" label="新建邮件" @click="showNewMailDialog" />
      </q-btn-group>
    </div>
  </div>
  <!-- 邮件列表 -->
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6 q-mb-md">
        <q-icon name="outgoing_mail" class="q-mr-sm" /> 邮件列表 <q-chip v-if="mails.length > 0" color="primary" text-color="white" :label="`共 ${mails.length} 封邮件`" class="q-ml-sm" />
      </div>
      <!-- 邮件为空提示 -->
      <div v-if="mails.length === 0 && !loading" class="text-center q-pa-xl text-grey-6">
        <q-icon name="mail_outline" size="64px" class="q-mb-md" />
        <div class="text-h6">暂无邮件记录</div>
        <div class="text-body2 q-mt-sm">点击上方"新建邮件"按钮发送第一封邮件</div>
      </div>
      <!-- 邮件列表 -->
      <div v-else>
        <!-- 分页控制栏 -->
        <div class="row items-center justify-between q-mb-md">
          <div class="col-auto">
            <span class="text-body2 text-grey-7"> 共 {{ mails.length }} 条记录，当前显示第 {{ (currentPage - 1) * pageSize + 1 }} - {{ Math.min(currentPage * pageSize, mails.length) }} 条 </span>
          </div>
          <div class="col-auto">
            <q-select v-model="pageSize" :options="pageSizeOptions" dense outlined label="每页显示" style="min-width: 120px" @update:model-value="onPageSizeChange" />
          </div>
        </div>
        <!-- 邮件卡片列表 -->
        <div class="row q-gutter-md">
          <div v-for="(mail, index) in paginatedMails" :key="index" class="col-12">
            <q-card class="mail-card" flat bordered>
              <q-card-section>
                <div class="row items-start q-gutter-md">
                  <!-- 邮件图标 -->
                  <div class="col-auto">
                    <q-avatar color="primary" text-color="white" icon="mail" size="48px" />
                  </div>
                  <!-- 邮件内容 -->
                  <div class="col">
                    <div class="text-h6 text-weight-bold q-mb-xs"> {{ mail.title || '无标题' }} </div>
                    <div class="text-body2 text-grey-7 q-mb-md mail-content" v-html="mail.content"></div>
                    <!-- 邮件信息 -->
                    <div class="row q-gutter-md text-body2 text-grey-6">
                      <div class="col-auto">
                        <q-icon name="person" class="q-mr-xs" /> 收件人：{{ mail.tos }}
                      </div>
                      <div class="col-auto">
                        <q-icon name="schedule" class="q-mr-xs" /> 发送时间：{{ formatDateTime(mail.time) }}
                      </div>
                      <div class="col-auto">
                        <q-icon name="computer" class="q-mr-xs" /> 请求IP： <a :href="`/tools/ip/${mail.clientip}`" target="_blank" class="text-primary"> {{ mail.clientip }} </a>
                        <q-btn color="negative" icon="block" dense flat size="sm" class="q-ml-sm">
                          <q-tooltip>添加到黑名单</q-tooltip>
                          <q-popup-proxy breakpoint="" transition-show="scale" transition-hide="scale">
                            <q-card>
                              <q-card-section class="row items-center">
                                <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                                <div>
                                  <div class="text-h6">确认操作</div>
                                  <div class="text-subtitle2">此操作将标记该用户IP为黑名单，是否继续？</div>
                                </div>
                              </q-card-section>
                              <q-card-actions align="right">
                                <q-btn flat label="确认" color="negative" v-close-popup @click="addToBlackList(mail.clientip)" />
                                <q-btn flat label="取消" color="primary" v-close-popup />
                              </q-card-actions>
                            </q-card>
                          </q-popup-proxy>
                        </q-btn>
                      </div>
                    </div>
                  </div>
                  <!-- 操作按钮 -->
                  <div class="col-auto">
                    <q-btn color="info" icon="visibility" flat round @click="viewMailDetail(mail)">
                      <q-tooltip>查看详情</q-tooltip>
                    </q-btn>
                  </div>
                </div>
              </q-card-section>
            </q-card>
          </div>
        </div>
        <!-- 分页器 -->
        <div class="row justify-center q-mt-md" v-if="totalPages > 1">
          <q-pagination v-model="currentPage" :max="totalPages" :max-pages="6" direction-links boundary-links color="primary" @update:model-value="onPageChange" />
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 新建邮件对话框 -->
  <q-dialog v-model="showMailDialog" persistent>
    <q-card style="min-width: 800px; max-width: 75vw;">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">发送邮件</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="closeMailDialog" />
      </q-card-section>
      <q-card-section>
        <div class="q-gutter-md">
          <!-- 收件人 -->
          <q-input v-model="newMail.tos" label="收件人" hint="多个邮箱用逗号分隔" dense outlined required :rules="[val => !!val || '请输入收件人']" />
          <!-- 邮件标题 -->
          <q-input v-model="newMail.title" label="邮件标题" dense outlined required :rules="[val => !!val || '请输入邮件标题']" />
          <div class="text-subtitle2 ">邮件内容：</div>
          <vue-ueditor-wrap v-model="newMail.content" :config="{ initialFrameHeight: 200, initialFrameWidth: '100%' }" />
        </div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" @click="closeMailDialog" />
        <q-btn icon="send" color="primary" label="发送" @click="sendMail" :loading="sending" :disable="!newMail.tos || !newMail.title" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <!-- 邮件详情对话框 -->
  <q-dialog v-model="showDetailDialog">
    <q-card style="min-width: 700px; max-width: 90vw;">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">邮件详情</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="showDetailDialog = false" />
      </q-card-section>
      <q-card-section v-if="currentMail">
        <div class="q-gutter-md">
          <div class="row">
            <div class="col-2 text-weight-bold">标题：</div>
            <div class="col-10">{{ currentMail.title }}</div>
          </div>
          <div class="row">
            <div class="col-2 text-weight-bold">收件人：</div>
            <div class="col-10">{{ currentMail.tos }}</div>
          </div>
          <div class="row">
            <div class="col-2 text-weight-bold">发送时间：</div>
            <div class="col-10">{{ formatDateTime(currentMail.time) }}</div>
          </div>
          <div class="row">
            <div class="col-2 text-weight-bold">请求IP：</div>
            <div class="col-10">{{ currentMail.clientip }}</div>
          </div>
          <q-separator class="q-my-md" />
          <div>
            <div class="text-weight-bold q-mb-sm">邮件内容：</div>
            <div class="mail-detail-content" v-html="currentMail.content"></div>
          </div>
        </div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="关闭" @click="showDetailDialog = false" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'
import dayjs from 'dayjs'

// 定义接口类型
interface MailItem {
  title: string
  content: string
  tos: string
  time: string
  clientip: string
}

interface NewMail {
  tos: string
  title: string
  content: string
}

interface ApiResponse {
  Success: boolean
  Message: string
  Data?: any
}

// 响应式数据
const mails = ref<MailItem[]>([])
const loading = ref(false)
const sending = ref(false)

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
const showMailDialog = ref(false)
const showDetailDialog = ref(false)

// 新建邮件数据
const newMail = ref<NewMail>({
  tos: '',
  title: '',
  content: ''
})

// 当前查看的邮件
const currentMail = ref<MailItem | null>(null)

// 计算属性
const totalPages = computed(() => {
  return Math.ceil(mails.value.length / pageSize.value)
})

const paginatedMails = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return mails.value.slice(start, end)
})

// 分页方法
const onPageChange = (page: number) => {
  currentPage.value = page
}

const onPageSizeChange = (newPageSize: number) => {
  pageSize.value = newPageSize
  currentPage.value = 1 // 重置到第一页
}

// 加载邮件列表
const loadMails = async () => {
  loading.value = true
  try {
    const response = await api.get('/system/sendbox')
    if (Array.isArray(response)) {
      mails.value = response
    } else {
      mails.value = []
    }
  } catch (error) {
    toast.error('加载邮件列表失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error loading mails:', error)
    mails.value = []
  } finally {
    loading.value = false
  }
}

// 显示新建邮件对话框
const showNewMailDialog = () => {
  newMail.value = {
    tos: '',
    title: '',
    content: ''
  }
  showMailDialog.value = true
}

// 关闭邮件对话框
const closeMailDialog = () => {
  showMailDialog.value = false
}

// 发送邮件
const sendMail = async () => {
  if (!newMail.value.tos || !newMail.value.title) {
    toast.warning('请填写完整的邮件信息', { autoClose: 2000, position: 'top-center' })
    return
  }

  sending.value = true
  try {
    const response = await api.post('/system/sendmail', newMail.value) as ApiResponse
    if (response?.Success !== false) {
      toast.success('邮件发送成功', { autoClose: 2000, position: 'top-center' })
      closeMailDialog()
      // 延迟刷新列表
      setTimeout(() => {
        loadMails()
      }, 500)
    } else {
      toast.error(response?.Message || '邮件发送失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('邮件发送失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error sending mail:', error)
  } finally {
    sending.value = false
  }
}

// 查看邮件详情
const viewMailDetail = (mail: MailItem) => {
  currentMail.value = mail
  showDetailDialog.value = true
}

// 加入黑名单
const addToBlackList = async (ip: string) => {
  const data = await api.post(`/system/AddToBlackList/${ip}`) as ApiResponse
  toast.success(data?.Message || '已加入黑名单', { autoClose: 2000, position: 'top-center' })
}

// 日期格式化
const formatDateTime = (dateStr: string): string => {
  return dayjs(dateStr).format('YYYY-MM-DD HH:mm:ss')
}

// 生命周期钩子
onMounted(() => {
  loadMails()
})

</script>
<style scoped lang="scss">
.sendbox-page {
  padding: 20px;
}

.mail-card {
  transition: all 0.2s ease;

  &:hover {
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
}

.mail-content {
  max-height: 60px;
  overflow: hidden;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  text-overflow: ellipsis;
}

.mail-detail-content {
  background-color: #f5f5f5;
  padding: 16px;
  border-radius: 4px;
  max-height: 400px;
  overflow-y: auto;
  word-wrap: break-word;
}

.editor-container {
  border: 1px solid #ddd;
  border-radius: 4px;
}

// 响应式设计
@media (max-width: 768px) {
  .sendbox-page {
    padding: 10px;
  }

  .q-btn-group {
    flex-direction: column;

    .q-btn {
      margin-bottom: 8px;
    }
  }

  .mail-card {
    .row {
      flex-direction: column;

      .col-auto {
        margin-bottom: 12px;
      }
    }
  }
}
</style>