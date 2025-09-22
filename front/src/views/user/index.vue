<template>
<div class="user-list-container">
  <q-card>
    <q-card-section>
      <!-- 顶部控制栏 -->
      <div class="row">
        <div class="col">
          <q-btn-group push>
            <q-btn dense color="primary" icon="add" @click="openEditDialog()"> 添加用户 </q-btn>
            <q-btn dense color="info" icon="refresh" @click="loadPageData" :loading="loading"> 刷新 </q-btn>
          </q-btn-group>
        </div>
        <div class="col-2 text-right">
          <q-input v-model="searchKeyword" placeholder="全局搜索" outlined dense clearable @update:model-value="loadPageData" class="search-input" debounce="500">
            <template v-slot:prepend>
              <q-icon name="search" />
            </template>
          </q-input>
        </div>
      </div>
      <!-- 主表格 -->
      <vxe-table :data="tableData" :loading="loading" stripe border>
        <!-- 头像列 -->
        <vxe-column field="Avatar" title="头像" width="80" fixed="left">
          <template #default="{ row }">
            <q-avatar size="40px" v-if="row.Avatar">
              <img :src="row.Avatar?.startsWith('http') ? row.Avatar : globalConfig.baseURL + row.Avatar"/>
            </q-avatar>
          </template>
        </vxe-column>
        <!-- 用户名列 -->
        <vxe-column field="Username" title="用户名" min-width="120" />
        <!-- 用户邮箱列 -->
        <vxe-column field="Email" title="用户邮箱" min-width="200" />
        <!-- 昵称列 -->
        <vxe-column field="NickName" title="昵称" min-width="120" />
        <!-- 是否管理员列 -->
        <vxe-column field="IsAdmin" title="是否管理员" width="120">
          <template #default="{ row }">
            <q-chip :color="row.IsAdmin ? 'positive' : 'grey'" text-color="white" :label="row.IsAdmin ? '管理员' : '普通用户'" size="sm" />
          </template>
        </vxe-column>
        <!-- 操作列 -->
        <vxe-column title="操作" width="180" fixed="right">
          <template #default="{ row }">
            <q-btn dense flat color="primary" icon="edit" @click="openEditDialog(row)">
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
                      <div class="text-subtitle2">确认删除用户【{{ row.Username }}】吗？</div>
                    </div>
                  </q-card-section>
                  <q-card-actions align="right">
                    <q-btn flat label="确认" color="negative" v-close-popup @click="deletingItem = row; deleteUser()" :loading="deleting" />
                    <q-btn flat label="取消" color="primary" v-close-popup />
                  </q-card-actions>
                </q-card>
              </q-popup-proxy>
            </q-btn>
            <q-btn dense flat color="warning" icon="lock_reset" @click="openResetPasswordDialog(row)">
              <q-tooltip>重置密码</q-tooltip>
            </q-btn>
          </template>
        </vxe-column>
      </vxe-table>
      <!-- 分页组件 -->
      <div class="q-mt-md flex justify-center items-center">
        <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" direction-links @update:model-value="loadPageData" />
        <q-select outlined v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50, 100, 200]" dense @update:model-value="onPageSizeChange" label="每页" />
      </div>
    </q-card-section>
  </q-card>
  <!-- 用户编辑弹窗 -->
  <q-dialog v-model="showEditDialog" persistent>
    <q-card style="min-width: 500px">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">{{ editingItem?.Id ? '编辑用户' : '添加用户' }}</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="closeEditDialog" />
      </q-card-section>
      <q-card-section>
        <q-input v-model="editingItem.Username" label="用户名" outlined :rules="[val => !!val || '请输入用户名']" />
        <q-input v-model="editingItem.NickName" label="用户昵称" outlined :rules="[val => !!val || '请输入用户昵称']" />
        <q-input v-model="editingItem.Email" label="用户邮箱" outlined type="email" :rules="[
          val => !!val || '请输入用户邮箱',
          val => /.+@.+\..+/.test(val) || '请输入正确的邮箱格式'
        ]" />
        <q-toggle v-model="editingItem.IsAdmin" label="管理员权限" color="primary" />
        <div class="row justify-end q-gutter-sm">
          <q-btn label="取消" color="grey" @click="closeEditDialog" />
          <q-btn label="保存" type="submit" color="primary" :loading="saving" @click="saveUser" />
        </div>
      </q-card-section>
    </q-card>
  </q-dialog>
  <!-- 密码重置弹窗 -->
  <q-dialog v-model="showPasswordDialog" persistent>
    <q-card style="min-width: 400px">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">重置密码</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="closePasswordDialog" />
      </q-card-section>
      <q-card-section>
        <div class="q-mb-md">
          <strong>用户：{{ resetPasswordUser?.Username }}</strong>
        </div>
        <q-input v-model="newPassword" label="新密码" outlined type="password" :rules="[
          val => !!val || '请输入新密码',
          val => val.length >= 6 || '密码长度至少6位'
        ]" />
        <div class="row justify-end q-gutter-sm">
          <q-btn label="取消" color="grey" @click="closePasswordDialog" />
          <q-btn label="确定" type="submit" color="primary" :loading="resetting" @click="resetPassword" />
        </div>
      </q-card-section>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'
import globalConfig from '@/config'

// 接口定义
interface ApiResponse<T = any> {
  Success: boolean
  Message?: string
  Data?: T
  TotalCount?: number
}

interface User {
  Id?: number
  Username: string
  Email: string
  NickName: string
  Avatar?: string
  IsAdmin: boolean
}

// 响应式数据
const loading = ref(false)
const saving = ref(false)
const deleting = ref(false)
const resetting = ref(false)
const tableData = ref<User[]>([])
const searchKeyword = ref('')

// 分页数据
const pagination = ref({
  page: 1,
  rowsPerPage: 10,
  total: 0
})

// 弹窗控制
const showEditDialog = ref(false)
const showPasswordDialog = ref(false)

// 表单数据
const editingItem = ref<User>({
  Username: '',
  Email: '',
  NickName: '',
  Avatar: '',
  IsAdmin: false
})

const deletingItem = ref<User | null>(null)
const resetPasswordUser = ref<User | null>(null)
const newPassword = ref('')

// 加载数据
const loadPageData = async () => {
  loading.value = true
  try {
    const params = {
      page: pagination.value.page,
      size: pagination.value.rowsPerPage,
      search: searchKeyword.value || ''
    }

    const response = await api.get('/user/getusers', { params }) as ApiResponse<User[]>
    if (response) {
      tableData.value = response.Data || []
      pagination.value.total = response.TotalCount || 0
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
const openEditDialog = (user: User | null = null) => {
  if (user) {
    // 编辑模式
    editingItem.value = { ...user }
  } else {
    // 新增模式
    editingItem.value = {
      Username: '',
      Email: '',
      NickName: '',
      Avatar: '',
      IsAdmin: false
    }
  }
  showEditDialog.value = true
}

// 关闭编辑弹窗
const closeEditDialog = () => {
  showEditDialog.value = false
  editingItem.value = {
    Username: '',
    Email: '',
    NickName: '',
    Avatar: '',
    IsAdmin: false
  }
}

// 保存用户
const saveUser = async () => {
  saving.value = true
  try {
    const data = await api.post('/user/save', editingItem.value) as ApiResponse
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
const deleteUser = async () => {
  deleting.value = true
  const data = await api.post(`/user/delete?id=${deletingItem.value.Id}`) as ApiResponse
  toast.success(data?.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
  await loadPageData()
  deleting.value = false
}

// 打开密码重置弹窗
const openResetPasswordDialog = (user: User) => {
  resetPasswordUser.value = user
  newPassword.value = ''
  showPasswordDialog.value = true
}

// 关闭密码重置弹窗
const closePasswordDialog = () => {
  showPasswordDialog.value = false
  resetPasswordUser.value = null
  newPassword.value = ''
}

// 重置密码
const resetPassword = async () => {
  resetting.value = true
  try {
    const data = await api.post('/user/ResetPassword', {
      name: resetPasswordUser.value?.Username,
      pwd: newPassword.value
    }) as ApiResponse

    if (data?.Success) {
      toast.success(data.Message || '密码重置成功', { autoClose: 2000, position: 'top-center' })
      closePasswordDialog()
    } else {
      toast.error(data?.Message || '密码重置失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('密码重置失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    resetting.value = false
  }
}

// 生命周期
onMounted(() => {
  loadPageData()
})
</script>
<style scoped>
.user-list-container {
  padding: 16px;
}

.controls-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
}

.search-input {
  min-width: 200px;
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

/* 头像样式 */
:deep(.q-avatar img) {
  border-radius: 50%;
  object-fit: cover;
}

/* 按钮样式 */
.q-btn {
  border-radius: 6px;
}

/* 芯片样式 */
:deep(.q-chip) {
  border-radius: 12px;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .user-list-container {
    padding: 8px;
  }

  .controls-row {
    flex-direction: column;
    align-items: stretch;
  }

  .row .col-md-6 {
    margin-bottom: 8px;
  }

  .search-input {
    min-width: auto;
  }

  /* 移动端表格滚动 */
  :deep(.vxe-table) {
    font-size: 12px;
  }

  :deep(.vxe-table .vxe-body--column) {
    padding: 6px 8px;
  }

  /* 移动端头像尺寸调整 */
  :deep(.q-avatar) {
    width: 32px;
    height: 32px;
  }
}

@media (max-width: 576px) {
  .user-list-container {
    padding: 4px;
  }

  /* 小屏幕下操作按钮尺寸调整 */
  :deep(.vxe-table .q-btn) {
    min-width: 28px;
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

/* 表单弹窗内部样式 */
:deep(.q-dialog .q-card-section) {
  padding: 16px 24px;
}

:deep(.q-form .q-gutter-md) {
  gap: 16px;
}

/* 管理员标识样式 */
.admin-chip {
  font-weight: 600;
}

/* 表格行悬停效果 */
:deep(.vxe-table .vxe-body--row:hover) {
  background-color: #f5f5f5;
}
</style>
