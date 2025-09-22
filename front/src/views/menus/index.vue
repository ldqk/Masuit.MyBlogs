<template>
<div class="menu-container">
  <!-- 页面标题 -->
  <div class="text-h4 q-mb-lg">导航菜单管理</div>
  <!-- 操作工具栏 -->
  <div class="q-mb-lg">
    <div class="row items-center justify-between">
      <div class="col-auto">
        <div class="q-gutter-sm">
          <q-btn color="positive" icon="add" label="新增菜单" @click="addRootMenu" />
          <q-btn color="info" icon="unfold_more" :label="expanded ? '收起全部' : '展开全部'" @click="toggleExpandAll" />
          <q-btn color="primary" icon="refresh" label="刷新" @click="loadMenus" :loading="loading" />
        </div>
      </div>
      <div class="col-auto">
        <q-input v-model="searchQuery" outlined dense placeholder="搜索菜单名称" autofocus debounce="100" @update:model-value="expandAllNodes">
          <template v-slot:prepend>
            <q-icon name="search" />
          </template>
        </q-input>
      </div>
    </div>
  </div>
  <!-- 菜单树 -->
  <q-card>
    <q-card-section>
      <q-tree ref="treeRef" :nodes="filteredNodes" node-key="Id" label-key="Name" children-key="Children" v-model:expanded="expandedKeys" :filter="searchQuery" :filter-method="filterMethod" no-connectors @update:expanded="onExpandedUpdate" @lazy-load="onLazyLoad">
        <template v-slot:default-header="prop">
          <div class="row items-center full-width menu-node">
            <!-- 菜单信息 -->
            <div class="col">
              <div class="row items-center q-gutter-sm">
                <div class="text-weight-bold">
                  <a :href="prop.node.Url.startsWith('http') ? prop.node.Url : globalConfig.baseURL + prop.node.Url" target="_blank" class="text-primary" v-if="prop.node.Url && prop.node.Url !== '#'"> {{ prop.node.Id }}：{{ prop.node.Name }} </a>
                  <span v-else class="text-grey-8">{{ prop.node.Id }}：{{ prop.node.Name }}</span>
                </div>
                <q-chip v-if="prop.node.MenuType !== undefined" :label="getMenuTypeName(prop.node.MenuType)" size="sm" color="blue-grey-3" text-color="white" />
              </div>
              <div class="text-caption text-grey-7 q-mt-xs">
                <span v-if="prop.node.Url">URL：{{ prop.node.Url }}</span>
                <span v-if="prop.node.Sort" class="q-ml-md">排序：{{ prop.node.Sort }}</span>
                <span v-if="prop.node.Icon" class="q-ml-md">
                  <img :src="prop.node.Icon" style="height: 20px; vertical-align: middle;" class="q-mr-xs" />
                  图标 </span>
              </div>
            </div>
            <!-- 操作按钮 -->
            <div class="col-auto">
              <div class="q-gutter-xs">
                <q-btn size="sm" color="primary" icon="add" dense round @click.stop="addSubMenu(prop.node)" v-if="!prop.node.Url || prop.node.Url === '#'">
                  <q-tooltip>添加子菜单</q-tooltip>
                </q-btn>
                <q-btn size="sm" color="purple" icon="arrow_upward" dense round class="sort-btn" @click.stop="moveMenuUp(prop.node)">
                  <q-tooltip>上移</q-tooltip>
                </q-btn>
                <q-btn size="sm" color="purple" icon="arrow_downward" dense round class="sort-btn" @click.stop="moveMenuDown(prop.node)">
                  <q-tooltip>下移</q-tooltip>
                </q-btn>
                <q-btn size="sm" color="info" icon="edit" dense round @click.stop="editMenu(prop.node)">
                  <q-tooltip>编辑菜单</q-tooltip>
                </q-btn>
                <q-btn size="sm" color="negative" icon="delete" dense round @click.stop>
                  <q-tooltip>删除菜单</q-tooltip>
                  <q-popup-proxy transition-show="scale" transition-hide="scale">
                    <q-card style="min-width: 300px;">
                      <q-card-section class="row items-center">
                        <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                        <div>
                          <div class="text-h6">确认删除</div>
                          <div class="text-subtitle2">确认删除菜单【{{ prop.node.Name }}】吗？<br />此操作将同时删除其所有子菜单！</div>
                        </div>
                      </q-card-section>
                      <q-card-actions align="right">
                        <q-btn flat label="确认" color="negative" v-close-popup @click="deleteMenu(prop.node)" />
                        <q-btn flat label="取消" color="primary" v-close-popup />
                      </q-card-actions>
                    </q-card>
                  </q-popup-proxy>
                </q-btn>
              </div>
            </div>
          </div>
        </template>
      </q-tree>
    </q-card-section>
  </q-card>
  <!-- 编辑菜单对话框 -->
  <q-dialog v-model="showDialog" persistent>
    <q-card style="min-width: 650px; max-width: 800px">
      <q-card-section class="bg-primary text-white">
        <div class="row">
          <div class="col text-h6 flex items-center">
            <q-icon name="edit" class="q-mr-sm" /> {{ dialogTitle }}
          </div>
          <q-btn color="primary" round icon="close" class="text-right" v-close-popup />
        </div>
      </q-card-section>
      <q-card-section class="q-pa-lg">
        <q-form @submit="submitMenu" class="menu-form">
          <!-- 基本信息分组 -->
          <div class="form-section">
            <div class="section-title">
              <q-icon name="info" class="q-mr-xs" /> 基本信息
            </div>
            <div class="row q-gutter-md q-mb-md">
              <div class="col-12">
                <q-input v-model="currentMenu.Name" label="菜单名称" outlined :rules="[val => !!val || '请输入菜单名称']" lazy-rules>
                  <template v-slot:prepend>
                    <q-icon name="label" />
                  </template>
                </q-input>
              </div>
            </div>
            <div class="row q-gutter-md q-mb-md">
              <div class="col-12">
                <q-select v-model="currentMenu.ParentId" :options="parentMenuOptions" option-label="label" option-value="value" emit-value map-options label="父级菜单" outlined :hint="currentMenu.ParentId ? '已选择父级菜单，此菜单将成为子菜单' : '未选择父级菜单，此菜单将是根级菜单'">
                  <template v-slot:prepend>
                    <q-icon name="account_tree" />
                  </template>
                  <template v-slot:no-option>
                    <q-item>
                      <q-item-section class="text-grey"> 暂无可用的父级菜单（只有无链接的菜单才能作为父级） </q-item-section>
                    </q-item>
                  </template>
                  <template v-slot:after-options>
                    <q-item>
                      <q-item-section class="text-caption text-blue-grey-6">
                        <q-icon name="info" size="xs" class="q-mr-xs" /> 提示：只有URL为空或"#"的菜单才能作为父级菜单 </q-item-section>
                    </q-item>
                  </template>
                </q-select>
              </div>
            </div>
            <div class="row q-gutter-md q-mb-md">
              <div class="col-12">
                <q-input v-model="currentMenu.Url" label="URL地址" outlined hint="留空或#表示父级菜单">
                  <template v-slot:prepend>
                    <q-icon name="link" />
                  </template>
                  <template v-slot:hint>
                    <div class="text-caption">
                      <q-icon name="info" size="xs" class="q-mr-xs" /> 留空或填写"#"表示这是一个父级菜单，可以包含子菜单
                    </div>
                  </template>
                </q-input>
              </div>
            </div>
          </div>
          <!-- 配置信息分组 -->
          <div class="form-section">
            <div class="section-title">
              <q-icon name="settings" class="q-mr-xs" /> 配置信息
            </div>
            <div class="row q-gutter-md q-mb-md">
              <div class="col">
                <q-input v-model.number="currentMenu.Sort" label="排序号" outlined type="number" min="0" step="10">
                  <template v-slot:prepend>
                    <q-icon name="sort" />
                  </template>
                  <template v-slot:hint> 数值越小排序越靠前 </template>
                </q-input>
              </div>
              <div class="col">
                <q-select v-model="currentMenu.MenuType" :options="menuTypeOptions" option-label="name" option-value="e" emit-value map-options label="菜单类型" outlined>
                  <template v-slot:prepend>
                    <q-icon name="category" />
                  </template>
                </q-select>
              </div>
            </div>
            <div class="row q-mb-md">
              <div class="col-12">
                <q-card flat bordered class="bg-grey-1 q-pa-md">
                  <div class="row items-center">
                    <q-checkbox v-model="currentMenu.NewTab" label="在新标签页中打开链接" class="text-weight-medium" />
                    <q-space />
                    <q-icon name="open_in_new" color="grey-6" />
                  </div>
                  <div class="text-caption text-grey-7 q-mt-xs"> 启用后，点击此菜单链接将在新的浏览器标签页中打开 </div>
                </q-card>
              </div>
            </div>
          </div>
          <!-- 图标设置分组 -->
          <div class="form-section">
            <div class="section-title">
              <q-icon name="image" class="q-mr-xs" /> 图标设置
            </div>
            <div class="row q-gutter-md q-mb-md">
              <div class="col">
                <q-input v-model="currentMenu.Icon" label="图标地址" outlined hint="请输入图标URL或上传图片" clearable>
                  <template v-slot:prepend>
                    <q-icon name="insert_photo" />
                  </template>
                  <template v-slot:append>
                    <q-btn color="positive" icon="upload" label="上传图片" @click="showUpload = true" />
                  </template>
                </q-input>
              </div>
            </div>
            <!-- 图标预览 -->
            <div v-if="currentMenu.Icon" class="icon-preview-card">
              <q-card flat bordered class="bg-grey-1">
                <q-card-section class="text-center q-py-md">
                  <div class="text-caption text-grey-7 q-mb-sm">图标预览</div>
                  <div class="icon-preview-container">
                    <img :src="currentMenu.Icon?.startsWith('http') ? currentMenu.Icon : globalConfig.baseURL + currentMenu.Icon" class="icon-preview-image" @error="handleImageError" @load="handleImageLoad" style="min-height:160px;max-width: 100%; height: auto;" />
                  </div>
                  <div class="text-caption text-grey-6 q-mt-sm"> {{ currentMenu.Icon }} </div>
                </q-card-section>
              </q-card>
            </div>
            <div v-else class="text-center q-py-md">
              <q-icon name="image" size="48px" color="grey-4" />
              <div class="text-caption text-grey-6 q-mt-sm"> 暂无图标，可上传图片或输入图标URL </div>
            </div>
          </div>
        </q-form>
      </q-card-section>
      <q-separator />
      <q-card-actions align="right" class="q-pa-lg bg-grey-1">
        <q-btn flat label="取消" color="grey-7" @click="cancelEdit" class="q-mr-sm" />
        <q-btn label="保存" color="primary" @click="submitMenu" :loading="submitting" unelevated icon="save" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <!-- 图片上传对话框 -->
  <q-dialog v-model="showUpload" persistent>
    <q-card style="min-width: 400px">
      <q-card-section>
        <div class="text-h6">上传图标</div>
      </q-card-section>
      <q-card-section class="q-pt-none">
        <q-file v-model="uploadFile" label="选择图片文件" outlined accept="image/*" @input="handleFileSelect">
          <template v-slot:prepend>
            <q-icon name="image" />
          </template>
        </q-file>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" color="grey" @click="showUpload = false" />
        <q-btn label="上传" color="primary" @click="uploadImage" :loading="uploading" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, computed, getCurrentInstance } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'
import globalConfig from '@/config'

// 类型定义
interface Menu {
  Id: number
  Name: string
  Url?: string
  Sort: number
  MenuType: number
  NewTab?: boolean
  Icon?: string
  ParentId?: number | null
  Children?: Menu[]
}

interface ApiResponse {
  Data?: any
  Message?: string
  Success?: boolean
}

interface MenuType {
  e: number
  name: string
}

// 响应式数据
const loading = ref(false)
const submitting = ref(false)
const expanded = ref(false)
const searchQuery = ref('')
const treeData = ref<Menu[]>([])
const expandedKeys = ref<number[]>([])

// 对话框相关
const showDialog = ref(false)
const showUpload = ref(false)
const dialogTitle = ref('')
const currentMenu = ref<Partial<Menu>>({})
const uploading = ref(false)
const uploadFile = ref<File | null>(null)

// 菜单类型选项
const menuTypeOptions = ref<MenuType[]>([])

// 父级菜单选项
const parentMenuOptions = ref<{ label: string; value: number | null }[]>([])

// 树组件引用
const treeRef = ref(null)

// 计算属性
const filteredNodes = computed(() => {
  if (!searchQuery.value) {
    return treeData.value
  }
  return filterNodes(treeData.value, searchQuery.value)
})

// 方法
const filterNodes = (nodes: Menu[], query: string): Menu[] => {
  return nodes.reduce((acc: Menu[], node) => {
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

const filterMethod = (node: Menu, filter: string) => {
  return node.Name.toLowerCase().includes(filter.toLowerCase())
}

const getMenuTypeName = (menuType: number) => {
  const type = menuTypeOptions.value.find(t => t.e === menuType)
  return type ? type.name : '未知'
}

// 根据ID查找菜单
const findMenuById = (nodes: Menu[], targetId: number): Menu | null => {
  for (const node of nodes) {
    if (node.Id === targetId) {
      return node
    }
    if (node.Children) {
      const found = findMenuById(node.Children, targetId)
      if (found) return found
    }
  }
  return null
}

// 检查是否是子菜单（防止循环引用）
const isDescendant = (parentId: number, childId: number, nodes: Menu[]): boolean => {
  const parent = findMenuById(nodes, parentId)
  if (!parent || !parent.Children) return false

  for (const child of parent.Children) {
    if (child.Id === childId) return true
    if (child.Children && isDescendant(child.Id, childId, nodes)) return true
  }
  return false
}

// 生成父级菜单选项
const generateParentMenuOptions = (excludeId?: number) => {
  const options: { label: string; value: number | null }[] = [
    { label: '无（根级菜单）', value: null }
  ]

  const collectParentMenus = (nodes: Menu[], pathSegments: string[] = []) => {
    nodes.forEach(node => {
      // 构建当前节点的路径
      const currentPath = [...pathSegments, node.Name]

      // 排除当前编辑的菜单
      if (excludeId && node.Id === excludeId) {
        // 即使排除当前菜单，也要继续遍历其子菜单
        if (node.Children && node.Children.length > 0) {
          collectParentMenus(node.Children, currentPath)
        }
        return
      }

      // 排除当前菜单的子菜单（防止循环引用）
      if (excludeId && isDescendant(excludeId, node.Id, treeData.value)) {
        return
      }

      // 只有没有URL或URL为#的菜单才能作为父级菜单
      if (!node.Url || node.Url === '#') {
        // 构建全路径显示
        const fullPath = currentPath.join(' > ')
        options.push({
          label: `${fullPath}`,
          value: node.Id
        })
      }

      // 递归收集所有子菜单（无论父级是否有URL）
      if (node.Children && node.Children.length > 0) {
        collectParentMenus(node.Children, currentPath)
      }
    })
  }

  collectParentMenus(treeData.value)
  return options
}

// 数据加载
const loadMenus = async () => {
  loading.value = true
  try {
    const response = await api.get('/menu/getmenus') as ApiResponse
    if (response?.Data) {
      treeData.value = response.Data

      // 初始化时展开第一级节点
      if (expandedKeys.value.length === 0) {
        expandedKeys.value = treeData.value.map(node => node.Id)
      }
    }
  } catch (error) {
    console.error('加载菜单失败:', error)
    toast.error('加载菜单失败', { autoClose: 3000, position: 'top-center' })
  } finally {
    loading.value = false
  }
}

// 加载菜单类型
const loadMenuTypes = async () => {
  try {
    const response = await api.get('/menu/getmenutype') as ApiResponse
    if (response?.Data) {
      menuTypeOptions.value = response.Data
    }
  } catch (error) {
    console.error('加载菜单类型失败:', error)
    toast.error('加载菜单类型失败', { autoClose: 3000, position: 'top-center' })
  }
}

// 展开/收起功能
const expandAllNodes = () => {
  const keys: number[] = []
  const collectKeys = (nodes: Menu[]) => {
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

const onLazyLoad = () => {
  // 暂不实现懒加载
}

// 排序功能
const findMenuInTree = (nodes: Menu[], targetId: number, parent: Menu | null = null): {
  menu: Menu,
  parent: Menu | null,
  siblings: Menu[],
  index: number
} | null => {
  for (let i = 0; i < nodes.length; i++) {
    if (nodes[i].Id === targetId) {
      return {
        menu: nodes[i],
        parent,
        siblings: nodes,
        index: i
      }
    }
    if (nodes[i].Children) {
      const result = findMenuInTree(nodes[i].Children!, targetId, nodes[i])
      if (result) return result
    }
  }
  return null
}

const updateMenuSort = async (menu: Menu, newSort: number) => {
  try {
    const updatedMenu = { ...menu, Sort: newSort }
    const response = await api.post('/menu/save', updatedMenu) as ApiResponse
    if (response?.Success !== true) {
      toast.error('排序更新失败', { autoClose: 3000, position: 'top-center' })
    }
  } catch (error) {
    console.error('更新排序失败:', error)
    toast.error('更新排序失败', { autoClose: 3000, position: 'top-center' })
  }
}

const moveMenuUp = async (menu: Menu) => {
  const menuInfo = findMenuInTree(treeData.value, menu.Id)
  if (!menuInfo || menuInfo.index === 0) return

  const { siblings, index } = menuInfo
  const prevMenu = siblings[index - 1]

  // 交换排序值
  const tempSort = menu.Sort
  await updateMenuSort(menu, prevMenu.Sort)
  await updateMenuSort(prevMenu, tempSort)
  loadMenus()
}

const moveMenuDown = async (menu: Menu) => {
  const menuInfo = findMenuInTree(treeData.value, menu.Id)
  if (!menuInfo || menuInfo.index === menuInfo.siblings.length - 1) return

  const { siblings, index } = menuInfo
  const nextMenu = siblings[index + 1]

  // 交换排序值
  const tempSort = menu.Sort
  await updateMenuSort(menu, nextMenu.Sort)
  await updateMenuSort(nextMenu, tempSort)
  loadMenus()
}

// CRUD操作
const addRootMenu = () => {
  const lastMenu = treeData.value[treeData.value.length - 1]
  currentMenu.value = {
    Name: '',
    Url: '',
    Sort: lastMenu ? lastMenu.Sort + (lastMenu.Children?.length || 0 + 1) * 10 : 10,
    MenuType: 0,
    NewTab: false,
    Icon: '',
    ParentId: null
  }
  // 生成父级菜单选项
  parentMenuOptions.value = generateParentMenuOptions()
  dialogTitle.value = '新增根菜单'
  showDialog.value = true
}

const addSubMenu = (parentNode: Menu) => {
  if (parentNode.Url && parentNode.Url !== '#') {
    toast.error(`菜单【${parentNode.Name}】是一个有链接的菜单，不能作为父级菜单`, {
      autoClose: 3000,
      position: 'top-center'
    })
    return
  }

  currentMenu.value = {
    Name: '',
    Url: '',
    Sort: (parentNode.Sort + (parentNode.Children?.length || 0) + 1) * 10,
    MenuType: parentNode.MenuType,
    NewTab: false,
    Icon: '',
    ParentId: parentNode.Id
  }
  // 生成父级菜单选项
  parentMenuOptions.value = generateParentMenuOptions()
  dialogTitle.value = `新增子菜单到 "${parentNode.Name}"`
  showDialog.value = true
}

const editMenu = (menu: Menu) => {
  currentMenu.value = { ...menu }
  // 生成父级菜单选项，排除当前菜单
  parentMenuOptions.value = generateParentMenuOptions(menu.Id)
  dialogTitle.value = '编辑菜单'
  showDialog.value = true
}

const cancelEdit = () => {
  showDialog.value = false
  currentMenu.value = {}
}

const submitMenu = async () => {
  if (!currentMenu.value.Name?.trim()) {
    toast.error('请输入菜单名称', { autoClose: 3000, position: 'top-center' })
    return
  }

  // 验证父级菜单选择的合法性
  if (currentMenu.value.ParentId) {
    const parentMenu = findMenuById(treeData.value, currentMenu.value.ParentId)
    if (parentMenu && parentMenu.Url && parentMenu.Url !== '#') {
      toast.error('所选父级菜单有链接地址，不能作为父级菜单', {
        autoClose: 3000,
        position: 'top-center'
      })
      return
    }
  }

  submitting.value = true
  try {
    // 清理空的图标字段
    if (!currentMenu.value.Icon || currentMenu.value.Icon.trim() === '') {
      currentMenu.value.Icon = null
    }

    const response = await api.post('/menu/save', currentMenu.value) as ApiResponse

    if (response?.Success !== false) {
      toast.success(response?.Message || '操作成功', {
        autoClose: 3000,
        position: 'top-center'
      })
      showDialog.value = false
      currentMenu.value = {}
      await loadMenus()
    } else {
      toast.error(response?.Message || '操作失败', {
        autoClose: 3000,
        position: 'top-center'
      })
    }
  } catch (error) {
    console.error('保存菜单失败:', error)
    toast.error('保存菜单失败', { autoClose: 3000, position: 'top-center' })
  } finally {
    submitting.value = false
  }
}

const deleteMenu = async (menu: Menu) => {
  try {
    const response = await api.post(`/menu/delete/${menu.Id}`) as ApiResponse
    toast.success(response?.Message || '删除成功', {
      autoClose: 3000,
      position: 'top-center'
    })
    await loadMenus()
  } catch (error) {
    console.error('删除菜单失败:', error)
    toast.error('删除菜单失败', { autoClose: 3000, position: 'top-center' })
  }
}

// 文件上传
const handleFileSelect = (file: File | null) => {
  uploadFile.value = file
}

// 图片预览处理
const handleImageError = (event: Event) => {
  const img = event.target as HTMLImageElement
  img.style.display = 'none'
  // 可以显示一个错误图标或占位符
}

const handleImageLoad = (event: Event) => {
  const img = event.target as HTMLImageElement
  img.style.display = 'block'
}

const uploadImage = async () => {
  if (!uploadFile.value) {
    toast.error('请选择图片文件', { autoClose: 3000, position: 'top-center' })
    return
  }

  uploading.value = true
  try {
    const formData = new FormData()
    formData.append('file', uploadFile.value)

    const response = await api.post('/Upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    }) as ApiResponse

    if (response?.Data) {
      currentMenu.value.Icon = response.Data
      toast.success('图片上传成功', { autoClose: 3000, position: 'top-center' })
      showUpload.value = false
      uploadFile.value = null
    } else {
      toast.error('图片上传失败', { autoClose: 3000, position: 'top-center' })
    }
  } catch (error) {
    console.error('上传图片失败:', error)
    toast.error('上传图片失败', { autoClose: 3000, position: 'top-center' })
  } finally {
    uploading.value = false
  }
}

// 生命周期钩子
onMounted(() => {
  loadMenus()
  loadMenuTypes()
})
</script>
<style lang="scss" scoped>
.menu-container {
  padding: 16px;
}

.text-primary {
  color: #1976d2;
  text-decoration: none;
}

.text-primary:hover {
  text-decoration: underline;
}

.menu-node {
  padding: 8px 12px;
  border-radius: 4px;
  margin-bottom: 4px;
  background: #f8faff;
  border: 1px solid #dae2ea;
  transition: all 0.2s ease;
}

.menu-node:hover {
  background: #f4f6f7;
  border-color: #dce2e8;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

:deep(.q-tree__node-header) {
  padding: 0;
  border-radius: 4px;
}

:deep(.q-tree__node-header):hover {
  background: transparent;
}

:deep(.q-tree__node-body) {
  padding-left: 20px;
}

// 拖拽样式
:deep(.q-tree__node--selected) {
  .menu-node {
    background: #e3f2fd;
    border-color: #1976d2;
  }
}

:deep(.q-tree__node--disabled) {
  opacity: 0.6;
}

// 拖拽排序相关样式
:deep(.q-tree__node--dragging) {
  .menu-node {
    opacity: 0.8;
    background: #fff3cd;
    border-color: #ffeaa7;
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
  }
}

:deep(.q-tree__node--drag-over) {
  .menu-node {
    background: #e8f5e8;
    border-color: #4caf50;
    border-style: dashed;
  }
}

:deep(.q-tree__node--drag-over-invalid) {
  .menu-node {
    background: #ffebee;
    border-color: #f44336;
    border-style: dashed;
  }
}

// 排序按钮样式
.q-btn.sort-btn {
  transition: all 0.2s ease;
}

.q-btn.sort-btn:hover {
  transform: scale(1.1);
}

// 拖拽指示器样式
.drag-handle {
  cursor: grab;
  opacity: 0.6;
  transition: opacity 0.2s ease;
}

.menu-node:hover .drag-handle {
  opacity: 1;
}

.drag-handle:active {
  cursor: grabbing;
}

// 菜单类型标签样式
.q-chip {
  font-size: 10px;
  height: 20px;
  line-height: 20px;
}

// 表单样式优化
.menu-form {
  .form-section {
    margin-bottom: 24px;

    .section-title {
      font-size: 16px;
      font-weight: 600;
      color: #1976d2;
      margin-bottom: 16px;
      display: flex;
      align-items: center;
      padding: 8px 0;
      border-bottom: 2px solid #e1f5fe;
    }
  }
}

// 图标预览样式
.icon-preview-card {
  margin-top: 16px;

  .icon-preview-container {
    display: flex;
    justify-content: center;
    align-items: center;
    min-height: 60px;

    .icon-preview-image {
      max-height: 50px;
      max-width: 50px;
      object-fit: contain;
      border-radius: 4px;
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
      background: white;
      padding: 4px;
    }
  }
}

// 对话框头部样式优化
:deep(.q-dialog .q-card) {
  border-radius: 12px;
  overflow: hidden;

  .bg-primary {
    background: linear-gradient(135deg, #1976d2 0%, #42a5f5 100%);
  }
}

// 输入框样式优化
:deep(.q-field--outlined .q-field__control) {
  border-radius: 8px;
}

:deep(.q-field--outlined.q-field--focused .q-field__control) {
  border-color: #1976d2;
  box-shadow: 0 0 0 1px rgba(25, 118, 210, 0.2);
}

// 按钮样式优化
:deep(.q-btn--unelevated) {
  border-radius: 8px;
}

// 复选框卡片样式
:deep(.bg-grey-1) {
  border-radius: 8px;
}
</style>