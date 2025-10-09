<template>
<div class="post-list-container">
  <q-card>
    <q-card-section><!-- 顶部控制栏 -->
      <div class="row q-gutter-md">
        <div class="col">
          <q-select multiple v-model="showColumns" :options="columnOptions" outlined dense map-options emit-value use-chips @update:model-value="saveShowColumns">
            <template v-slot:prepend>
              <span style="font-size: 16px;">显示列:</span>
            </template>
          </q-select>
        </div>
        <div class="text-right">
          <div class="row q-gutter-md">
            <q-input class="col" v-model="searchKeyword" placeholder="全局搜索" outlined dense style="min-width: 200px" clearable @update:model-value="loadPageData" debounce="1000">
              <template v-slot:append>
                <span style="font-size: 12px;"><q-checkbox size="sm" v-model="useRegex" label="正则" @update:model-value="loadPageData" dense /></span>
              </template>
            </q-input>
            <!-- 分类选择 -->
            <q-select class="col" v-model="selectedCategory" :options="filteredCategoryOptions" option-value="Id" option-label="Name" label="分类" outlined dense style="min-width: 200px" map-options use-input input-debounce="300" @filter="filterCategoryOptions" clearable @update:model-value="loadPageData">
              <template v-slot:no-option>
                <q-item>
                  <q-item-section class="text-grey"> 没有找到匹配的分类 </q-item-section>
                </q-item>
              </template>
            </q-select>
            <!-- 排序方式选择 -->
            <q-select class="col search-btn" v-model="orderBy" :options="sortOptions" option-value="value" option-label="name" label="排序方式" outlined dense style="min-width: 220px" map-options emit-value @update:model-value="loadPageData">
              <template #after>
                <q-btn color="primary" icon="refresh" :loading="loading" @click="loadPageData"> 搜索 </q-btn>
              </template>
            </q-select>
          </div>
        </div>
      </div>
      <!-- 主表格 -->
      <div style="height: calc(100vh - 240px);margin-top: 10px;">
        <vxe-table ref="tableRef" :data="tableData" :loading="loading" resizable border height="auto" :row-config="{ isCurrent: true, isHover: true }" :row-class-name="getRowClassName">
          <!-- 标题列 -->
          <vxe-column field="Title" title="标题" min-width="240" fixed="left">
            <template #default="{ row }">
              <q-tooltip class="bg-grey" v-if="row.AverageViewCount < 1 && row.IsNsfw === false && row.LimitDesc === '无限制'">日均浏览量低于1</q-tooltip>
              <a :href="`/${row.Id}`" target="_blank" :class="{ 'text-grey text-bold': row.AverageViewCount < 1 && row.IsNsfw === false && row.LimitDesc === '无限制' }"> {{ row.Title }} </a>
            </template>
          </vxe-column>
          <!-- 作者列 -->
          <vxe-column field="Author" title="作者" width="120">
            <template #default="{ row }">
              <q-tooltip class="bg-grey" v-if="row.AverageViewCount < 1 && row.IsNsfw === false && row.LimitDesc === '无限制'">日均浏览量低于1</q-tooltip>
              <a :href="`/author/${row.Author}`" target="_blank" :class="{ 'text-grey text-bold': row.AverageViewCount < 1 && row.IsNsfw === false && row.LimitDesc === '无限制' }">{{ row.Author }}</a>
            </template>
          </vxe-column>
          <!-- 作者邮箱列 -->
          <vxe-column field="Email" title="作者邮箱" width="180" /><!-- 分类列 -->
          <vxe-column field="CategoryId" title="分类" width="200">
            <template #default="{ row }">
              <q-select outlined v-model="row.CategoryId" :options="filteredCategoryOptions" option-value="Id" option-label="Name" map-options emit-value dense borderless @update:model-value="(val) => changeCategory(row.Id, val)" use-input input-debounce="300" @filter="filterCategoryOptions" />
            </template>
          </vxe-column>
          <!-- 专题列 -->
          <vxe-column field="Seminars" title="专题" width="270">
            <template #default="{ row }">
              <q-select use-chips outlined v-model="row.Seminars" :options="filteredSeminarOptions" option-value="Id" option-label="Title" multiple dense borderless @update:model-value="(val) => changeSeminar(row.Id, val)" map-options use-input input-debounce="300" @filter="filterSeminarOptions" />
            </template>
          </vxe-column>
          <!-- 阅读量列-->
          <vxe-column v-if="showColumns.includes('ViewCount')" field="ViewCount" title="阅读" min-width="70">
            <template #default="{ row }">
              <q-tooltip class="bg-grey" v-if="row.AverageViewCount < 1">日均浏览量低于1</q-tooltip>
              <span :class="{ 'text-grey': row.AverageViewCount < 1 }"> {{ row.ViewCount }} </span>
            </template>
          </vxe-column>
          <!-- 在看列-->
          <vxe-column v-if="showColumns.includes('Online')" field="Online" title="在看" width="60" />
          <!-- 发表时间列-->
          <vxe-column v-if="showColumns.includes('PostDate')" field="PostDate" title="发表" width="140">
            <template #default="{ row }"> {{ formatDate(row.PostDate) }} </template>
          </vxe-column>
          <!-- 修改时间列 -->
          <vxe-column field="ModifyDate" title="修改" width="140">
            <template #default="{ row }"> {{ formatDate(row.ModifyDate) }} </template>
          </vxe-column>
          <!-- 修改次数列-->
          <vxe-column v-if="showColumns.includes('ModifyCount')" field="ModifyCount" title="修改次数" width="80" />
          <!-- 标签列-->
          <vxe-column v-if="showColumns.includes('Label')" field="Label" title="标签" width="160" />
          <!-- 支持数列-->
          <vxe-column v-if="showColumns.includes('VoteUpCount')" field="VoteUpCount" title="支持" width="60" />
          <!-- 状态列 -->
          <vxe-column field="Status" title="状态" width="80">
            <template #default="{ row }">
              <q-chip :color="getStatusColor(row.Status)" text-color="white" :label="row.Status" dense />
            </template>
          </vxe-column>
          <!-- 权限控制相关列 -->
          <vxe-column v-if="showColumns.includes('LimitDesc')" field="LimitDesc" title="是否限区" width="100" />
          <vxe-column v-if="showColumns.includes('DisableComment')" title="禁止评论" width="100">
            <template #default="{ row }">
              <q-toggle v-model="row.DisableComment" @update:model-value="() => toggleDisableComment(row)" />
            </template>
          </vxe-column>
          <vxe-column v-if="showColumns.includes('DisableCopy')" title="禁止转载" width="100">
            <template #default="{ row }">
              <q-toggle v-model="row.DisableCopy" @update:model-value="() => toggleDisableCopy(row)" />
            </template>
          </vxe-column>
          <vxe-column v-if="showColumns.includes('Rss')" title="开启RSS" width="100">
            <template #default="{ row }">
              <q-toggle v-model="row.Rss" @update:model-value="() => rssSwitch(row.Id)" />
            </template>
          </vxe-column>
          <vxe-column v-if="showColumns.includes('Locked')" title="锁定编辑" width="100">
            <template #default="{ row }">
              <q-toggle v-model="row.Locked" @update:model-value="() => lockedSwitch(row.Id)" />
            </template>
          </vxe-column>
          <vxe-column v-if="showColumns.includes('IsNsfw')" title="不安全内容" width="120">
            <template #default="{ row }">
              <q-toggle v-model="row.IsNsfw" @update:model-value="() => nsfwSwitch(row.Id)" />
            </template>
          </vxe-column>
          <!-- 操作列 -->
          <vxe-column title="操作" width="180" fixed="right">
            <template #default="{ row }">
              <!-- 审核通过按钮 -->
              <q-btn v-if="row.Status === '审核中'" dense flat size="md" color="positive" icon="check" @click="passPost(row)">
                <q-tooltip class="bg-green">通过审核</q-tooltip>
              </q-btn>
              <!-- 编辑按钮 -->
              <q-btn size="md" dense flat color="positive" icon="edit" :to="`/posts/write?id=${row.Id}`">
                <q-tooltip class="bg-positive">编辑</q-tooltip>
              </q-btn>
              <!-- 复制按钮 -->
              <q-btn size="md" dense flat color="info" icon="content_copy" :to="`/posts/write?refer=${row.Id}`">
                <q-tooltip class="bg-info">复制</q-tooltip>
              </q-btn>
              <!-- 下架按钮 -->
              <q-btn v-if="row.Status !== '已下架'" dense flat size="md" color="negative" icon="archive">
                <q-tooltip class="bg-negative">下架</q-tooltip>
                <q-popup-proxy>
                  <q-banner style="width: 180px;">
                    <p>确认下架这篇文章吗？</p>
                    <q-btn color="negative" size="sm" label="确认" @click="takedownPost(row)" v-close-popup />
                  </q-banner>
                </q-popup-proxy>
              </q-btn>
              <!-- 上架按钮 -->
              <q-btn v-if="row.Status === '已下架'" dense flat size="md" color="positive" icon="unarchive" @click="takeupPost(row)">
                <q-tooltip class="bg-positive">上架</q-tooltip>
              </q-btn>
              <!-- 删除按钮 -->
              <q-btn v-if="row.Status === '已下架'" dense flat size="md" color="negative" icon="delete">
                <q-tooltip class="bg-negative">删除</q-tooltip>
                <q-popup-proxy>
                  <q-banner>
                    <p>确认彻底删除这篇文章吗？</p>
                    <q-btn size="sm" color="negative" label="确认" @click="deletePost(row)" v-close-popup />
                  </q-banner>
                </q-popup-proxy>
              </q-btn>
              <!-- 置顶按钮 -->
              <q-btn v-if="row.Status !== '已下架'" dense flat size="md" :color="row.IsFixedTop ? 'positive' : 'primary'" icon="push_pin" @click="toggleFixTop(row.Id)">
                <q-tooltip :class="row.IsFixedTop ? 'bg-positive' : 'bg-primary'">切换置顶</q-tooltip>
              </q-btn>
              <!-- 统计按钮 -->
              <q-btn dense flat size="md" color="info" icon="analytics" @click="showInsight(row)">
                <q-tooltip class="bg-info">统计信息</q-tooltip>
              </q-btn>
            </template>
          </vxe-column>
        </vxe-table>
      </div>
      <!-- 分页组件 -->
      <div class="q-mt-md flex justify-center items-center ">
        <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.rowsPerPage)" :max-pages="10" boundary-numbers @update:model-value="loadPageData" />
        <q-select v-model="pagination.rowsPerPage" :options="[10, 15, 20, 30, 50, 100, 200]" label="每页条数" outlined dense style="width: 120px" @update:model-value="onPageSizeChange" />
        <div class="text-caption"> 共 {{ pagination.total }} 条，第 {{ pagination.page }} / {{ Math.ceil(pagination.total / pagination.rowsPerPage) }} 页 </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 统计信息展示区域 -->
  <div v-if="aggregateData" class="q-mt-lg">
    <div class="row q-gutter-md">
      <!-- 今日热榜 -->
      <div v-if="aggregateData.trending?.length > 0" class="col">
        <q-card>
          <q-card-section>
            <div class="text-h6 text-center">今日热榜（总阅读量：{{ aggregateData.readCount }}）</div>
          </q-card-section>
          <q-separator />
          <q-card-section>
            <vxe-table :data="aggregateData.trending" stripe border>
              <vxe-column field="index" title="序号" width="80">
                <template #default="{ rowIndex }">{{ rowIndex + 1 }}</template>
              </vxe-column>
              <vxe-column field="Title" title="标题">
                <template #default="{ row }">
                  <a :href="`/${row.Id}`" target="_blank">{{ row.Title }}</a>
                </template>
              </vxe-column>
              <vxe-column field="ViewCount" title="今日访问量" width="120" align="center" />
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
      <!-- 当前在看排行榜 -->
      <div v-if="aggregateData.mostHots?.length > 0" class="col">
        <q-card>
          <q-card-section>
            <div class="text-h6 text-center">当前在看排行榜</div>
          </q-card-section>
          <q-separator />
          <q-card-section>
            <vxe-table :data="aggregateData.mostHots" stripe border>
              <vxe-column field="index" title="序号" width="80">
                <template #default="{ rowIndex }">{{ rowIndex + 1 }}</template>
              </vxe-column>
              <vxe-column field="Title" title="标题">
                <template #default="{ row }">
                  <a :href="`/${row.Id}`" target="_blank">{{ row.Title }}</a>
                </template>
              </vxe-column>
              <vxe-column field="ViewCount" title="在看人数" width="120" align="center" />
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
    </div>
    <div class="row q-mt-md q-gutter-md">
      <!-- 日均访问量排行榜 -->
      <div v-if="aggregateData.mostAverage?.length > 0" class="col">
        <q-card>
          <q-card-section>
            <div class="text-h6 text-center">日均访问量排行榜</div>
          </q-card-section>
          <q-separator />
          <q-card-section>
            <vxe-table :data="aggregateData.mostAverage" stripe border>
              <vxe-column field="index" title="序号" width="80">
                <template #default="{ rowIndex }">{{ rowIndex + 1 }}</template>
              </vxe-column>
              <vxe-column field="Title" title="标题">
                <template #default="{ row }">
                  <a :href="`/${row.Id}`" target="_blank">{{ row.Title }}</a>
                </template>
              </vxe-column>
              <vxe-column field="ViewCount" title="日均访问量" width="120" align="center" />
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
      <!-- 访问量最高排行榜 -->
      <div v-if="aggregateData.mostView?.length > 0" class="col">
        <q-card>
          <q-card-section>
            <div class="text-h6 text-center">访问量最高排行榜</div>
          </q-card-section>
          <q-separator />
          <q-card-section>
            <vxe-table :data="aggregateData.mostView" stripe border>
              <vxe-column field="index" title="序号" width="80">
                <template #default="{ rowIndex }">{{ rowIndex + 1 }}</template>
              </vxe-column>
              <vxe-column field="Title" title="标题">
                <template #default="{ row }">
                  <a :href="`/${row.Id}`" target="_blank">{{ row.Title }}</a>
                </template>
              </vxe-column>
              <vxe-column field="ViewCount" title="访问量" width="120" align="center" />
            </vxe-table>
          </q-card-section>
        </q-card>
      </div>
    </div>
  </div>
  <!-- 访问趋势图表 -->
  <div class="q-mt-lg">
    <q-card>
      <q-card-section>
        <div class="flex items-center justify-between">
          <div class="text-h6">访问趋势</div>
          <div class="flex items-center ">
            <q-select prefix="对比最近" v-model="chartPeriod" :options="chartPeriodOptions" option-value="value" option-label="label" map-options emit-value outlined dense @update:model-value="updateChart" />
          </div>
        </div>
      </q-card-section>
      <q-separator />
      <q-card-section>
        <div ref="chartContainer" style="height: 500px"></div>
      </q-card-section>
    </q-card>
  </div><q-dialog v-model="showInsightDialog" persistent maximized>
    <q-card>
      <q-bar>
        <q-space />
        <q-btn dense flat icon="close" v-close-popup @click="showInsightDialog = false; closeInsight()" />
      </q-bar>
      <q-card-actions>
        <iframe ref="insightFrame" :src="insightSrc" width="100%" frameborder="0" scrolling="yes" @load="handleIframeLoad"></iframe>
      </q-card-actions>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { toast } from 'vue3-toastify'
import dayjs from 'dayjs'
import * as echarts from 'echarts'
import api from '@/axios/AxiosConfig'
import globalConfig from '@/config'

const insightFrame = ref(null)
const insightSrc = ref('')
// 处理iframe加载完成
const handleIframeLoad = () => {
  // 由于跨域限制，我们无法访问iframe内容
  // 设置一个固定高度或通过postMessage与iframe通信
  if (insightFrame.value) {
    insightFrame.value.style.height = 'calc(100vh - 50px)'
  }
}

// 响应式数据
const loading = ref(false)
const tableData = ref([])
const searchKeyword = ref('')
const useRegex = ref(false)
const selectedCategory = ref(null)
const orderBy = ref(1)
const showInsightDialog = ref(false);

// 分页数据
const pagination = ref({
  page: 1,
  rowsPerPage: 15,
  total: 0
})

// 选项数据
const categoryOptions = ref([])
const filteredCategoryOptions = ref([])
const seminarOptions = ref([])
const filteredSeminarOptions = ref([])
const aggregateData = ref(null)

// 表格引用
const tableRef = ref(null)

// 图表相关
const chartContainer = ref(null)
const chartPeriod = ref(30)
let chartInstance = null

// EventSource 引用
let eventSource = null

const showColumns = ref([]);
const columnOptions = [
  { label: "阅读量", value: 'ViewCount' },
  { label: "在看数", value: 'Online' },
  { label: "发表时间", value: 'PostDate' },
  { label: "修改次数", value: 'ModifyCount' },
  { label: "标签", value: 'Label' },
  { label: "支持数", value: 'VoteUpCount' },
  { label: "是否限区", value: 'LimitDesc' },
  { label: "禁止评论", value: 'DisableComment' },
  { label: "禁止转载", value: 'DisableCopy' },
  { label: "锁定编辑", value: 'Locked' },
  { label: "不安全内容", value: 'IsNsfw' },
]

// 排序选项
const sortOptions = [
  { name: "发表时间", value: 0 },
  { name: "最后修改", value: 1 },
  { name: "访问量最多", value: 2 },
  { name: "支持数最多", value: 4 },
  { name: "每日平均访问量(发布以来)", value: 5 },
  { name: "每日平均访问量(最近一年)", value: 6 }
]

// 图表周期选项
const chartPeriodOptions = [
  { label: '一周', value: 7 },
  { label: '15天', value: 15 },
  { label: '一个月', value: 30 },
  { label: '两个月', value: 60 },
  { label: '三个月', value: 90 },
  { label: '半年', value: 180 },
  { label: '一年', value: 365 }
]

// 计算属性
const diffDateFromNow = (date) => {
  return dayjs().diff(dayjs(date), 'day')
}

// 方法
const formatDate = (date) => {
  return dayjs(date).format('YYYY-MM-DD HH:mm')
}

const getRowClassName = ({ row }) => {
  return diffDateFromNow(row.ModifyDate) > 365 ? 'warning-row' : ''
}

// 获取状态对应的颜色
const getStatusColor = (status) => {
  const statusColorMap = {
    已发表: 'positive',
    审核中: 'warning',
    已下架: 'negative',
    草稿: 'grey',
    待审核: 'info',
    已驳回: 'dark'
  }
  return statusColorMap[status] || 'grey'
}

// 加载数据
const loadPageData = async () => {
  loading.value = true
  try {
    const params = {
      page: pagination.value.page,
      size: pagination.value.rowsPerPage,
      kw: searchKeyword.value,
      useRegex: useRegex.value,
      orderby: orderBy.value,
      cid: selectedCategory.value?.Id || ''
    }

    const data = await api.get('/post/getpagedata', { params });

    if (data) {
      tableData.value = data?.Data || []
      pagination.value.total = data?.TotalCount || 0
      localStorage.setItem('postlist-params', JSON.stringify(params))
    }
  } catch (error) {
    toast.error('加载数据失败', { autoClose: 2000, position: 'top-center' })
  } finally {
    loading.value = false
  }
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

// 加载分类数据
const loadCategories = async () => {
  try {
    const data = await api.get('/category/getcategories')

    if (data?.Success) {
      // 先平铺分类数据
      const flattenedCategories = flattenCategories(data?.Data || [])
      // 添加"全部"选项到开头
      const categories = [{ Name: "全部", Id: "" }].concat(flattenedCategories)
      categoryOptions.value = categories
      filteredCategoryOptions.value = categories
    }
  } catch (error) {
    toast.error('获取文章分类失败！', { autoClose: 2000, position: 'top-center' })
  }
}

// 加载专题数据
const loadSeminars = async () => {
  try {
    const data = await api.get('/seminar/getall')

    if (data) {
      seminarOptions.value = data?.Data || []
      filteredSeminarOptions.value = seminarOptions.value
    }
  } catch (error) {
    toast.error('加载专题失败！', { autoClose: 2000, position: 'top-center' })
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

const saveShowColumns = () => {
  localStorage.setItem('showColumns', showColumns.value.join(','))
}

const onPageSizeChange = () => {
  pagination.value.page = 1
  loadPageData()
}

// 操作方法
const passPost = async (row) => {
  const data = await api.post('/post/pass', row)
  toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
  await loadPageData()
}

const takedownPost = async (row) => {
  const data = await api.post(`/post/takedown/${row.Id}`)
  toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
  await loadPageData()
}

const takeupPost = async (row) => {
  try {
    const data = await api.post(`/post/Takeup/${row.Id}`)

    toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })

    await loadPageData()
  } catch (error) {
    toast.error('上架失败！', { autoClose: 2000, position: 'top-center' })
  }
}

const deletePost = async (row) => {
  try {
    const data = await api.post(`/post/truncate/${row.Id}`)
    toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
    await loadPageData()
  } catch (error) {
    toast.error('删除失败！', { autoClose: 2000, position: 'top-center' })
  }
}

const toggleFixTop = async (id) => {
  const data = await api.post(`/post/Fixtop/${id}`)
  toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
  await loadPageData()
}
const iframMessageHandler = (event) => {
  // 验证消息来源
  if (event.origin !== window.location.origin && !event.origin.includes('localhost')) {
    return
  }

  // 如果iframe发送高度信息
  if (event.data && event.data.type === 'resize' && event.data.height) {
    if (insightFrame.value) {
      insightFrame.value.style.height = event.data.height + 'px'
    }
  }
};
const closeInsight = () => {
  window.removeEventListener('message', iframMessageHandler)
}
const showInsight = (row) => {
  insightSrc.value = globalConfig.baseURL + `/${row.Id}/insight`
  showInsightDialog.value = true
  window.addEventListener('message', iframMessageHandler);
}

const changeCategory = async (postId, categoryId) => {
  try {
    await api.post(`/post/${postId}/ChangeCategory/${categoryId}`)
  } catch (error) {
    toast.error('修改分类失败！', { autoClose: 2000, position: 'top-center' })
  }
}

const changeSeminar = async (postId, seminarIds) => {
  try {
    const ids = Array.isArray(seminarIds) ? seminarIds.map(s => s.Id || s).join(',') : ''
    await api.post(`/post/${postId}/ChangeSeminar?sids=${ids}`)
  } catch (error) {
    toast.error('修改专题失败！', { autoClose: 2000, position: 'top-center' })
  }
}

const toggleDisableComment = async (row) => {
  try {
    const data = await api.post(`/post/${row.Id}/DisableComment`)
    toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
  } catch (error) {
    toast.error('操作失败！', { autoClose: 2000, position: 'top-center' })
  }
}

const toggleDisableCopy = async (row) => {
  try {
    const data = await api.post(`/post/${row.Id}/DisableCopy`)
    toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
  } catch (error) {
    toast.error('操作失败！', { autoClose: 2000, position: 'top-center' })
  }
}

const rssSwitch = async (id) => {
  try {
    const data = await api.post(`/post/${id}/rss-switch`)
    toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
  } catch (error) {
    toast.error('操作失败！', { autoClose: 2000, position: 'top-center' })
  }
}

const lockedSwitch = async (id) => {
  try {
    const data = await api.post(`/post/${id}/locked-switch`)
    toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
  } catch (error) {
    toast.error('操作失败！', { autoClose: 2000, position: 'top-center' })
  }
}

const nsfwSwitch = async (id) => {
  try {
    const data = await api.post(`/post/${id}/nsfw`)

    toast.success(data?.Message || '操作成功', { autoClose: 2000, position: 'top-center' })
  } catch (error) {
    toast.error('操作失败！', { autoClose: 2000, position: 'top-center' })
  }
}

// 图表相关方法
const initChart = () => {
  if (chartContainer.value && !chartInstance) {
    chartInstance = echarts.init(chartContainer.value)
    updateChart()
  }
}

const updateChart = async () => {
  if (!chartInstance) return
  try {
    const data = await api.get(`/post/records-chart?compare=${chartPeriod.value > 0}&period=${chartPeriod.value}`);
    const xSeries = []
    const yCountSeries = []
    const yUvSeries = []

    for (const series of data) {
      const x = []
      const yCount = []
      const yUV = []

      // 确保 series 是数组
      if (Array.isArray(series)) {
        for (const item of series) {
          if (item && item.Date) {
            x.push(new Date(Date.parse(item.Date)).toLocaleDateString())
            yCount.push(item.Count || 0)
            yUV.push(item.UV || 0)
          }
        }
      }

      xSeries.push(x)
      yCountSeries.push(yCount)
      yUvSeries.push(yUV)
    }

    const colors = ['#0091ee', '#ccc'];
    const option = {
      color: colors,
      tooltip: {
        trigger: 'none',
        axisPointer: {
          type: 'cross'
        }
      },
      legend: {},
      grid: {
        top: 70,
        bottom: 50
      },
      title: {
        left: 'center',
        text: '最近访问趋势'
      },
      xAxis: xSeries.map(function (item, index) {
        return {
          type: 'category',
          axisTick: {
            alignWithLabel: true
          },
          axisLine: {
            onZero: false,
            lineStyle: {
              color: colors[index]
            }
          },
          axisPointer: {
            label: {
              formatter: function (params) {
                if (params.seriesData && params.seriesData.length >= 2) {
                  return params.value + ' 访问量：' + params.seriesData[0].data + '，UV：' + params.seriesData[1].data
                }
                return params.value
              }
            }
          },
          data: item
        }
      }),
      yAxis: [
        {
          type: 'value'
        }
      ],
      series: yCountSeries.map(function (item, index) {
        return {
          type: 'line',
          //smooth: true,
          symbol: 'none',
          xAxisIndex: index,
          data: item,
          lineStyle: {
            type: index === 1 ? 'dashed' : ""
          },
          markPoint: {
            data: [
              { type: 'max', name: '最大值' },
              { type: 'min', name: '最小值' }
            ]
          },
          markLine: {
            data: [
              { type: 'average', name: '平均值' }
            ]
          }
        }
      }).concat(yUvSeries.map(function (item, index) {
        return {
          type: 'line',
          //smooth: true,
          symbol: 'none',
          xAxisIndex: index,
          areaStyle: {},
          data: item,
          lineStyle: {
            type: index === 1 ? 'dashed' : ""
          },
          markPoint: {
            data: [
              { type: 'average', name: '平均值' }
            ]
          },
          markLine: {
            data: [
              { type: 'average', name: '平均值' }
            ]
          }
        }
      }))
    };

    // 只有在有数据时才设置图表选项
    if (xSeries.length > 0 && yCountSeries.length > 0) {
      chartInstance.setOption(option)
    }
  } catch (error) {
    toast.error('加载图表数据失败！', { autoClose: 2000, position: 'top-center' })
  }
}

// 初始化EventSource监听统计数据
const initEventSource = () => {
  // 如果页面不可见，不初始化连接
  if (document.hidden) {
    return
  }

  eventSource = new EventSource(globalConfig.baseURL + '/post/Statistic')
  eventSource.onmessage = (event) => {
    try {
      aggregateData.value = JSON.parse(event.data)
    } catch (error) {
      toast.error('解析统计数据失败！', { autoClose: 2000, position: 'top-center' })
    }
  }

  eventSource.onerror = (error) => {
    toast.error('EventSource 连接错误！', { autoClose: 2000, position: 'top-center' })
    eventSource.close()
  }
}

// 关闭 EventSource 连接
const closeEventSources = () => {
  if (eventSource) {
    eventSource.close()
    eventSource = null
  }
}

// 页面可见性变化处理
const handleVisibilityChange = () => {
  if (document.hidden) {
    // 页面变为不可见，断开连接
    closeEventSources()
  } else {
    // 页面变为可见，恢复连接
    initEventSource()
  }
}

// 从本地存储恢复参数
const restoreParams = () => {
  const savedParams = localStorage.getItem('postlist-params')
  if (savedParams) {
    try {
      const params = JSON.parse(savedParams)
      searchKeyword.value = params.kw || ''
      useRegex.value = params.useRegex || false
      orderBy.value = params.orderby || 1
      pagination.value.page = params.page || 1
      if (params.cid) {
        const category = categoryOptions.value.find(c => c.Id === params.cid)
        if (category) {
          selectedCategory.value = category
        }
      }
    } catch (error) {
      toast.error('恢复参数失败！', { autoClose: 2000, position: 'top-center' })
    }
  }
}
watch(categoryOptions, (newVal) => {
  if (newVal.length > 0) {
    restoreParams()
    loadPageData()
  }
})
// 生命周期钩子
onMounted(async () => {
  await loadCategories()
  await loadSeminars()
  // 添加页面可见性变化监听
  document.addEventListener('visibilitychange', handleVisibilityChange)

  initEventSource()
  initChart()
  showColumns.value = (localStorage.getItem('showColumns') || 'PostDate').split(',')
  document.querySelector('.search-btn').scrollIntoView({ behavior: 'smooth', block: 'center' })
})

onBeforeUnmount(() => {
  // 移除页面可见性变化监听
  document.removeEventListener('visibilitychange', handleVisibilityChange)

  if (eventSource) {
    eventSource.close()
  }
  if (chartInstance) {
    chartInstance.dispose()
    chartInstance = null
  }
})
</script>
<style scoped>
.post-list-container {
  padding: 16px;
}

.controls-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 16px;
}

:deep(.warning-row) {
  background-color: #fff3cd !important;
}

.text-red {
  color: #f56565 !important;
}

@media (max-width: 768px) {
  .controls-row {
    flex-direction: column;
    align-items: stretch;
  }
}
</style>
