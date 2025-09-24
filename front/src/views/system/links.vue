<template>
<div class="links-container">
  <!-- 顶部操作栏 -->
  <div class="row q-mb-md">
    <div class="text-h6">
      <q-icon name="link" class="q-mr-sm" /> 友情链接列表 <q-chip v-if="links.length > 0" color="primary" text-color="white" :label="`共 ${links.length} 个链接`" class="q-ml-sm" />
    </div>
    <div class="col">
      <q-btn-group>
        <q-btn color="primary" icon="refresh" label="刷新" @click="loadLinks" :loading="loading" />
        <q-btn color="positive" icon="add" label="添加链接" @click="showAddDialog" />
      </q-btn-group>
    </div>
    <div class="col-4 text-right">
      <q-input autofocus v-model="searchTerm" dense outlined placeholder="搜索名称、链接地址" debounce="100">
        <template #prepend>
          <q-icon name="search" class="cursor-pointer" />
        </template>
        <template #append>
          <q-icon name="close" class="cursor-pointer" v-if="searchTerm" @click="searchTerm = ''" />
        </template>
      </q-input>
    </div>
  </div>
  <!-- 友情链接表格 -->
  <q-card flat bordered>
    <q-card-section>
      <!-- 链接为空提示 -->
      <div v-if="filteredLinks.length === 0 && !loading" class="text-center q-pa-xl text-grey-6">
        <q-icon name="link_off" size="64px" class="q-mb-md" />
        <div class="text-h6">暂无友情链接</div>
        <div class="text-body2 q-mt-sm">点击上方"添加链接"按钮添加第一个友情链接</div>
      </div>
      <!-- 友情链接表格 -->
      <vxe-table v-else ref="tableRef" :data="paginatedLinks" stripe border show-header-overflow show-overflow :loading="loading" :edit-config="{ trigger: 'manual', mode: 'row' }">
        <!-- 名称列 -->
        <vxe-column field="Name" title="名称" width="150" sortable :edit-render="{}">
          <template #default="{ row }">
            <a :href="`https://www.baidu.com/s?wd=${row.Name}`" target="_blank" class="text-primary text-weight-bold"> {{ row.Name }} </a>
          </template>
          <template #edit="{ row }">
            <q-input v-model="row.Name" dense outlined />
          </template>
        </vxe-column>
        <!-- 链接地址列 -->
        <vxe-column field="Url" title="链接地址" min-width="200" sortable :edit-render="{}">
          <template #default="{ row }">
            <a :href="row.Url" target="_blank" class="text-primary"> {{ row.Url }} </a>
          </template>
          <template #edit="{ row }">
            <q-input v-model="row.Url" dense outlined />
          </template>
        </vxe-column>
        <!-- 主页地址列 -->
        <vxe-column field="UrlBase" title="主页地址" min-width="200" :edit-render="{}">
          <template #default="{ row }">
            <a :href="row.UrlBase" target="_blank" class="text-primary"> {{ row.UrlBase }} </a>
          </template>
          <template #edit="{ row }">
            <q-input v-model="row.UrlBase" dense outlined />
          </template>
        </vxe-column>
        <!-- 回链次数列 -->
        <vxe-column field="Loopbacks" title="最近来源次数" width="120" sortable align="center">
          <template #default="{ row }">
            <q-chip :color="row.Loopbacks > 0 ? 'positive' : 'grey'" text-color="white" :label="row.Loopbacks" />
          </template>
        </vxe-column>
        <!-- 更新时间列 -->
        <vxe-column field="UpdateTime" title="更新时间" width="160" sortable>
          <template #default="{ row }"> {{ formatDateTime(row.UpdateTime) }} </template>
        </vxe-column>
        <!-- 白名单列 -->
        <vxe-column field="Except" title="白名单" width="80" align="center">
          <template #default="{ row }">
            <q-toggle v-model="row.Except" color="positive" @update:model-value="toggleWhitelist(row)" />
          </template>
        </vxe-column>
        <!-- 推荐列 -->
        <vxe-column field="Recommend" title="推荐" width="80" align="center">
          <template #default="{ row }">
            <q-toggle v-model="row.Recommend" color="warning" @update:model-value="toggleRecommend(row)" />
          </template>
        </vxe-column>
        <!-- 可用状态列 -->
        <vxe-column field="Status" title="可用状态" width="90" align="center">
          <template #default="{ row }">
            <q-toggle :model-value="row.Status === 1" color="primary" @update:model-value="toggleStatus(row)" />
          </template>
        </vxe-column>
        <!-- 操作列 -->
        <vxe-column title="操作" width="100" align="center">
          <template #default="{ row, $table }">
            <div class="q-gutter-xs">
              <!-- 编辑模式下的按钮 -->
              <template v-if="$table.isEditByRow(row)">
                <q-btn color="primary" icon="check" size="sm" dense flat @click="saveLink(row, $table)">
                  <q-tooltip>保存</q-tooltip>
                </q-btn>
                <q-btn color="grey" icon="close" size="sm" dense flat @click="cancelEdit(row, $table)">
                  <q-tooltip>取消</q-tooltip>
                </q-btn>
              </template>
              <!-- 正常模式下的按钮 -->
              <template v-else>
                <q-btn color="info" icon="edit" size="sm" dense flat @click="$table.setEditRow(row)">
                  <q-tooltip>编辑</q-tooltip>
                </q-btn>
                <q-btn color="negative" icon="delete" size="sm" dense flat>
                  <q-tooltip>删除</q-tooltip>
                  <q-popup-proxy transition-show="scale" transition-hide="scale">
                    <q-card>
                      <q-card-section class="row items-center">
                        <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                        <div>
                          <div class="text-h6">确认删除</div>
                          <div class="text-subtitle2">确认删除该友情链接 {{ row.Name }} 吗？</div>
                        </div>
                      </q-card-section>
                      <q-card-actions align="right">
                        <q-btn flat label="确认" color="negative" v-close-popup @click="deleteLink(row)" />
                        <q-btn flat label="取消" color="primary" v-close-popup />
                      </q-card-actions>
                    </q-card>
                  </q-popup-proxy>
                </q-btn>
                <q-btn color="primary" icon="link" size="sm" dense flat @click="checkLink(row)">
                  <q-tooltip>检查链接</q-tooltip>
                </q-btn>
              </template>
            </div>
          </template>
        </vxe-column>
      </vxe-table>
      <!-- 分页器 -->
      <div class="row justify-center q-mt-md" v-if="totalPages > 1">
        <q-pagination v-model="currentPage" :max="totalPages" :max-pages="6" direction-links boundary-links color="primary" @update:model-value="onPageChange" />
      </div>
    </q-card-section>
  </q-card>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'
import dayjs from 'dayjs'

// 定义接口类型
interface LinkItem {
  Id: number
  Name: string
  Url: string
  UrlBase: string
  Loopbacks: number
  UpdateTime: string
  Except: boolean
  Recommend: boolean,
  Status: number
}

interface ApiResponse {
  Success: boolean
  Message: string
  Data?: any
}

// 响应式数据
const links = ref<LinkItem[]>([])
const loading = ref(false)

// 分页相关数据
const currentPage = ref(1)
const pageSize = ref(15)
const searchTerm = ref('')

// 表格引用
const tableRef = ref()

// 原始数据备份（用于编辑取消）
const originalData = ref(new Map())
// 分页计算属性
const filteredLinks = computed(() => {
  return links.value.filter(x => x.Name?.includes(searchTerm.value) || x.Url?.includes(searchTerm.value) || x.UrlBase?.includes(searchTerm.value))
})
// 计算属性
const totalPages = computed(() => {
  return Math.ceil(filteredLinks.value.length / pageSize.value)
})

const paginatedLinks = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredLinks.value.slice(start, end)
})

// 分页方法
const onPageChange = (page: number) => {
  currentPage.value = page
}

// 加载友情链接列表
const loadLinks = async () => {
  loading.value = true
  try {
    const response = await api.get('/links/get') as ApiResponse
    if (response?.Success && response.Data) {
      links.value = response.Data
    } else {
      links.value = []
    }
  } catch (error) {
    toast.error('加载友情链接失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error loading links:', error)
    links.value = []
  } finally {
    loading.value = false
  }
}

// 显示添加对话框
const showAddDialog = async () => {
  const $table = tableRef.value
  if ($table) {
    const record = {
      Name: '',
      Url: '',
      UrlBase: '',
      Id: 0,
      Loopbacks: 0,
      UpdateTime: dayjs().format('YYYY-MM-DD HH:mm:ss'),
      Except: false,
      Recommend: false,
      Status: 0
    }
    const { row: newRow } = await $table.insert(record)
    $table.setEditCell(newRow, 'Name')
  }
}

// 保存编辑
const saveLink = async (row: LinkItem, table: any) => {
  if (!row.Name || !row.Url || !row.UrlBase) {
    toast.warning('请填写完整的链接信息', { autoClose: 2000, position: 'top-center' })
    return
  }
  const response = await api.post('/links/save', row) as ApiResponse

  if (response?.Success) {
    toast.success(response.Message || '修改成功', { autoClose: 2000, position: 'top-center' })
    table.clearEdit()
    // 更新原始数据
    originalData.value.set(row.Id, { ...row })
  } else {
    toast.error(response?.Message || '修改失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 取消编辑
const cancelEdit = (row: LinkItem, table: any) => {
  const original = originalData.value.get(row.Id)
  if (original) {
    Object.assign(row, original)
  }
  table.clearEdit()
}

// 删除友情链接
const deleteLink = async (row: LinkItem) => {
  const response = await api.post(`/links/delete/${row.Id}`) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
    loadLinks()
  } else {
    toast.error(response?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 检查链接
const checkLink = async (row: LinkItem) => {
  try {
    toast.success('正在检查链接 ' + row.Name, { autoClose: 5000, position: 'top-center' })
    const response = await api.post('/links/check', { link: row.Url }) as ApiResponse
    if (response?.Message) {
      toast.success(response.Message, { autoClose: 5000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('检查链接失败: ' + row.Name, { autoClose: 2000, position: 'top-center' })
    console.error('Error checking link:', error)
  }
}

// 切换白名单状态
const toggleWhitelist = async (row: LinkItem) => {
  try {
    await api.post(`/links/ToggleWhitelist/${row.Id}`)
    // 状态已经通过 v-model 更新了
  } catch (error) {
    // 如果失败，恢复原状态
    row.Except = !row.Except
    toast.error('切换白名单状态失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error toggling whitelist:', error)
  }
}

// 切换推荐状态
const toggleRecommend = async (row: LinkItem) => {
  try {
    await api.post(`/links/ToggleRecommend/${row.Id}`)
    // 状态已经通过 v-model 更新了
  } catch (error) {
    // 如果失败，恢复原状态
    row.Recommend = !row.Recommend
    toast.error('切换推荐状态失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error toggling recommend:', error)
  }
}

// 切换可用状态
const toggleStatus = async (row: LinkItem) => {
  try {
    await api.post(`/links/Toggle/${row.Id}`)
    // 切换状态值
    row.Status = row.Status === 1 ? 0 : 1
  } catch (error) {
    toast.error('切换可用状态失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error toggling status:', error)
  }
}

// 日期格式化
const formatDateTime = (dateStr: string): string => {
  return dayjs(dateStr).format('YYYY-MM-DD HH:mm:ss')
}

// 生命周期钩子
onMounted(() => {
  loadLinks()

  // 监听表格编辑开始事件，备份原始数据
  if (tableRef.value) {
    tableRef.value.$on('edit-actived', ({ row }: { row: LinkItem }) => {
      originalData.value.set(row.Id, { ...row })
    })
  }
})
</script>
<style scoped lang="scss">
.links-container {
  padding: 20px;
}

// 响应式设计
@media (max-width: 768px) {
  .links-container {
    padding: 10px;
  }

  .q-btn-group {
    flex-direction: column;

    .q-btn {
      margin-bottom: 8px;
    }
  }
}
</style>