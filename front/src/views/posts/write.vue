<template>
<div v-if="pageLoading">
  <q-inner-loading :showing="pageLoading">
    <q-spinner color="primary" size="100px" />
  </q-inner-loading>
</div>
<div class="write-blog-container" v-else>
  <div class="text-h6" v-if="route.query.id">编辑ID：{{ post.Id }}</div>
  <div class="text-h6" v-if="route.query.refer">复制来源ID：{{ route.query.refer }}</div>
  <!-- 文章标题 -->
  <div class="row">
    <q-input autogrow class="col" v-model="post.Title" placeholder="文章标题" outlined required :rules="[val => !!val || '请输入文章标题', val => val.length >= 2 || '标题至少2个字符', val => val.length <= 128 || '标题最多128个字符']" style="font-size: 26px;">
      <template v-slot:append>
        <q-btn dense size="lg" color="info" label="上传Word文档" @click="showWordUpload" class="full-width" icon="upload" no-caps />
      </template>
    </q-input>
  </div>
  <q-card flat bordered class="q-mb-md">
    <q-card-section>
      <div class="text-h6">文章内容：</div>
      <vue-ueditor-wrap ref="editor" v-model="post.Content" :config="{ initialFrameHeight: 500 }" />
    </q-card-section>
  </q-card>
  <!-- 文章加密设置 -->
  <q-card flat bordered class="q-mb-md">
    <q-card-section>
      <div class="row items-center">
        <q-select dense v-model="post.ProtectContentMode" :options="protectModeOptions" label="加密模式" outlined style="min-width: 200px;" map-options emit-value>
          <template v-slot:before>
            <div class="text-h6">文章加密内容</div>
          </template>
        </q-select>
        <!-- 地区可见配置 -->
        <div v-if="post.ProtectContentMode === 2" class="row items-center">
          <q-input autogrow dense v-model="post.ProtectContentRegions" label="可见地区" shadow-text="竖线分隔，支持国家、地区、城市、运营商、ASN" style="width: 450px;" outlined>
            <template v-slot:append>
              <q-select dense borderless v-model="post.ProtectContentLimitMode" :options="[{ label: '以内', value: 1 }, { label: '以外', value: 2 }]" emit-value map-options />
            </template>
          </q-input>
        </div>
        <!-- 密码可见配置 -->
        <q-input autogrow dense v-if="post.ProtectContentMode === 4" v-model="post.ProtectPassword" label="访问密码" type="password" outlined />
      </div>
      <div v-if="post.ProtectContentMode > 0">
        <vue-ueditor-wrap ref="protectContentEditor" v-model="post.ProtectContent" :config="{ initialFrameHeight: 300 }" />
      </div>
    </q-card-section>
  </q-card>
  <q-card flat bordered class="q-mb-md">
    <q-card-section>
      <div class="text-h6">作者信息</div>
      <div class="row q-gutter-md">
        <div class="col">
          <q-input autogrow dense v-model="post.Author" label="作者" outlined :rules="[val => !!val || '请输入作者', val => val.length <= 20 || '作者名最多20个字符']" />
        </div>
        <div class="col">
          <q-input autogrow dense v-model="post.Email" label="邮箱" type="email" outlined :rules="[val => !!val || '请输入邮箱', val => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val) || '邮箱格式不正确']" />
        </div>
      </div>
    </q-card-section>
  </q-card>
  <q-card flat bordered class="q-mb-md">
    <q-card-section>
      <div class="text-h6">文章设置</div>
      <div class="row q-mb-md">
        <div class="col">
          <q-select dense v-model="post.CategoryId" :options="filteredCategoryOptions" label="分类" outlined option-label="Name" option-value="Id" emit-value map-options use-input input-debounce="300" @filter="filterCategoryOptions">
            <template v-slot:no-option>
              <q-item>
                <q-item-section class="text-grey"> 没有找到匹配的分类 </q-item-section>
              </q-item>
            </template>
          </q-select>
        </div>
        <div class="col">
          <q-select dense v-model="post.Seminars" :options="filteredSeminarOptions" label="专题" outlined multiple use-chips option-label="Title" option-value="Id" map-options emit-value use-input input-debounce="300" @filter="filterSeminarOptions" clearable>
            <template v-slot:no-option>
              <q-item>
                <q-item-section class="text-grey"> 没有找到匹配的专题 </q-item-section>
              </q-item>
            </template>
            <template v-slot:selected-item="scope">
              <q-chip :style="{ backgroundColor: ['#FFB300', '#39B54A', '#00A1E9', '#F75000', '#8C6E63', '#E67E22'][scope.index % 6], color: 'white', fontSize: '11px' }" removable @remove="scope.removeAtIndex(scope.index)"> {{ scope.opt.Title }} </q-chip>
            </template>
          </q-select>
        </div>
        <div class="col">
          <q-select dense v-model="post.Labels" :options="filteredTagOptions" label="标签" outlined multiple use-chips use-input new-value-mode="add-unique" input-debounce="0" @filter="filterTagOptions" @new-value="createNewTag" clearable>
            <template v-slot:selected-item="scope">
              <q-chip :style="{ backgroundColor: ['#FFB300', '#39B54A', '#00A1E9', '#F75000', '#8C6E63', '#E67E22'][scope.index % 6], color: 'white', fontSize: '11px' }" removable @remove="scope.removeAtIndex(scope.index)"> {{ scope.opt }} </q-chip>
            </template>
          </q-select>
        </div>
        <div class="col">
          <q-tag-input clearable dense autogrow v-model="keywords" label="文章关键词" outlined />
        </div>
      </div>
      <div class="row">
        <div class="col-3">
          <q-input dense autogrow v-model="post.Redirect" label="跳转到第三方链接" shadow-text="如：https://baidu.com 留空不跳转" outlined hint="当跳转第三方链接时，文章内容不宜过多" />
        </div>
        <div class="col-3">
          <q-input dense v-model="post.ExpireAt" label="过期时间" outlined readonly>
            <template v-slot:append>
              <q-icon name="event" class="cursor-pointer">
                <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                  <q-date v-model="post.ExpireAt" :options="dateOptions" mask="YYYY-MM-DD" v-close-popup></q-date>
                </q-popup-proxy>
              </q-icon>
              <q-btn round flat icon="clear" @click="post.ExpireAt = ''" v-if="post.ExpireAt" />
            </template>
          </q-input>
        </div>
        <div class="col">
          <div class="row items-center">
            <q-checkbox v-model="post.IsNsfw" label="标记为不安全的内容" />
            <q-checkbox v-model="post.Reserve" label="保留历史版本" v-if="editMode" />
            <q-checkbox v-model="post.DisableCopy" label="禁止转载" />
            <q-checkbox v-model="post.DisableComment" label="禁止评论" />
            <q-checkbox v-model="post.schedule" label="定时发表" v-if="!editMode && !post.schedule" />
          </div>
        </div>
        <div class="col-3" v-if="post.schedule">
          <q-input dense v-model="post.timespan" label="计划发表时间" outlined readonly>
            <template v-slot:prepend>
              <span style="font-size: 14px;"><q-checkbox v-model="post.schedule" label="定时发表" v-if="!editMode" /></span>
            </template>
            <template v-slot:append>
              <q-icon name="event" class="cursor-pointer">
                <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                  <div style="min-height: 420px;">
                    <div class="row" style="min-width: 600px;">
                      <q-date v-model="post.timespan" mask="YYYY-MM-DD HH:mm" :options="dateOptions"></q-date>
                      <q-time v-if="post.timespan" v-model="post.timespan" mask="YYYY-MM-DD HH:mm" :options="timeOptions" />
                    </div>
                    <q-btn color="primary" label="确认" v-close-popup v-if="post.timespan" />
                  </div>
                </q-popup-proxy>
              </q-icon>
              <q-btn round flat icon="clear" @click="post.timespan = ''" v-if="post.timespan" />
            </template>
          </q-input>
        </div>
      </div>
      <div class="row q-gutter-md">
        <div class="col">
          <q-select dense v-model="post.LimitMode" :options="limitModeOptions" label="文章地区投放" outlined map-options emit-value></q-select>
        </div>
        <div v-if="post.LimitMode > 0 && post.LimitMode < 5" class="col">
          <q-select dense v-model="post.Regions" :options="filteredRegions" :label="post.LimitMode % 2 === 1 ? '可见地区' : '不可见地区'" :label-color="post.LimitMode % 2 === 1 ? '' : 'red'" outlined use-input new-value-mode="add-unique" input-debounce="0" @filter="filterRegions" @new-value="createNewRegion" hint="竖线分隔，如：江苏|苏州|移动|AS2333|DMIT，支持地区、运营商、ASN、机房名称">
            <template v-slot:append>
              <q-btn round flat icon="clear" @click="post.Regions = null" v-if="post.Regions" />
            </template>
          </q-select>
        </div>
        <div v-if="post.LimitMode === 3 || post.LimitMode === 4" class="col">
          <q-select dense v-model="post.ExceptRegions" :options="filteredExceptRegions" label="排除地区" outlined use-input new-value-mode="add-unique" input-debounce="0" @filter="filterExceptRegions" @new-value="createNewExceptRegion" hint="竖线分隔，如：江苏|苏州|移动|AS2333|DMIT，支持地区、运营商、ASN、机房名称">
            <template v-slot:append>
              <q-btn round flat icon="clear" @click="post.ExceptRegions = null" v-if="post.ExceptRegions" />
            </template>
          </q-select>
        </div><!-- 提交按钮 -->
        <div class="row items-start">
          <q-btn icon="send" type="submit" color="primary" label="发布文章" :loading="loading" @click="submitPost" />
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- Word文档上传对话框 -->
  <q-dialog v-model="showUploadDialog">
    <q-card style="min-width: 400px">
      <q-card-section>
        <div class="text-h6">上传Word文档</div>
        <div class="text-caption text-orange">注意：重复上传将会覆盖之前上传的内容！</div>
      </q-card-section>
      <q-card-section>
        <q-file v-model="uploadFile" label="选择Word文档" outlined accept=".doc,.docx" @input="handleFileSelect">
          <template v-slot:prepend>
            <q-icon name="attach_file" />
          </template>
        </q-file>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" color="grey" v-close-popup />
        <q-btn label="上传" color="primary" @click="uploadWordDocument" :loading="uploading" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <q-dialog v-model="showDraftDialog">
    <q-card style="min-width: 400px">
      <q-card-section>
        <div class="text-h6">检测到草稿</div>
        <div class="text-caption text-orange">检查到上次有未提交的草稿，是否加载？</div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" color="grey" v-close-popup @click="startDraftTimer" />
        <q-btn label="加载" color="primary" @click="loadDraft" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <q-inner-loading :showing="loading">
    <q-spinner color="primary" size="100px" />
  </q-inner-loading>
</div>
</template>
<script setup lang="ts">
import { ref, reactive, onMounted, onUnmounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { PostApi } from '@/api/PostApi'
import type { PostData, Category, Tag, Seminar } from '@/api/PostApi'
import { toast } from 'vue3-toastify'
import 'vue3-toastify/dist/index.css'
import { useUserStore } from '@/store/users'
import QTagInput from '@/components/QTagInput.vue'


const userStore = useUserStore();
const route = useRoute();
const router = useRouter()
const editor = ref(null);
const protectContentEditor = ref(null);
const editMode = ref(route.query.id > 0);

// 响应式数据
const post = reactive({
  Id: route.query.id ? Number(route.query.id) : 0,
  Title: '',
  Author: '',
  Email: '',
  Content: '',
  ProtectContent: '',
  ProtectContentMode: 0,
  ProtectContentRegions: '',
  ProtectContentLimitMode: 1,
  ProtectPassword: '',
  CategoryId: 1,
  Labels: [],
  Keyword: '',
  Seminars: [],
  DisableCopy: false,
  DisableComment: false,
  schedule: false,
  timespan: '',
  Redirect: '',
  IsNsfw: false,
  ExpireAt: '',
  LimitMode: 0,
  Regions: '',
  ExceptRegions: '',
  Reserve: true,
})
const keywords = computed({
  get() {
    if (post.Keyword) {
      return post.Keyword.split(',').filter(k => k.trim().length > 0)
    }
    return []
  },
  set(val: string[]) {
    post.Keyword = val.join(',')
  }
});
// UI 状态
const loading = ref(false)
const pageLoading = ref(true)
const uploading = ref(false)
const showUploadDialog = ref(false)
const showDraftDialog = ref(false)
const uploadFile = ref(null)

// 编辑器相关
let draftTimer: any = null

// 选项数据
const categoryOptions = ref([])
const filteredCategoryOptions = ref([])
const tagOptions = ref([])
const filteredTagOptions = ref([])
const seminarOptions = ref([])
const filteredSeminarOptions = ref([])
const limitRegions = ref([])
const filteredRegions = ref([])
const limitExceptRegions = ref([])
const filteredExceptRegions = ref([])

// 加密模式选项
const protectModeOptions = [
  { label: '无加密', value: 0 },
  { label: '评论可见', value: 1 },
  { label: '地区可见', value: 2 },
  { label: '授权可见', value: 3 },
  { label: '密码可见', value: 4 },
  { label: '仅搜索引擎可见', value: 5 }
]

// 地区投放模式选项
const limitModeOptions = [
  { label: '不限', value: 0 },
  { label: '指定地区可见', value: 1 },
  { label: '指定地区不可见', value: 2 },
  { label: '指定地区可见但排除在内的这些地区', value: 3 },
  { label: '指定地区不可见但排除在内的这些地区', value: 4 },
  { label: '仅搜索引擎可见', value: 5 }
]

// 加载分类数据
const loadCategories = async () => {
  try {
    const response = await PostApi.getCategories()
    if (response.Success) {
      categoryOptions.value = flattenCategories(response.Data) || []
      filteredCategoryOptions.value = categoryOptions.value
    }
  } catch (error) {
    console.error('加载分类失败:', error)
    toast.error('加载分类失败', { position: "top-center", autoClose: 3000 })
  }
}

// 过滤分类选项
const filterCategoryOptions = (val, update) => {
  update(() => {
    if (val === '') {
      filteredCategoryOptions.value = categoryOptions.value
    } else {
      const needle = val.toLowerCase()
      filteredCategoryOptions.value = categoryOptions.value.filter(
        option => option.Name.toLowerCase().indexOf(needle) > -1
      )
    }
  })
}

// 递归函数：将树形结构的分类数据平铺
const flattenCategories = (categories, parentName = '') => {
  const result = []

  for (const category of categories) {
    // 构建当前分类的完整名称
    const fullName = parentName ? `${parentName}/${category.Name}` : category.Name

    // 添加当前分类到结果中
    result.push({
      ...category,
      Name: fullName,
      OriginalName: category.Name // 保存原始名称
    })

    // 递归处理子分类
    if (category.Children && category.Children.length > 0) {
      const childCategories = flattenCategories(category.Children, fullName)
      result.push(...childCategories)
    }
  }

  return result
}

// 加载标签数据
const loadTags = async () => {
  try {
    const response = await PostApi.getTags()
    if (response.Success) {
      tagOptions.value = response.Data || []
      filteredTagOptions.value = tagOptions.value
    }
  } catch (error) {
    console.error('加载标签失败:', error)
    toast.error('加载标签失败', { position: "top-center", autoClose: 3000 })
  }
}

// 过滤标签选项
const filterTagOptions = (val: string, update: Function) => {
  if (val === '') {
    update(() => {
      filteredTagOptions.value = tagOptions.value
    })
    return
  }

  update(() => {
    const needle = val?.toLowerCase()
    filteredTagOptions.value = tagOptions.value.filter(
      (v: any) => v?.toLowerCase().indexOf(needle) > -1
    )
  })
}

// 创建新标签
const createNewTag = (val: string, done: Function) => {
  // 检查是否已存在该标签
  const exists = tagOptions.value.find((tag: any) => tag.TagName === val)
  if (!exists && val.length > 0) {
    // 创建新标签对象
    const newTag = {
      Id: Date.now(), // 临时ID
      TagName: val
    }

    // 添加到标签选项中
    tagOptions.value.push(newTag)
    filteredTagOptions.value = tagOptions.value

    // 调用done函数添加到选中值
    done(val)
  }
}

// 加载专题数据
const loadSeminars = async () => {
  const response = await PostApi.getSeminars()
  if (response.Success) {
    seminarOptions.value = response.Data || []
    filteredSeminarOptions.value = seminarOptions.value
  }
}

// 过滤专题选项
const filterSeminarOptions = (val, update) => {
  update(() => {
    if (val === '') {
      filteredSeminarOptions.value = seminarOptions.value
    } else {
      const needle = val.toLowerCase()
      filteredSeminarOptions.value = seminarOptions.value.filter(
        option => option.Title.toLowerCase().indexOf(needle) > -1
      )
    }
  })
}

const loadRegions = async () => {
  let response = await PostApi.getRegions()
  if (response.Success) {
    limitRegions.value = response.Data || []
    filteredRegions.value = limitRegions.value
  }
  response = await PostApi.getRegions('ExceptRegions')
  if (response.Success) {
    limitExceptRegions.value = response.Data || []
  }
}

// 过滤地区选项
const filterRegions = (val: string, update: Function) => {
  if (val === '') {
    update(() => {
      filteredRegions.value = limitRegions.value
    })
    return
  }

  update(() => {
    const needle = val?.toLowerCase()
    filteredRegions.value = limitRegions.value.filter(
      (v: any) => v?.toLowerCase().indexOf(needle) > -1
    )
  })
}

// 创建新地区
const createNewRegion = (val: string, done: Function) => {
  // 检查是否已存在该地区
  const exists = limitRegions.value.find((region: any) => region === val)
  if (!exists && val.length > 0) {
    // 添加到地区选项中
    limitRegions.value.push(val)
    filteredRegions.value = limitRegions.value

    // 调用done函数添加到选中值
    done(val)
  }
}

// 过滤地区选项
const filterExceptRegions = (val: string, update: Function) => {
  if (val === '') {
    update(() => {
      filteredExceptRegions.value = limitExceptRegions.value
    })
    return
  }

  update(() => {
    const needle = val?.toLowerCase()
    filteredExceptRegions.value = limitExceptRegions.value.filter(
      (v: any) => v?.toLowerCase().indexOf(needle) > -1
    )
  })
}

// 创建新地区
const createNewExceptRegion = (val: string, done: Function) => {
  // 检查是否已存在该地区
  const exists = limitExceptRegions.value.find((region: any) => region === val)
  if (!exists && val.length > 0) {
    // 添加到地区选项中
    limitExceptRegions.value.push(val)
    filteredExceptRegions.value = limitExceptRegions.value

    // 调用done函数添加到选中值
    done(val)
  }
}

// 显示Word文档上传对话框
const showWordUpload = () => {
  showUploadDialog.value = true
}

// 处理文件选择
const handleFileSelect = (file: File) => {
  uploadFile.value = file
}

// 上传Word文档
const uploadWordDocument = async () => {
  if (!uploadFile.value) {
    toast.error('请选择要上传的Word文档', { position: "top-center", autoClose: 3000 })
    return
  }

  uploading.value = true

  try {
    const response = await PostApi.uploadWord(uploadFile.value)

    if (response.Success) {
      // 将Word内容插入编辑器
      post.Title = response.Data?.Title || post.Title
      post.Content = response.Data?.Content || ''

      toast.success('Word文档上传成功', { position: "top-center", autoClose: 3000 })

      showUploadDialog.value = false
      uploadFile.value = null
    } else {
      toast.error(response.Message || 'Word文档上传失败', { position: "top-center", autoClose: 3000 })
    }
  } catch (error) {
    console.error('上传Word文档失败:', error)
    toast.error('上传失败，请重试', { position: "top-center", autoClose: 3000 })
  } finally {
    uploading.value = false
  }
}

// 提交文章
const submitPost = async () => {
  // 表单验证
  if (!post.Title || post.Title.trim().length < 2) {
    toast.error('文章标题至少需要2个字符', { position: "top-center", autoClose: 3000 });
    return
  }

  if (!post.Content || post.Content.length < 20) {
    toast.error('文章内容至少需要20个字符', { position: "top-center", autoClose: 3000 })
    return
  }

  // 验证定时发表时间
  if (post.schedule && post.timespan) {
    const scheduledTime = new Date(post.timespan)
    const now = new Date()
    if (scheduledTime <= now) {
      toast.error('定时发表时间必须大于当前时间', { position: "top-center", autoClose: 3000 })
      return
    }
  } else if (post.schedule && !post.timespan) {
    toast.error('请选择定时发表时间', { position: "top-center", autoClose: 3000 })
    return
  }

  loading.value = true

  try {
    post.Label = post.Labels.join(',')
    const response = editMode.value ? await PostApi.editPost(post) : await PostApi.writePost(post)

    if (response.Success) {
      toast.success('文章发布成功', { position: "top-center", autoClose: 3000 })

      // 清除草稿
      localStorage.removeItem('write-post-draft:' + post.Id)
      clearInterval(draftTimer);

      // 跳转到文章列表
      router.push('/posts/list')
    } else {
      toast.error(response.Message || '文章发布失败', { position: "top-center", autoClose: 3000 })
    }
  } catch (error) {
    console.error('提交文章失败:', error)
    toast.error('提交失败，请重试', { position: "top-center", autoClose: 3000 })
  } finally {
    loading.value = false
  }
}

// 加载草稿
const loadDraft = () => {
  const draftData = localStorage.getItem('write-post-draft:' + post.Id)
  if (draftData) {
    const draft = JSON.parse(draftData)
    Object.assign(post, draft)
  }
  startDraftTimer()
  showDraftDialog.value = false
}

// 启动草稿定时器
const startDraftTimer = () => {
  draftTimer = setInterval(() => {
    const draftData = { ...post }
    localStorage.setItem('write-post-draft:' + post.Id, JSON.stringify(draftData))
  }, 5000) // 每5秒保存一次
}

// 日期选择限制 - 只能选择今天及以后的日期
const dateOptions = (date: string) => {
  const today = new Date()
  const selectedDate = new Date(date)
  return selectedDate >= new Date(today.getFullYear(), today.getMonth(), today.getDate())
}

// 时间选择限制 - 如果是今天，只能选择当前时间之后的时间
const timeOptions = (hr: number, min?: number) => {
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())

  // 如果没有选择日期或选择的日期为空，允许所有时间
  if (!post.timespan) return true

  // 获取选择的日期部分
  const selectedDateStr = post.timespan.split(' ')[0]
  if (!selectedDateStr) return true

  const selectedDate = new Date(selectedDateStr)

  // 如果选择的不是今天，允许所有时间
  if (selectedDate.getTime() !== today.getTime()) return true

  // 如果是今天，检查时间限制
  const currentHour = now.getHours()
  const currentMinute = now.getMinutes()

  if (min !== undefined) {
    // 检查分钟
    return hr > currentHour || (hr === currentHour && min > currentMinute)
  } else {
    // 检查小时
    return hr >= currentHour
  }
}

// 从URL参数加载引用文章
const loadReferPost = async () => {
  const urlParams = new URLSearchParams(window.location.search)
  const referId = route.query.refer || route.query.id || urlParams.get('refer')

  if (referId) {
    const response = await PostApi.getPost(parseInt(referId))
    if (response.Success && response.Data) {
      const referData = response.Data
      referData.Id = route.query.id || 0
      referData.Labels = referData.Label?.split(',') || []
      Object.assign(post, referData)
    }
  }
}

onMounted(async () => {
  const user = await userStore.getUserInfo();
  post.Author = user.NickName
  post.Email = user.Email
  await Promise.all([loadCategories(), loadTags(), loadSeminars(), loadReferPost(), loadRegions()])
  // 检查草稿
  showDraftDialog.value = JSON.parse(localStorage.getItem('write-post-draft:' + post.Id))?.Content?.length > 0
  if (!showDraftDialog.value) {
    startDraftTimer()
  }
  document.querySelector('.q-page-container').addEventListener('scroll', handleScroll)
  pageLoading.value = false
})

// 组件卸载
onUnmounted(() => {
  if (draftTimer) {
    clearInterval(draftTimer)
  }
  if (scrollDebounceTimer) {
    clearTimeout(scrollDebounceTimer)
  }
  // 清理滚动事件监听器
  document.querySelector('.q-page-container')?.removeEventListener('scroll', handleScroll)
})

let scrollDebounceTimer: any = null
let editorToolbarFixed = false
let protectEditorToolbarFixed = false

// 滚动事件处理 - 优化版本，解决抖动问题
const handleScroll = () => {
  // 清除之前的防抖定时器
  if (scrollDebounceTimer) {
    clearTimeout(scrollDebounceTimer)
  }

  // 使用防抖，减少频繁执行
  scrollDebounceTimer = setTimeout(() => {
    handleScrollLogic()
  }, 100)
}

// 实际的滚动处理逻辑
const handleScrollLogic = () => {
  // 处理主编辑器工具栏
  if (editor.value?.$el) {
    const toolbar = editor.value.$el.querySelector('.edui-editor-toolbarbox')
    if (toolbar) {
      const editorRect = editor.value.$el.getBoundingClientRect()
      const shouldFix = editorRect.top < 84 && editorRect.bottom > 300

      // 只在状态真正改变时才操作DOM
      if (shouldFix && !editorToolbarFixed) {
        // 固定工具栏
        const editorWidth = editor.value.$el.offsetWidth
        toolbar.style.cssText = `
          position: fixed !important;
          top: 84px !important;
          z-index: 1000 !important;
          width: ${editorWidth}px !important;
          transition: all 0.5s ease !important;
        `
        editorToolbarFixed = true
      } else if (!shouldFix && editorToolbarFixed) {
        // 取消固定
        toolbar.style.cssText = `
          position: static !important;
          top: auto !important;
          z-index: auto !important;
          width: auto !important;
          transition: all 0.5s ease !important;
        `
        editorToolbarFixed = false
      }
    }
  }

  // 处理保护内容编辑器工具栏
  if (protectContentEditor.value?.$el) {
    const toolbar2 = protectContentEditor.value.$el.querySelector('.edui-editor-toolbarbox')
    if (toolbar2) {
      const protectEditorRect = protectContentEditor.value.$el.getBoundingClientRect()
      const shouldFix = protectEditorRect.top < 84 && protectEditorRect.bottom > 300

      // 只在状态真正改变时才操作DOM
      if (shouldFix && !protectEditorToolbarFixed) {
        // 固定工具栏
        const protectEditorWidth = protectContentEditor.value.$el.offsetWidth
        toolbar2.style.cssText = `
          position: fixed !important;
          top: 84px !important;
          z-index: 1000 !important;
          width: ${protectEditorWidth}px !important;
          transition: all 0.2s ease !important;
        `
        protectEditorToolbarFixed = true
      } else if (!shouldFix && protectEditorToolbarFixed) {
        // 取消固定
        toolbar2.style.cssText = `
          position: static !important;
          top: auto !important;
          z-index: auto !important;
          width: auto !important;
          transition: all 0.2s ease !important;
        `
        protectEditorToolbarFixed = false
      }
    }
  }
}

</script>
<style scoped>
.write-blog-container {
  margin-top: 10px;
}
</style>
