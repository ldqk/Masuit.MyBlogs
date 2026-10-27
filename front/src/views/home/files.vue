<template>
<div class="file-manager-page" :class="{ 'file-manager-page--dragging': dragActive }" @dragover.prevent="dragActive = true" @dragleave.prevent="dragActive = false" @drop.prevent="handleDrop">
  <q-card flat bordered class="file-manager-card">
    <q-card-section class="file-manager-toolbar row items-center q-col-gutter-sm">
      <div class="col-auto q-ml-auto file-manager-toolbar__actions">
        <q-btn-group>
          <q-btn color="positive" icon="create_new_folder" label="新建文件夹" no-caps @click="openDialog('new-folder')" />
          <q-btn color="secondary" icon="upload" label="上传" no-caps @click="openDialog('upload')" />
        </q-btn-group>
      </div>
      <div class="file-manager-root-path">
        <q-input v-model="rootPathInput" class="file-manager-root-path__input" dense outlined label="文件根目录" placeholder="留空使用默认目录" @keyup.enter="saveRootPath">
          <template #prepend>
            <q-icon name="folder_open" />
          </template>
          <template #append>
            <q-btn color="primary" flat dense icon="save" no-caps :loading="savingRoot" @click="saveRootPath" />
          </template>
        </q-input>
      </div>
      <div class="file-manager-search">
        <q-input v-model="searchQuery" dense outlined clearable debounce="150" placeholder="搜索当前目录">
          <template #prepend>
            <q-icon name="search" />
          </template>
          <template #append>
            <q-btn color="info" dense flat icon="refresh" :loading="loading" aria-label="刷新" @click="refresh">
              <q-tooltip>刷新</q-tooltip>
            </q-btn>
          </template>
        </q-input>
      </div>
      <div class="col-auto">
        <q-btn-group flat>
          <q-btn :color="viewMode === 'table' ? 'primary' : 'grey-7'" :flat="viewMode !== 'table'" icon="view_list" aria-label="列表视图" @click="setViewMode('table')">
            <q-tooltip>列表视图</q-tooltip>
          </q-btn>
          <q-btn :color="viewMode === 'grid' ? 'primary' : 'grey-7'" :flat="viewMode !== 'grid'" icon="grid_view" aria-label="图标视图" @click="setViewMode('grid')">
            <q-tooltip>图标视图</q-tooltip>
          </q-btn>
          <q-btn v-if="selectedItems.length" color="deep-orange" icon="more_horiz" label="操作" no-caps :disable="operationBusy">
            <q-menu>
              <q-list dense style="min-width: 190px">
                <q-item v-for="action in availableActions" :key="action.id" v-close-popup clickable :disable="action.disabled" @click="runAction(action.id)">
                  <q-item-section avatar>
                    <q-icon :name="action.icon" :color="action.color" />
                  </q-item-section>
                  <q-item-section>{{ action.label }}</q-item-section>
                </q-item>
              </q-list>
            </q-menu>
          </q-btn>
        </q-btn-group>
      </div>
    </q-card-section>
    <q-card-section class="file-manager-content row q-col-gutter-md q-pt-none">
      <aside v-if="$q.screen.gt.sm" class="file-manager-sidebar col-12 col-md-4 col-lg-3">
        <q-card flat bordered class="file-tree-card">
          <q-card-section class="row items-center q-py-sm">
            <div class="text-subtitle2">目录</div>
            <q-space />
            <q-btn dense flat round icon="refresh" size="sm" aria-label="刷新目录树" @click="resetTree" />
          </q-card-section>
          <q-separator />
          <q-card-section class="file-tree-content q-pa-sm">
            <q-tree :nodes="treeNodes" node-key="key" label-key="label" children-key="children" :selected="selectedTreeKey" v-model:expanded="expandedTreeKeys" no-connectors @update:selected="onMainTreeSelect" @lazy-load="onTreeLazyLoad" />
          </q-card-section>
        </q-card>
      </aside>
      <main class="file-manager-main col-12" :class="$q.screen.gt.sm ? 'col-md-8 col-lg-9' : ''">
        <div class="file-main-toolbar row items-center q-mb-sm">
          <nav class="col file-path-navigation" :aria-label="`当前目录：${currentPathLabel}`">
            <q-breadcrumbs class="file-breadcrumbs text-grey-8">
              <q-breadcrumbs-el>
                <q-btn flat dense no-caps icon="home" label="根目录" class="file-breadcrumb-button" @click="navigateTo([])" />
              </q-breadcrumbs-el>
              <q-breadcrumbs-el v-for="(segment, index) in currentPath" :key="pathKey(currentPath.slice(0, index + 1))">
                <q-btn flat dense no-caps :label="segment" class="file-breadcrumb-button" @click="navigateTo(currentPath.slice(0, index + 1))" />
              </q-breadcrumbs-el>
            </q-breadcrumbs>
          </nav>
          <div class="col-auto text-body2 text-grey-7 q-ml-md"> 共 {{ filteredItems.length }} 项 <span v-if="selectedItems.length" class="q-ml-sm text-primary"> 已选择 {{ selectedItems.length }} 项 </span>
          </div>
          <q-btn v-if="selectedItems.length" flat round dense icon="close" class="q-ml-xs" aria-label="清除选择" @click="clearSelection" />
        </div>
        <q-banner v-if="loadError" rounded class="bg-red-1 text-negative q-mb-md">
          <template #avatar>
            <q-icon name="error_outline" />
          </template> {{ loadError }} <template #action>
            <q-btn flat color="negative" label="重试" @click="refresh" />
          </template>
        </q-banner>
        <q-card flat bordered class="file-list-card">
          <q-table v-if="viewMode === 'table'" class="file-table" v-model:pagination="pagination" :rows="filteredItems" :columns="columns" :loading="loading" :sort-method="sortItems" row-key="fullPath" :rows-per-page-options="[0]" hide-pagination flat>
            <template #body="props">
              <q-tr :props="props" :class="{ 'file-row--selected': isSelected(props.row) }" @click="selectItem(props.row, $event)" @dblclick.stop="openItem(props.row)" @contextmenu.prevent="selectForContext(props.row)">
                <q-td key="selected" :props="props" auto-width>
                  <q-checkbox :model-value="isSelected(props.row)" dense @click.stop @update:model-value="toggleSelection(props.row, Boolean($event))" />
                </q-td>
                <q-td key="name" :props="props" class="file-name-cell">
                  <div class="row no-wrap items-center">
                    <q-icon :name="getFileIcon(props.row)" :color="props.row.type === 'dir' ? 'amber-8' : 'blue-grey-6'" size="22px" class="q-mr-sm" />
                    <span class="ellipsis">{{ props.row.name }}</span>
                    <q-badge v-if="props.row.type === 'file' && getExtension(props.row.name)" color="grey-3" text-color="grey-8" class="q-ml-sm"> {{ getExtension(props.row.name) }} </q-badge>
                  </div>
                </q-td>
                <q-td key="size" :props="props" class="text-right"> {{ props.row.type === 'dir' ? '—' : formatSize(props.row.size) }} </q-td>
                <q-td key="date" :props="props"> {{ formatDate(props.row.date) }} </q-td>
                <q-td key="actions" :props="props" auto-width>
                  <q-btn v-if="props.row.type === 'file' && isImage(props.row)" dense flat round color="primary" icon="visibility" aria-label="预览" @click.stop="openPreview(props.row)" />
                </q-td>
                <q-menu context-menu touch-position>
                  <q-list dense style="min-width: 190px">
                    <q-item v-for="action in availableActions" :key="action.id" v-close-popup clickable :disable="action.disabled" @click="runAction(action.id)">
                      <q-item-section avatar>
                        <q-icon :name="action.icon" :color="action.color" />
                      </q-item-section>
                      <q-item-section>{{ action.label }}</q-item-section>
                    </q-item>
                  </q-list>
                </q-menu>
              </q-tr>
            </template>
            <template #no-data>
              <div class="full-width row flex-center text-grey-7 q-gutter-sm q-pa-lg">
                <q-icon name="folder_open" size="24px" />
                <span>{{ searchQuery ? '没有匹配的文件' : '此目录为空' }}</span>
              </div>
            </template>
          </q-table>
          <div v-else class="file-grid q-pa-md">
            <div v-if="!filteredItems.length && !loading" class="text-center text-grey-7 q-pa-xl">
              <q-icon name="folder_open" size="40px" />
              <div class="q-mt-sm">{{ searchQuery ? '没有匹配的文件' : '此目录为空' }}</div>
            </div>
            <div class="row q-col-gutter-md">
              <div v-for="item in filteredItems" :key="item.fullPath" class="col-6 col-sm-4 col-md-3 col-lg-2">
                <q-card flat bordered class="file-grid-item" :class="{ 'file-grid-item--selected': isSelected(item) }" @click="selectItem(item, $event)" @dblclick.stop="openItem(item)" @contextmenu.prevent="selectForContext(item)">
                  <q-card-section class="column items-center text-center q-pa-md">
                    <q-icon :name="getFileIcon(item)" :color="item.type === 'dir' ? 'amber-8' : 'blue-grey-6'" size="46px" />
                    <div class="file-grid-name ellipsis-2-lines q-mt-sm" :title="item.name"> {{ item.name }} </div>
                    <div class="text-caption text-grey-7 q-mt-xs"> {{ item.type === 'dir' ? '文件夹' : formatSize(item.size) }} </div>
                    <q-checkbox class="absolute-top-right q-ma-xs" :model-value="isSelected(item)" dense @click.stop @update:model-value="toggleSelection(item, Boolean($event))" />
                    <q-menu context-menu touch-position>
                      <q-list dense style="min-width: 190px">
                        <q-item v-for="action in availableActions" :key="action.id" v-close-popup clickable :disable="action.disabled" @click="runAction(action.id)">
                          <q-item-section avatar>
                            <q-icon :name="action.icon" :color="action.color" />
                          </q-item-section>
                          <q-item-section>{{ action.label }}</q-item-section>
                        </q-item>
                      </q-list>
                    </q-menu>
                  </q-card-section>
                </q-card>
              </div>
            </div>
          </div>
          <q-inner-loading v-if="viewMode === 'grid'" :show="loading">
            <q-spinner color="primary" size="38px" />
            <div class="text-caption q-mt-sm">加载中…</div>
          </q-inner-loading>
        </q-card>
      </main>
    </q-card-section>
    <div v-if="dragActive" class="drop-overlay">
      <q-icon name="cloud_upload" size="48px" />
      <div class="text-h6 q-mt-sm">松开以上传到 {{ currentPathLabel }}</div>
    </div>
  </q-card>
  <q-dialog v-model="dialogOpen" persistent>
    <q-card class="file-dialog-card">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">
          <q-icon :name="dialogIcon" color="primary" class="q-mr-sm" /> {{ dialogTitle }}
        </div>
        <q-space />
        <q-btn icon="close" flat round dense :disable="dialogBusy" aria-label="关闭" @click="closeDialog" />
      </q-card-section>
      <q-card-section v-if="dialogMode === 'preview'" class="q-pt-md">
        <div class="text-subtitle2 q-mb-sm">{{ previewItem?.name }}</div>
        <q-img :src="previewUrl" fit="contain" class="file-preview-image">
          <template #loading>
            <div class="absolute-full flex flex-center">
              <q-spinner color="primary" size="42px" />
            </div>
          </template>
          <template #error>
            <div class="absolute-full flex flex-center bg-grey-2 text-grey-7">
              <div class="text-center">
                <q-icon name="broken_image" size="42px" />
                <div>图片加载失败</div>
              </div>
            </div>
          </template>
        </q-img>
      </q-card-section>
      <q-form v-else @submit.prevent="submitDialog">
        <q-card-section class="q-pt-md">
          <q-banner v-if="dialogError" rounded class="bg-red-1 text-negative q-mb-md">
            <template #avatar><q-icon name="error_outline" /></template> {{ dialogError }} </q-banner>
          <template v-if="dialogMode === 'new-folder'">
            <q-input v-model="dialogName" outlined autofocus label="文件夹名称" :rules="[validateEntryName]" />
          </template>
          <template v-else-if="dialogMode === 'upload'">
            <div class="text-body2 q-mb-md"> 上传目标：<strong>{{ currentPathLabel }}</strong>
            </div>
            <q-file v-model="uploadSelection" outlined multiple use-chips clearable label="选择一个或多个文件">
              <template #prepend><q-icon name="attach_file" /></template>
            </q-file>
            <q-linear-progress v-if="dialogBusy" :value="uploadProgress / 100" color="primary" class="q-mt-md" />
            <div v-if="dialogBusy" class="text-caption text-grey-7 q-mt-xs"> 正在上传：{{ uploadProgress }}% </div>
          </template>
          <template v-else-if="dialogMode === 'rename'">
            <div class="text-body2 q-mb-md"> 重命名：<strong>{{ operationItems[0]?.name }}</strong>
            </div>
            <q-input v-model="dialogName" outlined autofocus label="新名称" :rules="[validateEntryName]" />
          </template>
          <template v-else-if="dialogMode === 'copy' || dialogMode === 'move'">
            <div class="text-body2 q-mb-sm"> {{ dialogMode === 'copy' ? '复制' : '移动' }} {{ operationItems.length }} 个项目到： <strong>{{ destinationLabel }}</strong>
            </div>
            <q-input v-if="dialogMode === 'copy' && operationItems.length === 1" v-model="dialogName" outlined autofocus label="副本名称" class="q-mb-md" :rules="[validateEntryName]" />
            <q-card flat bordered>
              <q-card-section class="row items-center q-py-sm">
                <div class="text-subtitle2">选择目标目录</div>
                <q-space />
                <q-btn dense flat icon="home" label="根目录" no-caps @click="setDestination([])" />
              </q-card-section>
              <q-separator />
              <q-card-section class="destination-tree">
                <q-tree :nodes="treeNodes" node-key="key" label-key="label" children-key="children" :selected="destinationTreeKey" v-model:expanded="destinationExpandedKeys" no-connectors @update:selected="onDestinationTreeSelect" @lazy-load="onTreeLazyLoad" />
              </q-card-section>
            </q-card>
            <div class="text-caption text-grey-7 q-mt-sm"> {{ dialogMode === 'move' ? '不能移动到原目录或所选目录的子目录。' : '目标目录中已有同名文件时会显示错误。' }} </div>
          </template>
          <template v-else-if="dialogMode === 'compress'">
            <div class="text-body2 q-mb-md"> 将 {{ operationItems.length }} 个项目压缩到当前目录。 </div>
            <q-input v-model="dialogName" outlined autofocus label="压缩包名称" hint="扩展名会自动使用 .zip" :rules="[validateEntryName]" />
          </template>
          <template v-else-if="dialogMode === 'extract'">
            <div class="text-body2 q-mb-md"> 解压 <strong>{{ operationItems[0]?.name }}</strong> 到当前目录。 </div>
            <q-input v-model="dialogName" outlined autofocus label="目标文件夹名称" :rules="[validateEntryName]" />
          </template>
          <template v-else-if="dialogMode === 'remove'">
            <div class="row no-wrap items-center">
              <q-icon name="warning" color="negative" size="32px" class="q-mr-md" />
              <div>
                <div>确定删除所选的 {{ operationItems.length }} 个项目吗？</div>
                <div class="text-caption text-grey-7 q-mt-xs">此操作不可撤销。</div>
              </div>
            </div>
            <q-list bordered separator class="q-mt-md operation-file-list">
              <q-item v-for="item in operationItems" :key="item.fullPath" dense>
                <q-item-section avatar>
                  <q-icon :name="getFileIcon(item)" />
                </q-item-section>
                <q-item-section>{{ item.name }}</q-item-section>
              </q-item>
            </q-list>
          </template>
          <template v-else-if="dialogMode === 'permissions'">
            <div class="text-body2 q-mb-md"> 修改 {{ operationItems.length }} 个项目的 Unix 权限。 </div>
            <div class="permission-grid">
              <div class="text-subtitle2">主体</div>
              <div class="text-subtitle2 text-center">读取</div>
              <div class="text-subtitle2 text-center">写入</div>
              <div class="text-subtitle2 text-center">执行</div>
              <template v-for="group in permissionGroups" :key="group.key">
                <div class="text-body2">{{ group.label }}</div>
                <div v-for="bit in permissionBits" :key="bit.key" class="text-center">
                  <q-checkbox v-model="permissions[group.key][bit.key]" dense />
                </div>
              </template>
            </div>
            <q-toggle v-if="operationItems.some(item => item.type === 'dir')" v-model="recursivePermissions" label="递归应用到子目录和文件" color="primary" class="q-mt-md" />
            <q-banner rounded class="bg-grey-2 q-mt-md"> 权限：<strong>{{ permissionsCode }}</strong>
              <span class="q-ml-sm">（{{ permissionsOctal }}）</span>
              <div class="text-caption text-grey-7 q-mt-xs"> 此操作要求运行环境支持 POSIX 文件权限；Windows 上服务端会明确返回不支持。 </div>
            </q-banner>
          </template>
          <template v-else-if="dialogMode === 'edit'">
            <div class="text-caption text-grey-7 q-mb-sm"> {{ operationItems[0]?.fullPath }} </div>
            <q-input v-model="dialogContent" type="textarea" outlined autogrow input-style="min-height: 48vh; font-family: monospace; white-space: pre;" :loading="dialogBusy" label="文件内容" />
          </template>
        </q-card-section>
        <q-card-actions align="right" class="q-px-md q-pb-md">
          <q-btn flat color="grey-7" label="取消" no-caps :disable="dialogBusy" @click="closeDialog" />
          <q-btn v-if="dialogMode === 'preview'" color="primary" label="关闭" no-caps @click="closeDialog" />
          <q-btn v-else :color="dialogMode === 'remove' ? 'negative' : 'primary'" :label="submitLabel" no-caps type="submit" :loading="dialogBusy" :disable="dialogMode === 'upload' && !hasUploadSelection" />
        </q-card-actions>
      </q-form>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { computed, nextTick, onMounted, reactive, ref } from "vue"
import { useQuasar } from "quasar"
import { FileManagerApi, type FileManagerItem } from "@/api/FileManagerApi"

interface FileEntry extends FileManagerItem {
  fullPath: string
}

interface TreeNode {
  key: string
  label: string
  path: string[]
  icon: string
  children?: TreeNode[]
  lazy?: boolean
}

interface TreeLazyRequest {
  node: TreeNode
  done: (children: TreeNode[]) => void
  fail: () => void
}

type DialogMode = "new-folder" | "upload" | "rename" | "copy" | "move" | "compress" | "extract" | "remove" | "permissions" | "edit" | "preview" | null
type ActionId = Exclude<DialogMode, null> | "download" | "download-zip"
type PermissionGroup = "owner" | "group" | "others"
type PermissionBit = "read" | "write" | "exec"

interface ActionOption {
  id: ActionId
  label: string
  icon: string
  color?: string
  disabled?: boolean
}

const $q = useQuasar()
const currentPath = ref<string[]>([])
const currentItems = ref<FileEntry[]>([])
const selectedPaths = ref<string[]>([])
const searchQuery = ref("")
const loadError = ref("")
const loading = ref(false)
const savingRoot = ref(false)
const rootPathInput = ref("")
const dragActive = ref(false)
const requestVersion = ref(0)
const lastSelectedPath = ref<string | null>(null)
const viewMode = ref<"table" | "grid">("table")
const pagination = ref({ sortBy: "name", descending: false, page: 1, rowsPerPage: 0 })
const uploadProgress = ref(0)
const uploadSelection = ref<File[] | File | null>([])
const hasUploadSelection = computed(() => Array.isArray(uploadSelection.value) ? uploadSelection.value.length > 0 : Boolean(uploadSelection.value))
const dialogMode = ref<DialogMode>(null)
const dialogName = ref("")
const dialogContent = ref("")
const dialogError = ref("")
const dialogBusy = ref(false)
const operationBusy = ref(false)
const operationItems = ref<FileEntry[]>([])
const destinationPath = ref<string[]>([])
const destinationTreeKey = ref("/")
const destinationExpandedKeys = ref<string[]>(["/"])
const previewItem = ref<FileEntry | null>(null)
const recursivePermissions = ref(false)
const permissions = reactive({
  owner: { read: true, write: true, exec: false },
  group: { read: true, write: false, exec: false },
  others: { read: true, write: false, exec: false }
})

const treeNodes = ref<TreeNode[]>([createRootTreeNode()])
const selectedTreeKey = ref("/")
const expandedTreeKeys = ref<string[]>(["/"])
const editablePattern = /\.(txt|diff?|patch|svg|asc|cnf|cfg|conf|html?|.\w{0,2}html|cfm|cgi|aspx?|ini|pl|py|md|css|cs|js|jsx|ts|tsx|jsp|log|htaccess|htpasswd|gitignore|gitattributes|env|json|atom|eml|rss|markdown|sql|\w{0,5}xml|xslt?|sh|rb|as|bat|cmd|cob|for|ftn|frm|frx|inc|lisp|scm|coffee|php[3-6]?|java|c|cbl|go|h|scala|vb|tmpl|lock|go|yml|yaml|tsv|lst|config|ashx|asax|\w{0,2}proj|map)$/i
const imagePattern = /\.(jpe?g|gif|bmp|png|svg|webp|ico|tiff?)$/i
const archivePattern = /\.(gz|tar|rar|g?zip|7z)$/i

const columns = [
  { name: "selected", label: "", field: "fullPath", align: "center" as const, sortable: false },
  { name: "name", label: "名称", field: "name", align: "left" as const, sortable: true },
  { name: "size", label: "大小", field: "size", align: "right" as const, sortable: true },
  { name: "date", label: "修改时间", field: "date", align: "left" as const, sortable: true },
  { name: "actions", label: "", field: "fullPath", align: "right" as const, sortable: false }
]

const permissionGroups: { key: PermissionGroup; label: string }[] = [
  { key: "owner", label: "所有者" },
  { key: "group", label: "群组" },
  { key: "others", label: "其他用户" }
]

const permissionBits: { key: PermissionBit; label: string; value: number; code: string }[] = [
  { key: "read", label: "读取", value: 4, code: "r" },
  { key: "write", label: "写入", value: 2, code: "w" },
  { key: "exec", label: "执行", value: 1, code: "x" }
]

const filteredItems = computed(() => {
  const term = searchQuery.value.trim().toLocaleLowerCase()
  return term
    ? currentItems.value.filter(item => item.name.toLocaleLowerCase().includes(term))
    : currentItems.value
})

const selectedItems = computed(() => {
  const selected = new Set(selectedPaths.value)
  return currentItems.value.filter(item => selected.has(item.fullPath))
})

const currentPathLabel = computed(() => `/${currentPath.value.join("/")}`)
const destinationLabel = computed(() => `/${destinationPath.value.join("/")}`)
const previewUrl = computed(() => previewItem.value ? FileManagerApi.downloadUrl(previewItem.value.fullPath) : "")
const dialogOpen = computed({
  get: () => dialogMode.value !== null,
  set: (open: boolean) => {
    if (!open && !dialogBusy.value) {
      dialogMode.value = null
    }
  }
})

const permissionsOctal = computed(() => permissionGroups.map(group => {
  return permissionBits.reduce((total, bit) => total + (permissions[group.key][bit.key] ? bit.value : 0), 0).toString()
}).join(""))

const permissionsCode = computed(() => permissionGroups.map(group => {
  return permissionBits.map(bit => permissions[group.key][bit.key] ? bit.code : "-").join("")
}).join(""))

const availableActions = computed(() => getActions(selectedItems.value))
const dialogTitle = computed(() => {
  const titles: Record<Exclude<DialogMode, null>, string> = {
    "new-folder": "新建文件夹",
    upload: "上传文件",
    rename: "重命名",
    copy: "复制到",
    move: "移动到",
    compress: "创建压缩包",
    extract: "解压文件",
    remove: "确认删除",
    permissions: "修改权限",
    edit: "编辑文件",
    preview: "图片预览"
  }
  return dialogMode.value ? titles[dialogMode.value] : ""
})

const dialogIcon = computed(() => {
  const icons: Record<Exclude<DialogMode, null>, string> = {
    "new-folder": "create_new_folder",
    upload: "upload",
    rename: "drive_file_rename_outline",
    copy: "content_copy",
    move: "drive_file_move",
    compress: "archive",
    extract: "unarchive",
    remove: "delete",
    permissions: "admin_panel_settings",
    edit: "edit",
    preview: "image"
  }
  return dialogMode.value ? icons[dialogMode.value] : "folder"
})

const submitLabel = computed(() => {
  const labels: Record<Exclude<DialogMode, null>, string> = {
    "new-folder": "创建",
    upload: "开始上传",
    rename: "重命名",
    copy: "复制",
    move: "移动",
    compress: "压缩",
    extract: "解压",
    remove: "删除",
    permissions: "应用权限",
    edit: "保存",
    preview: "关闭"
  }
  return dialogMode.value ? labels[dialogMode.value] : "确定"
})

function createRootTreeNode(): TreeNode {
  return {
    key: "/",
    label: "根目录",
    path: [],
    icon: "home",
    children: []
  }
}

function mergeDirectoryChildren(path: string[], items: FileManagerItem[]): void {
  if (!treeNodes.value.length) {
    treeNodes.value = [createRootTreeNode()]
  }
  let node = treeNodes.value[0]
  const traversed: string[] = []

  for (const segment of path) {
    traversed.push(segment)
    node.children ||= []
    let child = node.children.find(item => item.label === segment)
    if (!child) {
      child = {
        key: pathKey(traversed),
        label: segment,
        path: [...traversed],
        icon: "folder",
        children: []
      }
      node.children.push(child)
    }
    node.lazy = false
    node = child
  }

  node.children = items
    .filter(item => item.type === "dir")
    .map(item => {
      const childPath = [...path, item.name]
      return {
        key: pathKey(childPath),
        label: item.name,
        path: childPath,
        icon: "folder",
        lazy: true
      }
    })
  node.lazy = false
  treeNodes.value = [...treeNodes.value]
}

function pathKey(path: string[]): string {
  return path.length ? `/${path.join("/")}` : "/"
}

function pathFromKey(key: string): string[] {
  return key === "/" ? [] : key.replace(/^\/+/, "").split("/").filter(Boolean)
}

function entryPath(path: string[], name: string): string {
  return pathKey([...path, name]).replace(/\/+/g, "/")
}

function toEntry(item: FileManagerItem, path: string[]): FileEntry {
  return { ...item, fullPath: entryPath(path, item.name) }
}

function errorText(error: unknown): string {
  if (error instanceof Error && error.message) {
    return error.message
  }
  return "操作失败，请检查网络或服务器响应。"
}

function notifyError(error: unknown): void {
  $q.notify({ type: "negative", message: errorText(error), position: "top" })
}

async function loadDirectory(path: string[]): Promise<void> {
  const version = ++requestVersion.value
  currentPath.value = [...path]
  selectedTreeKey.value = pathKey(path)
  loading.value = true
  loadError.value = ""
  try {
    const items = await FileManagerApi.list(path)
    if (version === requestVersion.value) {
      currentItems.value = items.map(item => toEntry(item, path))
      mergeDirectoryChildren(path, items)
    }
  } catch (error) {
    if (version === requestVersion.value) {
      currentItems.value = []
      loadError.value = errorText(error)
    }
  } finally {
    if (version === requestVersion.value) {
      loading.value = false
    }
  }
}

async function navigateTo(path: string[]): Promise<void> {
  searchQuery.value = ""
  clearSelection()
  await loadDirectory(path)
  expandedTreeKeys.value = [...new Set(["/", ...path.map((_, index) => pathKey(path.slice(0, index + 1)))])]
}

async function refresh(): Promise<void> {
  searchQuery.value = ""
  clearSelection()
  await loadDirectory(currentPath.value)
}

function clearSelection(): void {
  selectedPaths.value = []
  lastSelectedPath.value = null
}

function isSelected(item: FileEntry): boolean {
  return selectedPaths.value.includes(item.fullPath)
}

function isInteractiveTarget(target: EventTarget | null): boolean {
  return target instanceof HTMLElement && Boolean(target.closest("button, input, textarea, .q-checkbox, .q-btn, .q-menu"))
}

function selectItem(item: FileEntry, event?: MouseEvent): void {
  if (event && isInteractiveTarget(event.target)) {
    return
  }
  const visible = filteredItems.value
  const index = visible.findIndex(entry => entry.fullPath === item.fullPath)
  const anchorIndex = lastSelectedPath.value ? visible.findIndex(entry => entry.fullPath === lastSelectedPath.value) : -1

  if (event?.shiftKey && anchorIndex >= 0 && index >= 0) {
    const start = Math.min(anchorIndex, index)
    const end = Math.max(anchorIndex, index)
    selectedPaths.value = visible.slice(start, end + 1).map(entry => entry.fullPath)
    return
  }
  if (event && (event.ctrlKey || event.metaKey)) {
    selectedPaths.value = isSelected(item)
      ? selectedPaths.value.filter(path => path !== item.fullPath)
      : [...selectedPaths.value, item.fullPath]
    lastSelectedPath.value = item.fullPath
    return
  }

  selectedPaths.value = [item.fullPath]
  lastSelectedPath.value = item.fullPath
}

function toggleSelection(item: FileEntry, checked: boolean): void {
  if (checked && !isSelected(item)) {
    selectedPaths.value = [...selectedPaths.value, item.fullPath]
    lastSelectedPath.value = item.fullPath
  } else if (!checked) {
    selectedPaths.value = selectedPaths.value.filter(path => path !== item.fullPath)
  }
}

function selectForContext(item: FileEntry): void {
  if (!isSelected(item)) {
    selectedPaths.value = [item.fullPath]
    lastSelectedPath.value = item.fullPath
  }
}

function getActions(items: FileEntry[]): ActionOption[] {
  if (!items.length) {
    return [
      { id: "new-folder", label: "新建文件夹", icon: "create_new_folder" },
      { id: "upload", label: "上传文件", icon: "upload" }
    ]
  }

  const allFiles = items.every(item => item.type === "file")
  const single = items.length === 1 ? items[0] : null
  const actions: ActionOption[] = []

  if (single?.type === "file") {
    actions.push({ id: "download", label: "下载", icon: "download" })
  } else if (items.length > 1 && allFiles) {
    actions.push({ id: "download-zip", label: "打包下载为 ZIP", icon: "folder_zip" })
  }
  if (single?.type === "file" && isImage(single)) {
    actions.push({ id: "preview", label: "图片预览", icon: "image" })
  }
  if (single) {
    actions.push({ id: "rename", label: "重命名", icon: "drive_file_rename_outline" })
  }
  if (allFiles) {
    actions.push({ id: "copy", label: "复制", icon: "content_copy" })
  }
  actions.push({ id: "move", label: "移动", icon: "drive_file_move" })
  if (single?.type === "file" && isEditable(single)) {
    actions.push({ id: "edit", label: "编辑文本", icon: "edit" })
  }
  actions.push({ id: "permissions", label: "修改权限", icon: "admin_panel_settings" })
  actions.push({ id: "compress", label: "压缩", icon: "archive" })
  if (single?.type === "file" && isExtractable(single)) {
    actions.push({ id: "extract", label: "解压", icon: "unarchive" })
  }
  actions.push({ id: "remove", label: "删除", icon: "delete", color: "negative" })
  return actions
}

function runAction(action: ActionId): void {
  if (operationBusy.value) {
    return
  }
  if (action === "download") {
    const item = selectedItems.value[0]
    if (item) {
      window.open(FileManagerApi.downloadUrl(item.fullPath), "_blank", "noopener")
    }
    return
  }
  if (action === "download-zip") {
    const items = selectedItems.value
    if (items.length) {
      const suffix = Date.now().toString().slice(-5)
      window.open(FileManagerApi.downloadMultipleUrl(items.map(item => item.fullPath), `${suffix}-files.zip`), "_blank", "noopener")
    }
    return
  }
  if (action === "preview") {
    const item = selectedItems.value[0]
    if (item) {
      openPreview(item)
    }
    return
  }
  openDialog(action)
}

function openItem(item: FileEntry): void {
  if (item.type === "dir") {
    navigateTo([...currentPath.value, item.name])
  } else if (isImage(item)) {
    openPreview(item)
  } else if (isEditable(item)) {
    openDialog("edit", [item])
  } else {
    window.open(FileManagerApi.downloadUrl(item.fullPath), "_blank", "noopener")
  }
}

function openPreview(item: FileEntry): void {
  previewItem.value = item
  dialogError.value = ""
  operationItems.value = [item]
  dialogMode.value = "preview"
}

async function openDialog(mode: Exclude<DialogMode, null>, explicitItems?: FileEntry[]): Promise<void> {
  operationItems.value = explicitItems ? [...explicitItems] : [...selectedItems.value]
  dialogName.value = ""
  dialogContent.value = ""
  dialogError.value = ""
  dialogBusy.value = false
  uploadProgress.value = 0
  recursivePermissions.value = false
  uploadSelection.value = []

  if (mode === "rename" && operationItems.value[0]) {
    dialogName.value = operationItems.value[0].name
  } else if (mode === "copy" && operationItems.value.length === 1) {
    dialogName.value = operationItems.value[0].name
  } else if (mode === "compress") {
    const name = operationItems.value.length === 1
      ? operationItems.value[0].name.replace(/\.[^.]+$/, "")
      : "files"
    dialogName.value = `${name}.zip`
  } else if (mode === "extract" && operationItems.value[0]) {
    dialogName.value = operationItems.value[0].name.replace(/\.[^.]+$/, "")
  }

  if (mode === "copy" || mode === "move") {
    destinationPath.value = [...currentPath.value]
    destinationTreeKey.value = pathKey(destinationPath.value)
    destinationExpandedKeys.value = [...new Set(["/", ...destinationPath.value.map((_, index) => pathKey(destinationPath.value.slice(0, index + 1)))])]
  }
  if (mode === "permissions") {
    applyPermissions("644")
  }

  dialogMode.value = mode
  if (mode === "edit" && operationItems.value[0]) {
    dialogBusy.value = true
    try {
      dialogContent.value = await FileManagerApi.getContent(operationItems.value[0].fullPath)
    } catch (error) {
      dialogError.value = errorText(error)
    } finally {
      dialogBusy.value = false
    }
  }
}

function closeDialog(): void {
  if (dialogBusy.value) {
    return
  }
  dialogMode.value = null
  dialogError.value = ""
}

function validateEntryName(value: string): true | string {
  const name = (value || "").trim()
  if (!name) {
    return "名称不能为空"
  }
  if (name === "." || name === ".." || /[\\/]/.test(name)) {
    return "名称不能包含路径分隔符"
  }
  return true
}

function validateNameAgainstDirectory(name: string, directory: string[], excludePath?: string): string | null {
  const invalid = validateEntryName(name)
  if (invalid !== true) {
    return invalid
  }
  if (pathKey(directory) === pathKey(currentPath.value)) {
    const duplicate = currentItems.value.find(item => item.name.toLocaleLowerCase() === name.trim().toLocaleLowerCase() && item.fullPath !== excludePath)
    if (duplicate) {
      return "此目录中已存在同名项目"
    }
  }
  return null
}

function isDestinationInsideSelectedFolder(destination: string[]): boolean {
  const target = pathKey(destination).replace(/\/+$/, "")
  return operationItems.value.some(item => {
    if (item.type !== "dir") {
      return false
    }
    const source = item.fullPath.replace(/\/+$/, "")
    return target === source || target.startsWith(`${source}/`)
  })
}

async function submitDialog(): Promise<void> {
  const mode = dialogMode.value
  if (!mode || mode === "preview" || dialogBusy.value) {
    return
  }

  dialogError.value = ""
  dialogBusy.value = true
  operationBusy.value = true
  try {
    const paths = operationItems.value.map(item => item.fullPath)
    switch (mode) {
      case "new-folder": {
        const error = validateNameAgainstDirectory(dialogName.value, currentPath.value)
        if (error) throw new Error(error)
        await FileManagerApi.createFolder([...currentPath.value, dialogName.value.trim()])
        break
      }
      case "upload": {
        const files = uploadSelection.value
          ? Array.isArray(uploadSelection.value) ? uploadSelection.value : [uploadSelection.value]
          : []
        if (!files.length) throw new Error("请先选择要上传的文件")
        await FileManagerApi.upload(files, currentPath.value, progress => {
          uploadProgress.value = progress
        })
        break
      }
      case "rename": {
        const item = operationItems.value[0]
        if (!item) throw new Error("请选择一个文件或文件夹")
        const error = validateNameAgainstDirectory(dialogName.value, currentPath.value, item.fullPath)
        if (error) throw new Error(error)
        await FileManagerApi.rename(item.fullPath, entryPath(currentPath.value, dialogName.value.trim()))
        break
      }
      case "copy": {
        if (!paths.length) throw new Error("请选择要复制的文件")
        if (operationItems.value.some(item => item.type === "dir")) throw new Error("当前接口不支持复制文件夹")
        const names = operationItems.value.length === 1
          ? [dialogName.value.trim()]
          : operationItems.value.map(item => item.name)
        await assertNoDestinationCollision(destinationPath.value, names)
        await FileManagerApi.copy(paths, destinationPath.value, operationItems.value.length === 1 ? dialogName.value.trim() : undefined)
        break
      }
      case "move": {
        if (!paths.length) throw new Error("请选择要移动的项目")
        if (pathKey(destinationPath.value) === pathKey(currentPath.value)) {
          throw new Error("目标目录与当前目录相同")
        }
        if (isDestinationInsideSelectedFolder(destinationPath.value)) {
          throw new Error("不能将文件夹移动到自身或其子目录中")
        }
        await assertNoDestinationCollision(destinationPath.value, operationItems.value.map(item => item.name))
        await FileManagerApi.move(paths, destinationPath.value)
        break
      }
      case "compress": {
        if (!paths.length) throw new Error("请选择要压缩的项目")
        const compressedName = `${dialogName.value.trim().replace(/\.zip$/i, "")}.zip`
        const error = validateNameAgainstDirectory(compressedName, currentPath.value)
        if (error) throw new Error(error)
        await FileManagerApi.compress(paths, currentPath.value, compressedName)
        break
      }
      case "extract": {
        const item = operationItems.value[0]
        if (!item) throw new Error("请选择要解压的文件")
        const error = validateNameAgainstDirectory(dialogName.value, currentPath.value)
        if (error) throw new Error(error)
        await FileManagerApi.extract(item.fullPath, currentPath.value, dialogName.value.trim())
        break
      }
      case "remove": {
        if (!paths.length) throw new Error("请选择要删除的项目")
        await FileManagerApi.remove(paths)
        break
      }
      case "permissions": {
        if (!paths.length) throw new Error("请选择要修改权限的项目")
        await FileManagerApi.changePermissions(paths, permissionsOctal.value, permissionsCode.value, recursivePermissions.value)
        break
      }
      case "edit": {
        const item = operationItems.value[0]
        if (!item) throw new Error("请选择要编辑的文件")
        await FileManagerApi.edit(item.fullPath, dialogContent.value)
        break
      }
      default:
        return
    }

    const successMessage: Record<Exclude<DialogMode, null>, string> = {
      "new-folder": "文件夹已创建",
      upload: "文件上传完成",
      rename: "项目已重命名",
      copy: "项目已复制",
      move: "项目已移动",
      compress: "压缩任务已完成",
      extract: "解压任务已完成",
      remove: "项目已删除",
      permissions: "权限已更新",
      edit: "文件已保存",
      preview: "图片已打开"
    }
    dialogBusy.value = false
    dialogMode.value = null
    $q.notify({ type: "positive", message: successMessage[mode], position: "top" })
    if (mode !== "edit") {
      await refresh()
    }
  } catch (error) {
    dialogError.value = errorText(error)
  } finally {
    dialogBusy.value = false
    operationBusy.value = false
  }
}

async function assertNoDestinationCollision(destination: string[], names: string[]): Promise<void> {
  const destinationItems = pathKey(destination) === pathKey(currentPath.value)
    ? currentItems.value
    : (await FileManagerApi.list(destination)).map(item => toEntry(item, destination))
  const conflicts = names.filter(name => destinationItems.some(item => item.name.toLocaleLowerCase() === name.toLocaleLowerCase()))
  if (conflicts.length) {
    throw new Error(`目标目录已存在：${conflicts.join("、")}`)
  }
}

function applyPermissions(octal: string): void {
  const digits = octal.padStart(3, "0").slice(-3).split("").map(value => Number(value))
  permissionGroups.forEach((group, index) => {
    const value = digits[index] || 0
    permissions[group.key].read = (value & 4) !== 0
    permissions[group.key].write = (value & 2) !== 0
    permissions[group.key].exec = (value & 1) !== 0
  })
}

function onMainTreeSelect(key: string | null): void {
  if (key !== null && key !== selectedTreeKey.value) {
    navigateTo(pathFromKey(key))
  }
}

function onDestinationTreeSelect(key: string | null): void {
  if (key !== null) {
    setDestination(pathFromKey(key))
  }
}

function setDestination(path: string[]): void {
  destinationPath.value = [...path]
  destinationTreeKey.value = pathKey(path)
  destinationExpandedKeys.value = [...new Set(["/", ...path.map((_, index) => pathKey(path.slice(0, index + 1)))])]
}

async function onTreeLazyLoad({ node, done, fail }: TreeLazyRequest): Promise<void> {
  try {
    const children = await FileManagerApi.list(node.path)
    done(children
      .filter(item => item.type === "dir")
      .map(item => {
        const path = [...node.path, item.name]
        return {
          key: pathKey(path),
          label: item.name,
          path,
          icon: "folder",
          lazy: true
        }
      }))
  } catch (error) {
    fail()
    notifyError(error)
  }
}

function resetTree(): void {
  treeNodes.value = [createRootTreeNode()]
  expandedTreeKeys.value = ["/"]
  destinationExpandedKeys.value = ["/"]
}

async function saveRootPath(): Promise<void> {
  if (savingRoot.value) {
    return
  }
  savingRoot.value = true
  try {
    await FileManagerApi.saveRootPath(rootPathInput.value.trim())
    resetTree()
    await navigateTo([])
    $q.notify({ type: "positive", message: "文件根目录已更新", position: "top" })
  } catch (error) {
    notifyError(error)
  } finally {
    savingRoot.value = false
  }
}

function setViewMode(mode: "table" | "grid"): void {
  viewMode.value = mode
  window.localStorage.setItem("viewTemplate", mode === "grid" ? "main-icons.html" : "main-table.html")
}

function isEditable(item: FileEntry): boolean {
  return editablePattern.test(item.name)
}

function isImage(item: FileEntry): boolean {
  return imagePattern.test(item.name)
}

function isExtractable(item: FileEntry): boolean {
  return archivePattern.test(item.name)
}

function getExtension(name: string): string {
  const index = name.lastIndexOf(".")
  return index > 0 ? name.slice(index + 1, index + 5).toUpperCase() : ""
}

function getFileIcon(item: FileEntry): string {
  if (item.type === "dir") return "folder"
  if (isImage(item)) return "image"
  if (isExtractable(item)) return "folder_zip"
  if (isEditable(item)) return "description"
  const extension = getExtension(item.name).toLowerCase()
  if (["pdf"].includes(extension)) return "picture_as_pdf"
  if (["doc", "docx", "odt", "rtf"].includes(extension)) return "article"
  if (["xls", "xlsx", "csv"].includes(extension)) return "table_chart"
  if (["mp3", "wav", "flac", "aac"].includes(extension)) return "audio_file"
  if (["mp4", "mov", "mkv", "avi"].includes(extension)) return "movie"
  return "insert_drive_file"
}

function formatSize(bytes: number): string {
  if (bytes <= 0) return "0 B"
  const units = ["B", "KB", "MB", "GB", "TB"]
  const index = Math.min(Math.floor(Math.log(bytes) / Math.log(1024)), units.length - 1)
  return `${(bytes / (1024 ** index)).toFixed(index === 0 ? 0 : 1)} ${units[index]}`
}

function formatDate(value: string): string {
  if (!value) return "—"
  const date = new Date(value.replace(" ", "T"))
  return Number.isNaN(date.getTime()) ? value : date.toLocaleString("zh-CN", { hour12: false })
}

function sortItems(rows: FileEntry[], sortBy: string, descending: boolean): FileEntry[] {
  const direction = descending ? -1 : 1
  return [...rows].sort((left, right) => {
    const folderOrder = (left.type === "dir" ? 0 : 1) - (right.type === "dir" ? 0 : 1)
    if (folderOrder) return folderOrder

    let comparison = 0
    if (sortBy === "size") {
      comparison = left.size - right.size
    } else if (sortBy === "date") {
      comparison = new Date(left.date.replace(" ", "T")).getTime() - new Date(right.date.replace(" ", "T")).getTime()
    } else {
      comparison = left.name.localeCompare(right.name, "zh-CN", { numeric: true, sensitivity: "base" })
    }
    return comparison * direction
  })
}

function handleDrop(event: DragEvent): void {
  dragActive.value = false
  const files = Array.from(event.dataTransfer?.files || [])
  if (!files.length) return
  openDialog("upload")
  uploadSelection.value = files
}

onMounted(() => {
  viewMode.value = window.localStorage.getItem("viewTemplate") === "main-icons.html" ? "grid" : "table"
  loadDirectory([])
  nextTick(() => {
    expandedTreeKeys.value = ["/"]
  })
})
</script>
<style scoped>
.file-manager-page {
  position: relative;
  display: flex;
  height: calc(100vh - 110px);
  min-height: 0;
  overflow: hidden;
}

.file-manager-card {
  position: relative;
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  overflow: hidden;
}

.file-manager-toolbar {
  flex: 0 0 auto;
  flex-wrap: nowrap;
}

.file-manager-root-path {
  display: flex;
  flex: 1 1 360px;
  min-width: 0;
  align-items: center;
  gap: 8px;
}

.file-manager-root-path__input {
  flex: 1 1 auto;
  min-width: 0;
}

.file-manager-search {
  flex: 1 1 220px;
  min-width: 0;
}

.file-manager-toolbar__actions {
  white-space: nowrap;
}

.file-manager-content {
  display: flex;
  flex: 1 1 0;
  min-height: 0;
  overflow: hidden;
}

.file-manager-sidebar,
.file-manager-main {
  height: 100%;
  min-height: 0;
}

.file-manager-sidebar {
  overflow: hidden;
}

.file-tree-card {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  overflow: hidden;
}

.file-tree-content {
  flex: 1 1 auto;
  min-height: 0;
  overflow-y: auto;
}

.file-manager-main {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.file-main-toolbar {
  flex: 0 0 auto;
  min-width: 0;
}

.file-path-navigation {
  min-width: 0;
  overflow-x: auto;
}

.file-breadcrumbs {
  min-width: max-content;
}

.file-breadcrumb-button {
  color: inherit;
}

.file-list-card {
  display: flex;
  flex: 1 1 0;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  overflow: hidden;
}

.file-list-card :deep(.q-table__container) {
  display: flex;
  flex: 1 1 auto;
  flex-direction: column;
  height: 100%;
  min-height: 0;
}

.file-list-card :deep(.q-table__middle) {
  flex: 1 1 auto;
  min-height: 0;
  max-height: none;
  overflow: auto;
}

.file-list-card :deep(.q-table thead tr th) {
  position: sticky;
  top: 0;
  z-index: 1;
  background: white;
}

.file-row--selected {
  background: rgba(25, 118, 210, 0.1);
}

.file-name-cell {
  max-width: 1px;
}

.file-grid {
  flex: 1 1 auto;
  height: 100%;
  min-height: 0;
  overflow-y: auto;
}

.file-grid-item {
  position: relative;
  min-height: 145px;
  cursor: pointer;
  transition: border-color 0.15s ease, box-shadow 0.15s ease;
}

.file-grid-item:hover {
  border-color: var(--q-primary);
}

.file-grid-item--selected {
  border: 1px solid var(--q-primary);
  background: rgba(25, 118, 210, 0.08);
}

.file-grid-name {
  width: 100%;
  min-height: 2.7em;
  overflow-wrap: anywhere;
}

.file-dialog-card {
  width: min(680px, calc(100vw - 32px));
  max-width: 95vw;
}

.destination-tree {
  max-height: 280px;
  overflow-y: auto;
}

.operation-file-list {
  max-height: 220px;
  overflow-y: auto;
}

.permission-grid {
  display: grid;
  grid-template-columns: minmax(100px, 1fr) repeat(3, 64px);
  gap: 8px;
  align-items: center;
}

.file-preview-image {
  width: min(75vw, 900px);
  height: min(68vh, 680px);
  background: #f5f5f5;
}

.drop-overlay {
  position: absolute;
  z-index: 20;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: var(--q-primary);
  background: rgba(255, 255, 255, 0.92);
  border: 2px dashed var(--q-primary);
  pointer-events: none;
}

.file-manager-page--dragging {
  outline: 2px dashed var(--q-primary);
  outline-offset: -6px;
}

@media (max-width: 1199px) {
  .file-manager-toolbar {
    flex-wrap: wrap;
  }

  .file-manager-root-path {
    flex: 1 1 55%;
  }

  .file-manager-search {
    flex: 1 1 30%;
  }
}

@media (max-width: 599px) {
  .file-manager-card {
    height: calc(100vh - 90px);
  }

  .file-manager-page {
    height: calc(100vh - 90px);
  }

  .file-manager-toolbar__actions {
    margin-left: 0;
  }

  .file-manager-root-path,
  .file-manager-search {
    flex: 1 1 100%;
  }

  .file-grid-item {
    min-height: 130px;
  }
}
</style>
