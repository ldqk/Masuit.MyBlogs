<template>
<div class="q-gutter-sm row items-center no-wrap">
  <q-btn round dense flat icon="home" v-if="$q.screen.gt.sm" href="/">
    <q-tooltip class="bg-grey">网站首页</q-tooltip>
  </q-btn>
  <q-btn round dense flat icon="video_call" v-if="$q.screen.gt.sm" to="/posts/write">
    <q-tooltip class="bg-green">写文章</q-tooltip>
  </q-btn>
  <q-btn round dense flat icon="apps" v-if="$q.screen.gt.sm" to="/filemanager">
    <q-tooltip class="bg-blue">文件管理器</q-tooltip>
  </q-btn>
  <q-btn round dense flat icon="schedule" v-if="$q.screen.gt.sm" to="/task-center">
    <q-tooltip class="bg-red">任务管理器</q-tooltip>
  </q-btn>
  <!-- 待审核文章 -->
  <q-btn round dense flat icon="library_music" v-if="pendingPosts.length > 0">
    <q-badge color="red" floating>{{ pendingPosts.length }}</q-badge>
    <q-tooltip>待审核文章</q-tooltip>
    <q-menu class="pending-menu">
      <div class="q-pa-md" style="min-width: 300px">
        <div class="text-subtitle2 q-mb-sm">待审核文章</div>
        <q-list separator>
          <q-item v-for="post in pendingPosts" :key="post.Id" clickable :href="`${globalConfig.baseURL}/${post.Id}`" class="q-py-sm" target="_blank">
            <q-item-section>
              <q-item-label class="text-body2">{{ post.Title }}</q-item-label>
              <q-item-label caption>{{ formatDate(post.PostDate) }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
        <div class="text-center q-mt-sm">
          <q-btn flat color="primary" size="sm" to="/posts/pending">查看所有</q-btn>
        </div>
      </div>
    </q-menu>
  </q-btn>
  <!-- 待审核评论 -->
  <q-btn round dense flat icon="comment" v-if="pendingComments.length > 0">
    <q-badge color="red" floating>{{ pendingComments.length }}</q-badge>
    <q-tooltip>待审核评论</q-tooltip>
    <q-menu class="pending-menu">
      <div class="q-pa-md" style="min-width: 300px">
        <div class="text-subtitle2 q-mb-sm">待审核评论</div>
        <q-list separator>
          <q-item v-for="comment in pendingComments" :key="comment.Id" clickable :href="`${globalConfig.baseURL}/${comment.PostId}?cid=${comment.Id}#comment`" target="_blank" class="q-py-sm">
            <q-item-section>
              <q-item-label class="text-body2">{{ comment.NickName }}</q-item-label>
              <q-item-label caption>{{ formatDate(comment.CommentDate) }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
        <div class="text-center q-mt-sm">
          <q-btn flat color="primary" size="sm" to="/audit/comments">查看所有</q-btn>
        </div>
      </div>
    </q-menu>
  </q-btn>
  <!-- 待审核留言 -->
  <q-btn round dense flat icon="message" v-if="pendingMessages.length > 0">
    <q-badge color="red" floating>{{ pendingMessages.length }}</q-badge>
    <q-tooltip>待审核留言</q-tooltip>
    <q-menu class="pending-menu">
      <div class="q-pa-md" style="min-width: 300px">
        <div class="text-subtitle2 q-mb-sm">待审核留言</div>
        <q-list separator>
          <q-item v-for="message in pendingMessages" :key="message.Id" clickable :href="`${globalConfig.baseURL}/msg?cid=${message.Id}`" target="_blank" class="q-py-sm">
            <q-item-section>
              <q-item-label class="text-body2">{{ message.NickName }}</q-item-label>
              <q-item-label caption>{{ formatDate(message.PostDate) }}</q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
        <div class="text-center q-mt-sm">
          <q-btn flat color="primary" size="sm" to="/audit/msgs">查看所有</q-btn>
        </div>
      </div>
    </q-menu>
  </q-btn>
  <!-- 未读消息 -->
  <q-btn round dense flat icon="notifications" v-if="internalMsgs.length > 0">
    <q-badge color="red" floating>{{ internalMsgs.length }}</q-badge>
    <q-tooltip>未读消息</q-tooltip>
    <q-menu class="pending-menu">
      <div class="q-pa-md" style="min-width: 350px">
        <div class="row items-center q-mb-sm">
          <div class="text-subtitle2">未读消息</div>
          <q-space />
          <q-btn flat dense icon="check_all" size="sm" @click="clearAllNotifications" class="q-ml-sm">
            <q-tooltip>全部标记为已读</q-tooltip>
          </q-btn>
        </div>
        <q-list separator>
          <q-item v-for="msg in internalMsgs" :key="msg.Id" clickable @click="readMessage(msg)" class="q-py-sm">
            <q-item-section>
              <q-item-label class="text-body2">{{ msg.Title }}</q-item-label>
              <q-item-label caption>
                <span v-html="msg.Content"></span>
              </q-item-label>
            </q-item-section>
          </q-item>
        </q-list>
        <div class="text-center q-mt-sm">
          <q-btn flat color="primary" size="sm" to="/msgs">查看所有消息</q-btn>
        </div>
      </div>
    </q-menu>
  </q-btn>
  <q-btn round flat>
    <q-menu>
      <div class="q-pa-md">
        <!-- 用户信息头部 -->
        <div class="row items-center q-mb-md">
          <q-avatar size="48px">
            <img :src="userInfo.Avatar?.startsWith('http') ? userInfo.Avatar : globalConfig.baseURL + userInfo.Avatar">
          </q-avatar>
          <div class="q-ml-md">
            <div class="text-subtitle1">{{ userInfo.NickName }}</div>
            <div class="text-caption text-grey">{{ userInfo.Username }}</div>
          </div>
        </div>
        <!-- 账号管理菜单 -->
        <q-list dense>
          <q-item clickable @click="changeUsername" class="q-mb-xs">
            <q-item-section avatar>
              <q-icon name="account_circle" />
            </q-item-section>
            <q-item-section>修改用户名</q-item-section>
          </q-item>
          <q-item clickable @click="changeNickname" class="q-mb-xs">
            <q-item-section avatar>
              <q-icon name="person" />
            </q-item-section>
            <q-item-section>修改昵称</q-item-section>
          </q-item>
          <q-item clickable @click="changePassword" class="q-mb-xs">
            <q-item-section avatar>
              <q-icon name="lock" />
            </q-item-section>
            <q-item-section>修改密码</q-item-section>
          </q-item>
          <q-item clickable @click="changeAvatar" class="q-mb-xs">
            <q-item-section avatar>
              <q-icon name="photo_camera" />
            </q-item-section>
            <q-item-section>修改头像</q-item-section>
          </q-item>
          <q-item clickable :to="'/loginrecord'" class="q-mb-xs">
            <q-item-section avatar>
              <q-icon name="history" />
            </q-item-section>
            <q-item-section>登录记录</q-item-section>
          </q-item>
          <q-separator class="q-my-sm" />
          <q-item clickable @click="logout" class="text-negative">
            <q-item-section avatar>
              <q-icon name="exit_to_app" />
            </q-item-section>
            <q-item-section>退出登录</q-item-section>
          </q-item>
        </q-list>
      </div>
    </q-menu>
    <q-avatar size="26px">
      <img :src="userInfo.Avatar?.startsWith('http') ? userInfo.Avatar : globalConfig.baseURL + userInfo.Avatar">
    </q-avatar>
    <q-tooltip>账号</q-tooltip>
  </q-btn>
</div>
<!-- 修改用户名对话框 -->
<q-dialog v-model="showUsernameDialog" persistent>
  <q-card style="min-width: 350px">
    <q-card-section class="row items-center q-pb-none">
      <div class="text-h6">修改用户名</div>
      <q-space />
      <q-btn icon="close" flat round dense v-close-popup />
    </q-card-section>
    <q-card-section>
      <q-input v-model="newUsername" label="请输入新的用户名" outlined :rules="[val => !!val || '请输入用户名']" autofocus />
    </q-card-section>
    <q-card-actions align="right">
      <q-btn flat label="取消" color="grey" v-close-popup />
      <q-btn flat label="确定" color="primary" @click="confirmChangeUsername" />
    </q-card-actions>
  </q-card>
</q-dialog>
<!-- 修改昵称对话框 -->
<q-dialog v-model="showNicknameDialog" persistent>
  <q-card style="min-width: 350px">
    <q-card-section class="row items-center q-pb-none">
      <div class="text-h6">修改昵称</div>
      <q-space />
      <q-btn icon="close" flat round dense v-close-popup />
    </q-card-section>
    <q-card-section>
      <q-input v-model="newNickname" label="请输入新的昵称" outlined :rules="[val => !!val || '请输入昵称']" autofocus />
    </q-card-section>
    <q-card-actions align="right">
      <q-btn flat label="取消" color="grey" v-close-popup />
      <q-btn flat label="确定" color="primary" @click="confirmChangeNickname" />
    </q-card-actions>
  </q-card>
</q-dialog>
<!-- 修改密码对话框 -->
<q-dialog v-model="showPasswordDialog" persistent>
  <q-card style="min-width: 400px">
    <q-card-section class="row items-center q-pb-none">
      <div class="text-h6">修改密码</div>
      <q-space />
      <q-btn icon="close" flat round dense v-close-popup />
    </q-card-section>
    <q-card-section>
      <q-input v-model="old" type="password" label="旧密码" outlined class="q-mb-md" :rules="[val => !!val || '请输入旧密码']" autofocus />
      <q-input v-model="pwd" type="password" label="新密码" outlined class="q-mb-md" :rules="[val => !!val || '请输入新密码']" />
      <q-input v-model="pwd2" type="password" label="确认新密码" outlined :rules="[
        val => !!val || '请确认新密码',
        val => val === pwd || '两次输入的密码不一致'
      ]" />
    </q-card-section>
    <q-card-actions align="right">
      <q-btn flat label="取消" color="grey" v-close-popup />
      <q-btn flat label="确定" color="primary" @click="confirmChangePassword" />
    </q-card-actions>
  </q-card>
</q-dialog>
<!-- 修改头像对话框 -->
<q-dialog v-model="showAvatarDialog" persistent>
  <q-card style="min-width: 350px">
    <q-card-section class="row items-center q-pb-none">
      <div class="text-h6">修改头像</div>
      <q-space />
      <q-btn icon="close" flat round dense v-close-popup />
    </q-card-section>
    <q-card-section class="text-center">
      <div class="q-mb-md">请选择新头像文件</div>
      <q-file v-model="avatarFile" label="选择图片文件" outlined accept="image/*" max-file-size="5242880" @rejected="onAvatarRejected">
        <template v-slot:prepend>
          <q-icon name="photo_camera" />
        </template>
      </q-file>
      <div v-if="avatarFile" class="q-mt-md">
        <q-img :src="avatarPreview?.startsWith('http') ? avatarPreview : globalConfig.baseURL + avatarPreview" style="height: 100px; max-width: 100px" class="rounded-borders" />
      </div>
    </q-card-section>
    <q-card-actions align="right">
      <q-btn flat label="取消" color="grey" v-close-popup />
      <q-btn flat label="上传" color="primary" @click="confirmChangeAvatar" :disable="!avatarFile" />
    </q-card-actions>
  </q-card>
</q-dialog>
<!-- 退出登录确认对话框 -->
<q-dialog v-model="showLogoutDialog" persistent>
  <q-card>
    <q-card-section class="row items-center">
      <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
      <div>
        <div class="text-h6">确认退出</div>
        <div class="text-subtitle2">您确定要退出系统吗？</div>
      </div>
    </q-card-section>
    <q-card-actions align="right">
      <q-btn flat label="取消" color="grey" v-close-popup />
      <q-btn flat label="确定" color="negative" @click="confirmLogout" />
    </q-card-actions>
  </q-card>
</q-dialog>
</template>
<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { useQuasar } from 'quasar'
import api from '@/axios/AxiosConfig'
import globalConfig from '@/config'
import { toast } from 'vue3-toastify'
import { useUserStore } from '@/store/users'

const userInfo = ref({});
// 组合式 API
const $q = useQuasar()

// 待审核数据
const pendingPosts = ref([])
const pendingComments = ref([])
const pendingMessages = ref([])
const internalMsgs = ref([])

// 对话框状态
const showUsernameDialog = ref(false)
const showNicknameDialog = ref(false)
const showPasswordDialog = ref(false)
const showAvatarDialog = ref(false)
const showLogoutDialog = ref(false)

// 表单数据
const newUsername = ref('')
const newNickname = ref('')
const old = ref('')
const pwd = ref('')
const pwd2 = ref('')
const avatarFile = ref(null)
const avatarPreview = ref('')

// EventSource 连接
let messagesSource = null
let internalMsgsSource = null

// 初始化 EventSource 连接
const initEventSources = () => {
  // 如果页面不可见，不初始化连接
  if (document.hidden) {
    return
  }

  try {
    // 获取待审核消息
    messagesSource = new EventSource(`${globalConfig.baseURL}/dashboard/getmessages`)
    messagesSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data)
        pendingPosts.value = data.post || []
        pendingComments.value = data.comments || []
        pendingMessages.value = data.msgs || []
      } catch (error) {
        console.error('解析待审核消息失败:', error)
      }
    }

    messagesSource.onerror = (error) => {
      console.error('待审核消息连接错误:', error)
    }

    // 获取未读内部消息
    internalMsgsSource = new EventSource(`${globalConfig.baseURL}/msg/GetUnreadMsgs`)
    internalMsgsSource.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data)
        internalMsgs.value = data || []
      } catch (error) {
        console.error('解析未读消息失败:', error)
      }
    }

    internalMsgsSource.onerror = (error) => {
      console.error('未读消息连接错误:', error)
    }
  } catch (error) {
    console.error('初始化 EventSource 失败:', error)
  }
}

// 关闭 EventSource 连接
const closeEventSources = () => {
  if (messagesSource) {
    messagesSource.close()
    messagesSource = null
  }
  if (internalMsgsSource) {
    internalMsgsSource.close()
    internalMsgsSource = null
  }
}

// 页面可见性变化处理
const handleVisibilityChange = () => {
  if (document.hidden) {
    // 页面变为不可见，断开连接
    closeEventSources()
  } else {
    // 页面变为可见，恢复连接
    initEventSources()
  }
}

// 格式化日期
const formatDate = (dateStr) => {
  try {
    const date = new Date(dateStr)
    return date.toLocaleDateString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch (error) {
    return dateStr
  }
}

// 标记消息为已读
const readMessage = async (message) => {
  try {
    await api.post(`/msg/read/${message.Id}`)
    if (message.Link) {
      window.open(message.Link, '_blank')
    }
  } catch (error) {
    toast.error('标记消息已读失败:', error)
  }
}

// 清除所有通知
const clearAllNotifications = async () => {
  try {
    if (internalMsgs.value.length === 0) return

    const maxId = Math.max(...internalMsgs.value.map(m => m.Id))
    await api.post(`/msg/MarkRead/${maxId}`)
    internalMsgs.value = []
    toast.success('所有消息已标记为已读')
  } catch (error) {
    toast.error('清除通知失败:', error)
  }
}

// 修改用户名
const changeUsername = () => {
  newUsername.value = userInfo.value.Username
  showUsernameDialog.value = true
}

const confirmChangeUsername = async () => {
  if (!newUsername.value) {
    toast.error('请输入用户名', { autoClose: 2000, position: 'top-center' })
    return
  }

  try {
    const response = await api.post('/user/changeusername', {
      id: userInfo.value.Id,
      username: newUsername.value
    })
    if (response.Success) {
      userInfo.value.Username = newUsername.value
      toast.success('用户名修改成功', { autoClose: 2000, position: 'top-center' })
      showUsernameDialog.value = false
    } else {
      toast.error(response.Message || '修改失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('修改失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 修改昵称
const changeNickname = () => {
  newNickname.value = userInfo.value.NickName
  showNicknameDialog.value = true
}

const confirmChangeNickname = async () => {
  if (!newNickname.value) {
    toast.error('请输入昵称', { autoClose: 2000, position: 'top-center' })
    return
  }

  try {
    const response = await api.post('/user/changenickname', {
      id: userInfo.value.Id,
      username: newNickname.value
    })
    if (response.Success) {
      userInfo.value.NickName = newNickname.value
      toast.success('昵称修改成功', { autoClose: 2000, position: 'top-center' })
      showNicknameDialog.value = false
    } else {
      toast.error(response.Message || '修改失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('修改失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 修改密码
const changePassword = () => {
  old.value = ''
  pwd.value = ''
  pwd2.value = ''
  showPasswordDialog.value = true
}

const confirmChangePassword = async () => {
  if (!old.value) {
    toast.error('请输入旧密码', { autoClose: 2000, position: 'top-center' })
    return
  }
  if (!pwd.value) {
    toast.error('请输入新密码', { autoClose: 2000, position: 'top-center' })
    return
  }
  if (!pwd2.value) {
    toast.error('请确认新密码', { autoClose: 2000, position: 'top-center' })
    return
  }
  if (pwd.value !== pwd2.value) {
    toast.error('两次输入的密码不一致', { autoClose: 2000, position: 'top-center' })
    return
  }

  try {
    const response = await api.post('/user/changepassword', {
      id: userInfo.value.Id,
      old: old.value,
      pwd: pwd.value,
      pwd2: pwd2.value
    })
    if (response.Success) {
      toast.success('密码修改成功', { autoClose: 2000, position: 'top-center' })
      showPasswordDialog.value = false
    } else {
      toast.error(response.Message || '密码修改失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('密码修改失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 修改头像
const changeAvatar = () => {
  avatarFile.value = null
  avatarPreview.value = ''
  showAvatarDialog.value = true
}

const onAvatarRejected = (rejectedEntries) => {
  toast.error('文件大小不能超过5MB', { autoClose: 2000, position: 'top-center' })
}

const confirmChangeAvatar = async () => {
  if (!avatarFile.value) {
    toast.error('请选择头像文件', { autoClose: 2000, position: 'top-center' })
    return
  }

  const formData = new FormData()
  formData.append('file', avatarFile.value)

  try {
    // 先上传文件
    const uploadResponse = await api.post('/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    })

    if (uploadResponse.Success) {
      // 然后更新头像
      const changeResponse = await api.post('/user/changeavatar', {
        id: userInfo.value.Id,
        path: uploadResponse.Data
      })

      if (changeResponse.Success) {
        userInfo.value.Avatar = uploadResponse.Data
        toast.success('头像修改成功', { autoClose: 2000, position: 'top-center' })
        showAvatarDialog.value = false
      } else {
        toast.error('头像修改失败', { autoClose: 2000, position: 'top-center' })
      }
    }
  } catch (error) {
    toast.error('上传失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 登出功能
const logout = () => {
  showLogoutDialog.value = true
}

const confirmLogout = async () => {
  try {
    await api.post('/passport/logout')
    toast.success('退出成功，即将返回网站首页', { autoClose: 2000, position: 'top-center' })
    showLogoutDialog.value = false
    // 延迟跳转
    setTimeout(() => {
      window.location.href = '/'
    }, 2000)
  } catch (error) {
    toast.error('退出登录失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 监听头像文件变化，生成预览
watch(avatarFile, (newFile) => {
  if (newFile) {
    const reader = new FileReader()
    reader.onload = (e) => {
      avatarPreview.value = e.target.result
    }
    reader.readAsDataURL(newFile)
  } else {
    avatarPreview.value = ''
  }
})

// 组件挂载时初始化
onMounted(async () => {
  // 添加页面可见性变化监听
  document.addEventListener('visibilitychange', handleVisibilityChange)

  // 初始化 EventSource 连接
  initEventSources()

  // 获取用户信息
  userInfo.value = await useUserStore().getUserInfo()
})

// 组件卸载时清理连接
onUnmounted(() => {
  // 移除页面可见性变化监听
  document.removeEventListener('visibilitychange', handleVisibilityChange)

  // 关闭 EventSource 连接
  closeEventSources()
})
</script>
<style scoped>
.pending-menu {
  max-height: 400px;
  overflow-y: auto;
}

.pending-menu .q-list {
  max-height: 300px;
  overflow-y: auto;
}

.pending-menu .q-item {
  border-radius: 4px;
  margin-bottom: 2px;
}

.pending-menu .q-item:hover {
  background-color: rgba(25, 118, 210, 0.04);
}

.pending-menu .q-item-label {
  line-height: 1.4;
}

.pending-menu .q-item-label--caption {
  color: rgba(0, 0, 0, 0.6);
  font-size: 12px;
}
</style>
