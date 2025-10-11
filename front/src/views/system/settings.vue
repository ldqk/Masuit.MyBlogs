<template>
<div class="system-settings-page">
  <!-- 顶部操作栏 -->
  <div class="row">
    <div class="col text-right">
      <q-btn id="save" color="primary" size="lg" icon="save" label="保存配置" @click="saveSettings" :loading="saving" />
    </div>
  </div>
  <!-- 基础配置 -->
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6 q-mb-md">基础配置</div>
      <div class="row q-gutter-md q-mb-md">
        <div class="col">
          <q-input v-model="settings.Domain" label="网站主域名" outlined dense />
        </div>
        <div class="col">
          <q-input v-model="settings.Title" label="网站标语" outlined dense />
        </div>
      </div>
      <div class="row q-gutter-md">
        <div class="col">
          <q-input v-model="settings.Slogan" label="网站口号" outlined dense />
        </div>
        <div class="col">
          <q-input v-model="settings.ReservedName" label="保留关键字" outlined dense hint="用逗号分隔多个关键字" />
        </div>
      </div>
      <div class="row q-gutter-md">
        <div class="col">
          <q-input v-model="settings.PathRoot" label="资源管理器根目录" outlined dense>
            <template v-slot:append>
              <q-btn flat round dense icon="folder" @click="testPath" />
            </template>
          </q-input>
        </div>
        <div class="col">
          <q-input v-model="settings.UploadPath" label="文件上传目录" outlined dense />
        </div>
        <div class="col">
          <div class="row q-mb-md">
            <div class="col">
              <q-checkbox v-model="enableRss" label="启用RSS" />
            </div>
            <div class="col">
              <q-checkbox v-model="enableDonate" label="启用网站打赏功能" />
            </div>
          </div>
        </div>
        <div class="col">
          <div class="row q-mb-md">
            <div class="col">
              <q-checkbox v-model="closeSite" label="闭站保护" />
            </div>
            <div class="col">
              <q-checkbox v-model="dataReadonly" label="数据写保护" />
            </div>
          </div>
        </div>
      </div>
      <div class="row q-gutter-md q-mb-md">
        <div class="col">
          <div class="row q-gutter-md">
            <div class="col">
              <q-select v-model="settings.WatermarkType" :options="watermarkOptions" label="水印类型" outlined dense emit-value map-options />
            </div>
            <div class="col">
              <div v-if="settings.WatermarkType == 'Text'">
                <q-input autogrow dense outlined v-model="settings.Watermark" placeholder="留空则上传图片时不添加水印" />
              </div>
              <div v-if="settings.WatermarkType == 'Image'">
                <q-img :src="settings.WatermarkImage?.startsWith('http') ? settings.WatermarkImage : globalConfig.baseURL + settings.WatermarkImage" spinner-color="white" @click="showImageUpload('WatermarkImage')" bordered class="logo-preview cursor-pointer" style="max-height: 200px; border: 1px dashed #ccc;" />
                <q-btn color="primary" @click="showImageUpload('WatermarkImage')" label="上传"></q-btn>
              </div>
            </div>
            <div class="col" v-if="settings.WatermarkType != 'None'">
              <q-select v-model="settings.WatermarkPosition" :options="[{ label: '左上角', value: '0' }, { label: '右上角', value: '1' }, { label: '左下角', value: '2' }, { label: '右下角', value: '3' }, { label: '中间', value: '4' }]" outlined dense emit-value map-options label="水印位置："> </q-select>
            </div>
          </div>
        </div>
        <div class="col">
          <div class="row" v-if="enableRss">
            <q-select class="col-auto" v-model="settings.RssStart" :options="[{ label: '1天前', value: '-1' }, { label: '2天前', value: '-2' }, { label: '3天前', value: '-3' }, { label: '4天前', value: '-4' }, { label: '5天前', value: '-5' }]" dense emit-value map-options borderless>
              <template v-slot:prepend>
                <span style="font-size: 14px;"> RSS订阅从 </span>
              </template>
            </q-select>
            <q-select class="col-auto" v-model="settings.RssEnd" :options="[{ label: '现在', value: '0' }, { label: '2小时前', value: '-2' }, { label: '5小时前', value: '-5' }, { label: '12小时前', value: '-12' }, { label: '1天前', value: '-24' }, { label: '2天前', value: '-48' }]" dense emit-value map-options borderless>
              <template v-slot:prepend>
                <span style="font-size: 14px;"> 到 </span>
              </template>
            </q-select>
          </div>
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 自定义脚本和样式 -->
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6">自定义脚本和样式</div>
      <div>
        <div class="text-subtitle1">CSS样式代码：</div>
        <div ref="stylesEditor" class="editor-container" style="height: 200px;"></div>
      </div>
      <div>
        <div class="text-subtitle1 q-mb-sm">JS脚本代码：</div>
        <div ref="scriptsEditor" class="editor-container" style="height: 400px;"></div>
      </div>
    </q-card-section>
  </q-card>
  <!-- SEO 配置 -->
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6">网站SEO相关</div>
      <div class="row q-gutter-md q-mb-md">
        <div class="col">
          <q-input v-model="settings.Title" label="SEO标题" outlined dense />
        </div>
        <div class="col">
          <q-input v-model="settings.Keyword" label="SEO关键词" outlined dense />
        </div>
      </div>
      <div class="row q-gutter-md">
        <div class="col">
          <q-input v-model="settings.Description" label="SEO描述" outlined dense autogrow="" />
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 邮箱配置 -->
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6">网站邮箱配置</div>
      <div class="row q-gutter-md q-mb-md">
        <div class="col">
          <q-input v-model="settings.EmailFrom" label="发件邮箱" outlined dense type="email" />
        </div>
        <div class="col">
          <q-input v-model="settings.EmailPwd" label="发件邮箱密码" outlined dense type="password" />
        </div>
        <div class="col">
          <q-input v-model="settings.SMTP" label="SMTP服务器" outlined dense />
        </div>
        <div class="col">
          <q-input v-model="settings.SmtpPort" label="SMTP端口号" outlined dense type="number" />
        </div>
      </div>
      <div class="row q-gutter-md">
        <div class="col">
          <q-checkbox v-model="enableSsl" label="启用SSL加密" />
        </div>
        <div class="col">
          <q-input v-model="settings.ReceiveEmail" label="接收网站通知邮箱" outlined dense type="email">
            <template v-slot:append>
              <q-btn dense color="info" label="测试发送" @click="testEmail" :loading="testing" />
            </template>
          </q-input>
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 媒体设置 -->
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6">网站媒体设置</div>
      <div class="row q-gutter-md">
        <div class="col-2">
          <div class="text-subtitle1">网站Logo：</div>
          <q-img :src="settings.logo?.startsWith('http') ? settings.logo : globalConfig.baseURL + settings.logo" class="logo-preview cursor-pointer" @click="showImageUpload('logo')" style="max-height: 200px; border: 1px dashed #ccc;" />
          <q-btn flat size="sm" label="选择图片" @click="showImageUpload('logo')" class="full-width q-mt-sm" />
        </div>
        <div class="col-12">
          <div class="text-subtitle1">文章版权声明：</div>
          <vue-ueditor-wrap v-model="settings.Disclaimer" :config="{ initialFrameHeight: 200, initialFrameWidth: '100%' }" />
        </div>
        <div class="col-12">
          <div class="text-subtitle1">网站页脚版权：</div>
          <vue-ueditor-wrap v-model="settings.Copyright" :config="{ initialFrameHeight: 120, initialFrameWidth: '100%' }" />
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 图片上传对话框 -->
  <q-dialog v-model="showUploadDialog" persistent>
    <q-card style="min-width: 400px">
      <q-card-section>
        <div class="text-h6">上传图片</div>
      </q-card-section>
      <q-card-section>
        <q-file v-model="uploadFile" label="选择图片文件" outlined accept="image/*" />
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" color="grey" @click="cancelUpload" />
        <q-btn flat label="上传" color="primary" @click="uploadImage" :loading="uploading" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, nextTick } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'
import globalConfig from '@/config'
import loadCodeMirror from '../../utils/codeMirrorLoader'

// 定义接口类型
interface Settings {
  Domain?: string
  Title?: string
  SubTitle?: string
  Keywords?: string
  WatermarkType?: string
  WatermarkOpacity?: string
  EnableRss?: string
  EnableDonate?: string
  CloseSite?: string
  DataReadonly?: string
  EnableSsl?: string
  RssTitle?: string
  RssDescription?: string
  PathRoot?: string
  UploadPath?: string
  Styles?: string
  Scripts?: string
  SiteTitle?: string
  SiteKeywords?: string
  SiteDescription?: string
  EmailFrom?: string
  EmailPwd?: string
  SMTP?: string
  SmtpPort?: string
  ReceiveEmail?: string
  logo?: string
  Disclaimer?: string
  Copyright?: string
  [key: string]: any
}

interface ApiResponse {
  Success: boolean
  Message: string
  Data?: any
}

// 响应式数据
const settings = ref<Settings>({})
const saving = ref(false)
const testing = ref(false)
const uploading = ref(false)
const showUploadDialog = ref(false)
const uploadFile = ref(null)
const currentImageProperty = ref('')

// 编辑器引用
const stylesEditor = ref()
const scriptsEditor = ref()
let styleCodeMirrorEditor: any = null
let scriptCodeMirrorEditor: any = null

const destroyEditors = () => {
  const disposeEditor = (editorRef: any, containerRef: any) => {
    if (editorRef) {
      const wrapper = editorRef.getWrapperElement?.()
      if (wrapper && wrapper.parentNode) {
        wrapper.parentNode.removeChild(wrapper)
      }
    }

    if (containerRef?.value) {
      containerRef.value.innerHTML = ''
    }
  }

  disposeEditor(styleCodeMirrorEditor, stylesEditor)
  disposeEditor(scriptCodeMirrorEditor, scriptsEditor)

  styleCodeMirrorEditor = null
  scriptCodeMirrorEditor = null
}

// 水印选项
const watermarkOptions = [
  { label: '无水印', value: 'None' },
  { label: '文字水印', value: 'Text' },
  { label: '图片水印', value: 'Image' }
]

// 计算属性
const enableRss = computed({
  get: () => settings.value.EnableRss === 'true',
  set: (val: boolean) => { settings.value.EnableRss = val ? 'true' : 'false' }
})

const enableDonate = computed({
  get: () => settings.value.EnableDonate === 'true',
  set: (val: boolean) => { settings.value.EnableDonate = val ? 'true' : 'false' }
})

const closeSite = computed({
  get: () => settings.value.CloseSite === 'true',
  set: (val: boolean) => { settings.value.CloseSite = val ? 'true' : 'false' }
})

const dataReadonly = computed({
  get: () => settings.value.DataReadonly === 'true',
  set: (val: boolean) => { settings.value.DataReadonly = val ? 'true' : 'false' }
})

const enableSsl = computed({
  get: () => settings.value.EnableSsl === 'true',
  set: (val: boolean) => { settings.value.EnableSsl = val ? 'true' : 'false' }
})

// 加载系统设置
const loadSettings = async () => {
  const response = await api.get('/system/getsettings') as ApiResponse
  if (response?.Success && response.Data) {
    const settingsObj: Settings = {}
    response.Data.forEach((item: any) => {
      settingsObj[item.Name] = item.Value
    })
    settings.value = settingsObj

    // 初始化编辑器内容
    if (styleCodeMirrorEditor) {
      styleCodeMirrorEditor.setValue(settings.value.Styles || '')
    }
    if (scriptCodeMirrorEditor) {
      scriptCodeMirrorEditor.setValue(settings.value.Scripts || '')
    }
  } else {
    toast.error('获取系统设置失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 保存系统设置
const saveSettings = async () => {
  saving.value = true
  // 更新编辑器内容到设置
  if (styleCodeMirrorEditor) {
    settings.value.Styles = styleCodeMirrorEditor.getValue()
  }
  if (scriptCodeMirrorEditor) {
    settings.value.Scripts = scriptCodeMirrorEditor.getValue()
  }

  const response = await api.post('/system/save', Object.keys(settings.value).map(key => {
    return { Name: key, Value: settings.value[key] }
  })) as ApiResponse
  if (response?.Success) {
    toast.success(response.Message || '保存成功', { autoClose: 2000, position: 'top-center' })
  } else {
    toast.error(response?.Message || '保存失败', { autoClose: 2000, position: 'top-center' })
  }
  saving.value = false
}

// 测试邮件发送
const testEmail = async () => {
  testing.value = true
  try {
    const response = await api.post('/system/mailtest', {
      smtp: settings.value.SMTP,
      user: settings.value.EmailFrom,
      pwd: settings.value.EmailPwd,
      port: settings.value.SmtpPort,
      to: settings.value.ReceiveEmail,
      ssl: enableSsl.value
    }) as ApiResponse

    if (response?.Success) {
      toast.success('邮件发送成功', { autoClose: 2000, position: 'top-center' })
    } else {
      toast.error(response?.Message || '邮件发送失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('邮件发送失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error testing email:', error)
  } finally {
    testing.value = false
  }
}

// 测试路径
const testPath = async () => {
  try {
    const response = await api.post('/system/pathtest', {
      path: settings.value.PathRoot
    }) as ApiResponse

    if (response?.Success) {
      toast.success('路径测试成功', { autoClose: 2000, position: 'top-center' })
    } else {
      toast.error(response?.Message || '路径测试失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('路径测试失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error testing path:', error)
  }
}

// 显示图片上传对话框
const showImageUpload = (property: string) => {
  currentImageProperty.value = property
  showUploadDialog.value = true
}

// 取消上传
const cancelUpload = () => {
  showUploadDialog.value = false
  uploadFile.value = null
  currentImageProperty.value = ''
}

// 上传图片
const uploadImage = async () => {
  if (!uploadFile.value) {
    toast.error('请选择图片文件', { autoClose: 2000, position: 'top-center' })
    return
  }

  uploading.value = true
  try {
    const formData = new FormData()
    formData.append('file', uploadFile.value)

    const response = await api.post('/Upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    }) as ApiResponse

    if (response?.Success && response.Data) {
      settings.value[currentImageProperty.value] = response.Data
      toast.success('图片上传成功', { autoClose: 2000, position: 'top-center' })
      cancelUpload()
    } else {
      toast.error(response?.Message || '图片上传失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('图片上传失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error uploading image:', error)
  } finally {
    uploading.value = false
  }
}

// 初始化CodeMirror编辑器
const initEditors = () => {
  const CodeMirror = (window as any).CodeMirror
  if (!CodeMirror) {
    return
  }

  destroyEditors()

  if (stylesEditor.value) {
    styleCodeMirrorEditor = CodeMirror(stylesEditor.value, {
      value: settings.value.Styles || '',
      lineNumbers: true,
      mode: 'css'
    })
  }

  if (scriptsEditor.value) {
    scriptCodeMirrorEditor = CodeMirror(scriptsEditor.value, {
      value: settings.value.Scripts || '',
      lineNumbers: true,
      mode: 'javascript'
    })
  }
}

// 生命周期钩子
onMounted(async () => {
  document.querySelector('#save').scrollIntoView({ behavior: 'smooth', block: 'center' })
  await loadSettings()
  try {
    await loadCodeMirror()
    await nextTick()
    // 延迟初始化编辑器，确保DOM已渲染
    setTimeout(initEditors, 100)
  } catch (error) {
    toast.error('编辑器初始化失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error loading CodeMirror assets:', error)
  }
})

onUnmounted(() => {
  destroyEditors()
})
</script>
<style scoped lang="scss">
.system-settings-page {
  padding: 20px;
  margin: 0 auto;
}

.editor-container {
  border: 1px solid #ddd;
  border-radius: 4px;
}

:deep(.CodeMirror) {
  height: 100%;
}

:deep(.CodeMirror-scroll) {
  height: 100%;
}

.logo-preview {
  border-radius: 4px;
  transition: all 0.3s ease;

  &:hover {
    opacity: 0.8;
  }
}

.q-card {
  margin-bottom: 16px;
}

// 响应式设计
@media (max-width: 768px) {
  .system-settings-page {
    padding: 10px;
  }

  .row.q-gutter-md {

    .col-md-6,
    .col-md-4,
    .col-md-3 {
      margin-bottom: 16px;
    }
  }
}
</style>