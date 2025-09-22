<template>
<div class="misc-list-page">
  <q-card>
    <q-card-section>
      <!-- 顶部控制栏 -->
      <div class="q-mb-md">
        <q-btn color="info" icon="refresh" label="刷新" @click="loadPageData" :loading="loading" />
        <q-btn color="primary" icon="add" label="新建杂项" class="q-ml-sm" :to="`/misc/write`" />
      </div>
      <!-- 主表格 -->
      <vxe-table ref="tableRef" :data="tableData" :loading="loading" stripe border>
        <!-- 标题列 -->
        <vxe-column field="Title" title="标题">
          <template #default="{ row }">
            <a :href="`/misc/${row.Id}`" target="_blank" class="text-primary"> {{ row.Title }} </a>
          </template>
        </vxe-column>
        <!-- 发表时间列 -->
        <vxe-column field="PostDate" title="发表时间" sortable>
          <template #default="{ row }"> {{ dayjs(row.PostDate).format('YYYY-MM-DD') }} </template>
        </vxe-column>
        <!-- 修改时间列 -->
        <vxe-column field="ModifyDate" title="修改时间" sortable>
          <template #default="{ row }"> {{ dayjs(row.ModifyDate).format('YYYY-MM-DD') }} </template>
        </vxe-column>
        <!-- 操作列 -->
        <vxe-column title="操作" width="150" align="center" fixed="right">
          <template #default="{ row }">
            <q-btn-group push>
              <q-btn dense size="md" color="primary" icon="edit" :disable="loading" :to="`/misc/write?id=${row.Id}`">
                <q-tooltip>编辑</q-tooltip>
              </q-btn>
              <q-btn dense size="md" color="negative" icon="delete" :disable="loading">
                <q-tooltip>删除</q-tooltip>
                <q-popup-proxy transition-show="scale" transition-hide="scale">
                  <q-card>
                    <q-card-section class="row items-center">
                      <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                      <div>
                        <div class="text-h6">确认删除</div>
                        <div class="text-subtitle2">确认删除这条杂项吗？<br />{{ row.Title }}</div>
                      </div>
                    </q-card-section>
                    <q-card-actions align="right">
                      <q-btn flat label="确认" color="negative" v-close-popup @click="deleteMisc(row)" />
                      <q-btn flat label="取消" color="primary" v-close-popup />
                    </q-card-actions>
                  </q-card>
                </q-popup-proxy>
              </q-btn>
            </q-btn-group>
          </template>
        </vxe-column>
      </vxe-table>
      <!-- 分页组件 -->
      <div class="row justify-center q-mt-md">
        <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.size)" :max-pages="6" direction-links boundary-numbers @update:model-value="loadPageData" />
        <q-select class="q-ml-md" v-model="pagination.size" :options="[10, 20, 50, 100]" dense outlined label="每页显示" style="width: 80px" @update:model-value="loadPageData" />
        <span class="q-ml-sm text-caption">共 {{ pagination.total }} 条</span>
      </div>
    </q-card-section>
  </q-card>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'
import dayjs from 'dayjs'

// 定义接口类型
interface Misc {
  Id: number
  Title: string
  Content: string
  PostDate: string
  ModifyDate: string
}

interface ApiResponse {
  Success: boolean
  Message: string
  TotalCount?: number
  Data?: Misc[]
}

// 响应式数据
const tableRef = ref()
const loading = ref(false)
const tableData = ref<Misc[]>([])
const pagination = ref({
  page: 1,
  size: 10,
  total: 0
})

// 加载分页数据
const loadPageData = async () => {
  loading.value = true
  const response = await api.get(`/misc/getpagedata?page=${pagination.value.page}&size=${pagination.value.size}`) as ApiResponse

  if (response?.Data) {
    tableData.value = response.Data
    pagination.value.total = response.TotalCount || 0
  } else {
    toast.error('获取数据失败', { autoClose: 2000, position: 'top-center' })
  }
  loading.value = false
}

// 删除杂项
const deleteMisc = async (row: Misc) => {
  const response = await api.get(`/misc/delete/${row.Id}`) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
    await loadPageData()
  } else {
    toast.error(response?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 生命周期钩子
onMounted(() => {
  loadPageData()
})
</script>
<style scoped lang="scss">
.misc-list-page {
  padding: 20px;
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
      }
    }
  }
}
</style>