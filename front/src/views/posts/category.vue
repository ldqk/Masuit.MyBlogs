<template>
<div class="category-container">
  <!-- 页面标题 -->
  <div class="text-h4 q-mb-lg">分类管理</div>
  <!-- 操作工具栏 -->
  <div class="q-mb-lg">
    <div class="row items-center justify-between">
      <div class="col-auto">
        <div class="q-gutter-sm">
          <q-btn color="positive" icon="add" label="新增分类" @click="addRootCategory" />
          <q-btn color="info" icon="unfold_more" :label="expanded ? '收起全部' : '展开全部'" @click="toggleExpandAll" />
          <q-btn color="primary" icon="refresh" label="刷新" @click="loadCategories" :loading="loading" />
        </div>
      </div>
      <div class="col-auto">
        <q-input v-model="searchQuery" outlined dense placeholder="搜索分类名称" debounce="100" @update:model-value="expandAllNodes">
          <template v-slot:prepend>
            <q-icon name="search" />
          </template>
        </q-input>
      </div>
    </div>
  </div>
  <!-- 分类树 -->
  <q-card>
    <q-card-section>
      <q-tree ref="treeRef" :nodes="filteredNodes" node-key="Id" label-key="Name" children-key="Children" v-model:expanded="expandedKeys" :filter="searchQuery" :filter-method="filterMethod" no-connectors @update:expanded="onExpandedUpdate">
        <template v-slot:default-header="prop">
          <div class="row items-center full-width">
            <!-- 分类信息 -->
            <div class="col">
              <div class="row items-center q-gutter-sm">
                <div class="text-weight-bold">
                  <a :href="`/cat/${prop.node.Id}`" target="_blank" class="text-primary"> {{ prop.node.Id }}：{{ prop.node.Name }} </a>
                </div>
              </div>
              <div class="text-caption text-grey-7 q-mt-xs" v-if="prop.node.Description"> {{ prop.node.Description }} </div>
            </div>
            <!-- 操作按钮 -->
            <div class="col-auto">
              <div class="q-gutter-xs">
                <!-- 主要操作按钮 -->
                <q-btn size="sm" color="primary" icon="add" dense round @click.stop="addSubCategory(prop.node)">
                  <q-tooltip>添加子分类</q-tooltip>
                </q-btn>
                <q-btn size="sm" color="info" icon="edit" dense round @click.stop="editCategory(prop.node)">
                  <q-tooltip>编辑分类</q-tooltip>
                </q-btn>
                <q-btn size="sm" color="negative" icon="delete" dense round @click.stop="deleteCategory(prop.node)" :disable="prop.node.Id === 1">
                  <q-tooltip>删除分类</q-tooltip>
                </q-btn>
              </div>
            </div>
          </div>
        </template>
      </q-tree>
    </q-card-section>
  </q-card>
  <!-- 编辑分类对话框 -->
  <q-dialog v-model="showDialog" persistent>
    <q-card style="min-width: 500px">
      <q-card-section>
        <div class="text-h6">{{ dialogTitle }}</div>
      </q-card-section>
      <q-card-section class="q-pt-none">
        <q-form @submit="submitCategory" class="q-gutter-md">
          <q-input v-model="currentCategory.Name" label="分类名称" outlined dense :rules="[val => !!val || '请输入分类名称']" lazy-rules />
          <q-input v-model="currentCategory.Description" label="分类描述" outlined dense type="textarea" rows="3" />
        </q-form>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" color="grey" @click="cancelEdit" />
        <q-btn label="确认" color="primary" @click="submitCategory" :loading="submitting" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <!-- 删除分类对话框 -->
  <q-dialog v-model="showDeleteDialog" persistent>
    <q-card style="min-width: 500px">
      <q-card-section>
        <div class="text-h6">删除分类</div>
      </q-card-section>
      <q-card-section class="q-pt-none">
        <div class="q-mb-md"> 确定要删除分类 <strong>{{ categoryToDelete?.Name }}</strong> 吗？ </div>
        <div class="q-mb-md"> 删除后将该分类下的所有文章移动到： </div>
        <q-select v-model="targetCategoryId" :options="categoryOptions" option-label="Name" option-value="Id" emit-value map-options outlined dense label="选择目标分类" :rules="[val => !!val || '请选择一个分类']" />
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" color="grey" @click="cancelDelete" />
        <q-btn label="确认删除" color="negative" @click="confirmDelete" :loading="deleting" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'

// 类型定义
interface Category {
  Id: number
  Name: string
  Description?: string
  ParentId?: number | null
  Children?: Category[]
}

interface ApiResponse {
  Data?: any
  Message?: string
  Success?: boolean
}

// 响应式数据
const loading = ref(false)
const submitting = ref(false)
const deleting = ref(false)
const expanded = ref(false)
const searchQuery = ref('')
const treeData = ref<Category[]>([])
const expandedKeys = ref<number[]>([])

// 对话框相关
const showDialog = ref(false)
const showDeleteDialog = ref(false)
const dialogTitle = ref('')
const currentCategory = ref<Partial<Category>>({})
const categoryToDelete = ref<Category | null>(null)
const targetCategoryId = ref<number | null>(null)

// 树组件引用
const treeRef = ref(null)

// 计算属性
const filteredNodes = computed(() => {
  if (!searchQuery.value) {
    return treeData.value
  }
  return filterNodes(treeData.value, searchQuery.value)
})

const categoryOptions = computed(() => {
  const flatCategories = flattenCategories(treeData.value)
  return flatCategories.filter(cat => cat.Id !== categoryToDelete.value?.Id)
})

// 方法
const filterNodes = (nodes: Category[], query: string): Category[] => {
  return nodes.reduce((acc: Category[], node) => {
    const matchesQuery = node.Name.toLowerCase().includes(query.toLowerCase())
    const filteredChildren = node.Children ? filterNodes(node.Children, query) : []
    if (matchesQuery || filteredChildren.length > 0) {
      acc.push({
        ...node,
        Children: filteredChildren
      })
    }

    return acc
  }, [])
}

const flattenCategories = (categories: Category[]): Category[] => {
  const result: Category[] = []
  const flatten = (cats: Category[]) => {
    cats.forEach(cat => {
      result.push(cat)
      if (cat.Children) {
        flatten(cat.Children)
      }
    })
  }

  flatten(categories)
  return result
}

const filterMethod = (node: Category, filter: string) => {
  return node.Name.toLowerCase().includes(filter.toLowerCase())
}

// 数据加载
const loadCategories = async () => {
  loading.value = true
  const data = await api.get('/category/GetCategories') as ApiResponse
  if (data?.Data) {
    treeData.value = data.Data

    // 初始化时展开第一级节点
    if (expandedKeys.value.length === 0) {
      expandedKeys.value = treeData.value.map(node => node.Id)
    }
  }
  loading.value = false
}

// 展开/收起功能
const expandAllNodes = () => {
  const keys: number[] = []
  const collectKeys = (nodes: Category[]) => {
    nodes.forEach(node => {
      keys.push(node.Id)
      if (node.Children) {
        collectKeys(node.Children)
      }
    })
  }
  collectKeys(treeData.value)
  expandedKeys.value = keys
}

const toggleExpandAll = () => {
  expanded.value = !expanded.value
  if (expanded.value) {
    expandAllNodes()
  } else {
    expandedKeys.value = []
  }
}

const onExpandedUpdate = (keys: number[]) => {
  expandedKeys.value = keys
}

// CRUD操作
const addRootCategory = () => {
  currentCategory.value = {
    Name: '',
    Description: '',
    ParentId: null
  }
  dialogTitle.value = '添加根分类'
  showDialog.value = true
}

const addSubCategory = (parentNode: Category) => {
  currentCategory.value = {
    Name: '',
    Description: '',
    ParentId: parentNode.Id
  }
  dialogTitle.value = `添加子分类到 "${parentNode.Name}"`
  showDialog.value = true
}

const editCategory = (category: Category) => {
  currentCategory.value = { ...category }
  dialogTitle.value = '编辑分类'
  showDialog.value = true
}

const cancelEdit = () => {
  showDialog.value = false
  currentCategory.value = {}
}

const submitCategory = async () => {
  if (!currentCategory.value.Name?.trim()) {
    toast.error('请输入分类名称', { autoClose: 2000, position: 'top-center' })
    return
  }

  submitting.value = true
  const data = await api.post('/category/save', currentCategory.value) as ApiResponse
  toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
  showDialog.value = false
  currentCategory.value = {}
  await loadCategories()
  submitting.value = false
}

const deleteCategory = async (category: Category) => {
  if (category.Id === 1) {
    toast.error('默认分类不能被删除！', { autoClose: 2000, position: 'top-center' })
    return
  }

  categoryToDelete.value = category
  targetCategoryId.value = null
  showDeleteDialog.value = true
}

const cancelDelete = () => {
  showDeleteDialog.value = false
  categoryToDelete.value = null
  targetCategoryId.value = null
}

const confirmDelete = async () => {
  if (!targetCategoryId.value) {
    toast.error('请选择一个分类', { autoClose: 2000, position: 'top-center' })
    return
  }

  if (categoryToDelete.value?.Id === 1) {
    toast.error('默认分类不能被删除！', { autoClose: 2000, position: 'top-center' })
    return
  }

  deleting.value = true
  const url = `/category/delete?id=${categoryToDelete.value?.Id}&cid=${targetCategoryId.value}`
  const data = await api.post(url) as ApiResponse

  toast.success(data?.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
  showDeleteDialog.value = false
  categoryToDelete.value = null
  targetCategoryId.value = null
  await loadCategories()
  deleting.value = false
}

// 移动功能
const findNodeParent = (nodes: Category[], targetId: number, parent: Category | null = null): { parent: Category | null, siblings: Category[], index: number } | null => {
  for (let i = 0; i < nodes.length; i++) {
    if (nodes[i].Id === targetId) {
      return { parent, siblings: nodes, index: i }
    }
    if (nodes[i].Children) {
      const result = findNodeParent(nodes[i].Children!, targetId, nodes[i])
      if (result) return result
    }
  }
  return null
}

// 生命周期钩子
onMounted(() => {
  loadCategories()
})
</script>
<style lang="scss" scoped>
.category-container {
  padding: 16px;
}

.text-primary {
  color: #1976d2;
  text-decoration: none;
}

.text-primary:hover {
  text-decoration: underline;
}

:deep(.q-tree__node-header) {
  padding: 8px 12px;
  border-radius: 4px;
  margin-bottom: 4px;
  background: #f8faff;
  border: 1px solid #dae2ea;
}

:deep(.q-tree__node-header):hover {
  background: #f4f6f7;
  border-color: #dce2e8;
}
</style>