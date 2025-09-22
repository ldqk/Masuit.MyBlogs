<template>
<div class="advertisement-page">
  <!-- 顶部操作栏 -->
  <div class="text-h6"> 广告推广管理 </div>
  <div class="row q-mb-md">
    <div class="col-6">
      <q-select multiple v-model="showColumns" :options="columnOptions" outlined dense map-options emit-value use-chips @update:model-value="saveShowColumns">
        <template v-slot:prepend>
          <span style="font-size: 14px;">显示列:</span>
        </template>
      </q-select>
    </div>
    <div class="col-6 text-right">
      <div class="row items-center justify-end q-gutter-md">
        <!-- 搜索框 -->
        <div class="col-auto">
          <q-input v-model="searchKeyword" placeholder="全局搜索" dense outlined clearable @update:model-value="onSearchChange" debounce="500">
            <template #prepend>
              <q-icon name="search" />
            </template>
          </q-input>
        </div>
        <!-- 排序方式 -->
        <div class="col-auto">
          <q-select v-model="orderBy" :options="sortOptions" label="排序方式" dense outlined emit-value map-options style="width: 140px" @update:model-value="onOrderByChange" />
        </div>
        <!-- 操作按钮 -->
        <div class="col-auto">
          <q-btn-group>
            <q-btn color="primary" icon="refresh" label="刷新" @click="loadPageData" :loading="loading" />
            <q-btn color="positive" icon="add" label="添加广告" @click="showAddDialog" />
          </q-btn-group>
        </div>
      </div>
    </div>
  </div>
  <!-- 广告列表表格 -->
  <q-card flat bordered class="q-mb-md">
    <q-card-section>
      <div class="text-h6 q-mb-md">
        <q-icon name="campaign" class="q-mr-sm" /> 广告列表 <q-chip v-if="pagination.total > 0" color="primary" text-color="white" :label="`共 ${pagination.total} 条广告`" class="q-ml-sm" />
      </div>
      <!-- 广告为空提示 -->
      <div v-if="advertisements.length === 0 && !loading" class="text-center q-pa-xl text-grey-6">
        <q-icon name="campaign" size="64px" class="q-mb-md" />
        <div class="text-h6">暂无广告记录</div>
        <div class="text-body2 q-mt-sm">点击上方"添加广告"按钮创建第一个广告</div>
      </div>
      <!-- 广告表格 -->
      <div v-else>
        <vxe-table ref="tableRef" :data="advertisements" :loading="loading" border stripe :scroll-y="{ enabled: true }" :sort-config="{ remote: true }" @sort-change="onSortChange">
          <vxe-column field="Id" title="ID" width="80" sortable />
          <vxe-column field="Title" title="标题" min-width="300">
            <template #default="{ row }">
              <q-btn flat dense color="primary" :label="row.Title" @click="showDetailDialog(row)" />
            </template>
          </vxe-column>
          <vxe-column field="Url" title="推广地址" min-width="200">
            <template #default="{ row }">
              <a :href="row.Url" target="_blank" class="text-primary">{{ row.Url }}</a>
            </template>
          </vxe-column>
          <!-- 当前竞价列 -->
          <vxe-column v-if="showColumns.includes('Price')" field="Price" title="当前竞价" width="100">
            <template #default="{ row }"> ¥{{ row.Price || 0 }} </template>
          </vxe-column>
          <!-- 当月曝光量列 -->
          <vxe-column v-if="showColumns.includes('DisplayCount')" field="DisplayCount" title="当月曝光量" width="120" />
          <!-- 当月点击列 -->
          <vxe-column v-if="showColumns.includes('ViewCount')" field="ViewCount" title="当月点击" width="100" />
          <!-- 日均点击列 -->
          <vxe-column v-if="showColumns.includes('AverageViewCount')" field="AverageViewCount" title="日均点击" width="100">
            <template #default="{ row }"> {{ row.AverageViewCount?.toFixed(2) }} </template>
          </vxe-column>
          <!-- 点击率列 -->
          <vxe-column v-if="showColumns.includes('ClickRate')" field="ClickRate" title="点击率" width="100">
            <template #default="{ row }"> {{ row.ClickRate?.toFixed(2) }}% </template>
          </vxe-column>
          <!-- 分类列 -->
          <vxe-column v-if="showColumns.includes('CategoryIds')" field="CategoryIds" title="分类" width="260" show-overflow>
            <template #default="{ row }">
              <q-select use-chips outlined :model-value="getCategoryIdsArray(row.CategoryIds)" @update:model-value="(val) => changeAdvertisementCategory(row, val)" :options="categoryOptions" option-value="value" option-label="label" multiple dense borderless map-options emit-value placeholder="选择分类" />
            </template>
          </vxe-column>
          <!-- 创建时间列 -->
          <vxe-column v-if="showColumns.includes('CreateTime')" field="CreateTime" title="创建时间" width="120">
            <template #default="{ row }"> {{ formatDate(row.CreateTime) }} </template>
          </vxe-column>
          <!-- 修改时间列 -->
          <vxe-column v-if="showColumns.includes('UpdateTime')" field="UpdateTime" title="修改时间" width="120">
            <template #default="{ row }"> {{ formatDate(row.UpdateTime) }} </template>
          </vxe-column>
          <vxe-column field="Status" title="状态" width="80">
            <template #default="{ row }">
              <q-toggle :model-value="row.Status === 1" @update:model-value="changeStatus(row)" color="positive" />
            </template>
          </vxe-column>
          <vxe-column title="操作" width="160" fixed="right">
            <template #default="{ row }">
              <q-btn dense flat size="sm" color="info" icon="visibility" @click="showDetailDialog(row)" />
              <q-btn dense flat size="sm" color="primary" icon="edit" @click="editAdvertisement(row)" />
              <q-btn dense flat size="sm" color="secondary" icon="content_copy" @click="copyAdvertisement(row)" />
              <q-btn dense flat size="sm" color="warning" icon="schedule" @click="showDelayDialog(row)" />
              <q-btn dense flat size="sm" color="accent" icon="insights" @click="showInsight(row)" />
              <q-btn dense flat size="sm" color="negative" icon="delete">
                <q-popup-proxy transition-show="scale" transition-hide="scale">
                  <q-card>
                    <q-card-section class="row items-center">
                      <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
                      <div>
                        <div class="text-h6">确认删除</div>
                        <div class="text-subtitle2">确认删除广告【{{ row.Title }}】吗？</div>
                      </div>
                    </q-card-section>
                    <q-card-actions align="right">
                      <q-btn flat label="确认" color="negative" v-close-popup @click="removeAdvertisement(row)" />
                      <q-btn flat label="取消" color="primary" v-close-popup />
                    </q-card-actions>
                  </q-card>
                </q-popup-proxy>
              </q-btn>
            </template>
          </vxe-column>
        </vxe-table>
        <!-- 分页器 -->
        <div class="row justify-between items-center q-mt-md">
          <div class="col-auto">
            <span class="text-body2 text-grey-7"> 共 {{ pagination.total }} 条记录，当前显示第 {{ (pagination.page - 1) * pagination.itemsPerPage + 1 }} - {{ Math.min(pagination.page * pagination.itemsPerPage, pagination.total) }} 条 </span>
          </div>
          <div class="col-auto">
            <q-pagination v-model="pagination.page" :max="Math.ceil(pagination.total / pagination.itemsPerPage)" :max-pages="6" direction-links boundary-links color="primary" @update:model-value="onPageChange" />
          </div>
          <div class="col-auto">
            <q-select v-model="pagination.itemsPerPage" :options="pageSizeOptions" dense outlined label="每页显示" style="min-width: 100px" @update:model-value="onPageSizeChange" />
          </div>
        </div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 访问趋势图表 -->
  <q-card flat bordered>
    <q-card-section>
      <div class="text-h6 q-mb-md flex items-center">
        <q-icon name="trending_up" class="q-mr-sm" /> 广告访问趋势 <q-space />
        <div class="row items-center q-gutter-sm">
          <q-select v-model="chartPeriod" :options="chartPeriodOptions" dense emit-value map-options outlined style="min-width: 120px" @update:model-value="updateChart">
            <template #prepend>
              <span class="text-body2">对比最近：</span>
            </template>
          </q-select>
        </div>
      </div>
      <div ref="chartContainer" style="height: 400px; width: 100%;" v-show="chartContainer"></div>
      <div v-if="!chartContainer" class="text-center q-pa-xl">
        <q-spinner color="primary" size="3em" />
        <div class="q-mt-md text-grey-6">正在加载图表...</div>
      </div>
    </q-card-section>
  </q-card>
  <!-- 添加/编辑广告对话框 -->
  <q-dialog v-model="showEditDialog" persistent>
    <q-card style="min-width: 85vw; max-height: 90vh;" class="advertisement-edit-dialog">
      <q-card-section class="row items-center q-pb-none bg-primary text-white">
        <q-icon name="campaign" class="q-mr-sm" />
        <div class="text-h6">{{ isEditing ? '编辑广告' : '添加新广告' }}</div>
        <q-space />
        <q-btn icon="close" flat round dense text-color="white" @click="closeEditDialog" />
      </q-card-section>
      <q-card-section>
        <div class="row q-col-gutter-lg">
          <!-- 左侧：基本信息 -->
          <div class="col-12 col-md-7">
            <!-- 基本信息区域 -->
            <q-card flat bordered class="q-mb-md">
              <q-card-section class="bg-grey-1">
                <div class="text-subtitle1 text-primary">
                  <q-icon name="info" class="q-mr-xs" /> 基本信息
                </div>
              </q-card-section>
              <q-card-section>
                <q-input autogrow dense v-model="currentAd.Title" label="广告标题" outlined required :rules="[val => !!val || '请输入广告标题']">
                  <template #prepend>
                    <q-icon name="title" />
                  </template>
                </q-input>
                <div class="row q-gutter-md">
                  <q-input autogrow class="col" dense v-model="currentAd.Url" label="推广地址" outlined required :rules="[val => !!val || '请输入推广地址']">
                    <template #prepend>
                      <q-icon name="link" />
                    </template>
                  </q-input>
                  <q-input class="col" dense v-model.number="currentAd.Price" label="竞价金额" type="number" outlined :min="0" step="10" suffix="元">
                    <template #prepend>
                      <q-icon name="paid" />
                    </template>
                  </q-input>
                </div>
              </q-card-section>
            </q-card>
            <!-- 内容描述区域 -->
            <q-card flat bordered class="q-mb-md">
              <q-card-section class="bg-grey-1">
                <div class="text-subtitle1 text-primary">
                  <q-icon name="description" class="q-mr-xs" /> 广告内容
                </div>
              </q-card-section>
              <q-card-section>
                <v-md-editor v-model="currentAd.Description" height="250px" :toolbar="editorToolbar" placeholder="请输入广告描述内容..." />
              </q-card-section>
            </q-card>
            <!-- 分类和区域设置 -->
            <q-card flat bordered>
              <q-card-section class="bg-grey-1">
                <div class="text-subtitle1 text-primary">
                  <q-icon name="category" class="q-mr-xs" /> 分类与投放设置
                </div>
              </q-card-section>
              <q-card-section class="q-gutter-md">
                <q-select dense v-model="selectedCategories" :options="filteredCategoryOptions" label="广告分类" multiple chips use-chips use-input @filter="filterCategories" outlined emit-value map-options option-value="value" option-label="label">
                  <template #prepend>
                    <q-icon name="category" />
                  </template>
                </q-select>
                <div class="row">
                  <div class="col-auto" style="line-height: 40px;">推广区域类型：</div>
                  <q-option-group class="col-auto" v-model="selectedTypes" :options="typeOptions" type="checkbox" inline color="primary" />
                </div>
                <div class="row">
                  <div class="col">
                    <q-input dense v-model="currentAd.Regions" label="地域限制" outlined hint="多个地区用逗号分隔">
                      <template #prepend>
                        <q-icon name="public" />
                      </template>
                      <template #append>
                        <q-select dense v-model="currentAd.RegionMode" :options="regionOptions" label="地域模式" emit-value map-options style="width: 80px;" />
                      </template>
                    </q-input>
                  </div>
                </div>
                <q-input v-model="currentAd.ExpireTime" label="有效期至" outlined readonly>
                  <template #append>
                    <q-icon name="event" class="cursor-pointer">
                      <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                        <q-date v-model="currentAd.ExpireTime" mask="YYYY-MM-DD HH:mm:ss">
                          <div class="row items-center justify-end">
                            <q-btn v-close-popup label="确定" color="primary" flat />
                          </div>
                        </q-date>
                      </q-popup-proxy>
                    </q-icon>
                  </template>
                </q-input>
              </q-card-section>
            </q-card>
          </div>
          <!-- 右侧：图片管理 -->
          <div class="col-12 col-md-5">
            <q-card flat bordered class="full-height">
              <q-card-section class="bg-grey-1">
                <div class="text-subtitle1 text-primary">
                  <q-icon name="image" class="q-mr-xs" /> 广告素材
                </div>
              </q-card-section>
              <q-card-section class="q-gutter-lg">
                <!-- 主广告图片 -->
                <div>
                  <div class="text-body2 text-grey-7 q-mb-sm">主广告图片</div>
                  <q-input v-model="currentAd.ImageUrl" label="图片地址" outlined readonly>
                    <template #append>
                      <q-btn icon="upload" dense flat color="primary" @click="uploadImage('ImageUrl')" />
                      <q-btn v-if="currentAd.ImageUrl" icon="clear" dense flat color="negative" @click="clearImage('ImageUrl')" />
                    </template>
                  </q-input>
                  <div v-if="currentAd.ImageUrl" class="q-mt-md text-center">
                    <q-img :src="currentAd.ImageUrl?.startsWith('http') ? currentAd.ImageUrl : globalConfig.baseURL + currentAd.ImageUrl" :ratio="16 / 9" style="max-width: 300px; cursor: pointer;" class="shadow-3" @click="previewImage(currentAd.ImageUrl)" />
                  </div>
                </div>
                <q-separator />
                <!-- 缩略图 -->
                <div>
                  <div class="text-body2 text-grey-7 q-mb-sm">缩略图</div>
                  <q-input v-model="currentAd.ThumbImgUrl" label="缩略图地址" outlined readonly>
                    <template #append>
                      <q-btn icon="upload" dense flat color="primary" @click="uploadImage('ThumbImgUrl')" />
                      <q-btn v-if="currentAd.ThumbImgUrl" icon="clear" dense flat color="negative" @click="clearImage('ThumbImgUrl')" />
                    </template>
                  </q-input>
                  <div v-if="currentAd.ThumbImgUrl" class="q-mt-md text-center">
                    <q-img :src="currentAd.ThumbImgUrl?.startsWith('http') ? currentAd.ThumbImgUrl : globalConfig.baseURL + currentAd.ThumbImgUrl" :ratio="1" style="width: 150px; cursor: pointer;" class="shadow-3" @click="previewImage(currentAd.ThumbImgUrl)" />
                  </div>
                </div>
              </q-card-section>
            </q-card>
          </div>
        </div>
      </q-card-section>
      <q-card-actions align="right" class="bg-grey-1">
        <q-btn flat label="取消" @click="closeEditDialog" />
        <q-btn color="primary" :label="isEditing ? '保存修改' : '确认添加'" @click="saveAdvertisement" :loading="saving" :disable="!currentAd.Title || !currentAd.Url" unelevated />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <!-- 广告详情对话框 -->
  <q-dialog v-model="showDetailDialogFlag">
    <q-card style="min-width: 900px; max-width: 95vw; max-height: 90vh;" class="advertisement-detail-dialog">
      <q-card-section class="row items-center q-pb-none bg-primary text-white">
        <q-icon name="visibility" class="q-mr-sm" />
        <div class="text-h6">广告详情</div>
        <q-space />
        <q-btn icon="close" flat round dense text-color="white" @click="showDetailDialogFlag = false" />
      </q-card-section>
      <q-card-section v-if="currentDetailAd">
        <div class="row q-col-gutter-lg">
          <!-- 左侧：基本信息和内容 -->
          <div class="col-12 col-md-7">
            <!-- 基本信息区域 -->
            <q-card flat bordered class="q-mb-md">
              <q-card-section class="bg-grey-1">
                <div class="text-subtitle1 text-primary">
                  <q-icon name="info" class="q-mr-xs" /> 基本信息
                </div>
              </q-card-section>
              <q-card-section class="q-gutter-md">
                <div class="row items-center">
                  <q-icon name="tag" class="q-mr-sm text-grey-6" />
                  <span class="text-body2 text-grey-7 q-mr-sm">ID:</span>
                  <q-chip size="sm" color="blue-grey-2" text-color="dark"> {{ currentDetailAd.Id }} </q-chip>
                </div>
                <div class="row">
                  <span class="text-body2 text-grey-7 q-mb-xs"><q-icon name="title" class="q-mt-sm text-grey-6" /> 广告标题：{{ currentDetailAd.Title }}</span>
                </div>
                <div class="row items-start">
                  <span class="text-body2 text-grey-7 q-mb-xs"><q-icon name="link" class="q-mt-sm text-grey-6" /> 推广地址：</span>
                  <a :href="currentDetailAd.Url" target="_blank" class="text-primary text-weight-medium text-decoration-none"> {{ currentDetailAd.Url }} <q-icon name="open_in_new" size="xs" class="q-ml-xs" />
                  </a>
                </div>
                <div class="row">
                  <div class="row items-start">
                    <div class="text-body2 text-grey-7"><q-icon name="paid" class="text-grey-6" /> 竞价金额：<span class="text-weight-bold text-green-7"> ¥{{ currentDetailAd.Price || 0 }} </span></div>
                  </div>
                </div>
              </q-card-section>
            </q-card>
            <!-- 广告内容区域 -->
            <q-card flat bordered class="q-mb-md" v-if="currentDetailAd.Description">
              <q-card-section class="bg-grey-1">
                <div class="text-subtitle1 text-primary">
                  <q-icon name="description" class="q-mr-xs" /> 广告描述
                </div>
              </q-card-section>
              <q-card-section>
                <div v-html="currentDetailAd.Description" />
              </q-card-section>
            </q-card>
            <!-- 分类和统计信息 -->
            <q-card flat bordered>
              <q-card-section class="bg-grey-1">
                <div class="text-subtitle1 text-primary">
                  <q-icon name="analytics" class="q-mr-xs" /> 分类与统计
                </div>
              </q-card-section>
              <q-card-section class="q-gutter-md">
                <div v-if="currentDetailAd.CategoryIds">
                  <q-icon name="category" class="q-mr-xs" /> 广告分类： <q-chip v-for="categoryName in getCategoryNames(currentDetailAd.CategoryIds)" :key="categoryName" color="primary" text-color="white" class="q-ma-xs"> {{ categoryName }} </q-chip>
                </div>
                <q-chip v-else color="grey-4" text-color="dark">未分类</q-chip>
                <div class="row q-gutter-md">
                  <div class="col">
                    <q-linear-progress :value="currentDetailAd.ClickRate || 0" color="positive" size="8px" class="q-mb-xs" />
                    <div class="text-caption text-grey-7">点击率</div>
                    <div class="text-weight-bold">{{ ((currentDetailAd.ClickRate || 0) * 100).toFixed(2) }}%</div>
                  </div>
                  <div class="col text-center">
                    <q-circular-progress :value="Math.min(currentDetailAd.DisplayCount || 0, 10000)" :max="10000" size="50px" :thickness="0.2" color="blue" track-color="grey-3" class="q-mb-xs" />
                    <div class="text-caption text-grey-7">展示次数</div>
                    <div class="text-weight-bold">{{ currentDetailAd.DisplayCount || 0 }}</div>
                  </div>
                  <div class="col text-right">
                    <div class="text-h5 text-weight-bold text-orange-7">{{ currentDetailAd.ViewCount || 0 }}</div>
                    <div class="text-caption text-grey-7">点击次数</div>
                  </div>
                </div>
              </q-card-section>
            </q-card>
          </div>
          <!-- 右侧：图片和状态信息 -->
          <div class="col-12 col-md-5">
            <q-card flat bordered class="full-height">
              <q-card-section class="bg-grey-1">
                <div class="text-subtitle1 text-primary">
                  <q-icon name="image" class="q-mr-xs" /> 广告素材
                </div>
              </q-card-section>
              <q-card-section class="q-gutter-lg">
                <!-- 主广告图片 -->
                <div>
                  <div class="text-body2 text-grey-7 q-mb-sm">
                    <q-icon name="wallpaper" class="q-mr-xs" /> 主广告图片
                  </div>
                  <div v-if="currentDetailAd.ImageUrl" class="image-preview-container">
                    <q-img :src="currentDetailAd.ImageUrl?.startsWith('http') ? currentDetailAd.ImageUrl : globalConfig.baseURL + currentDetailAd.ImageUrl" :ratio="16 / 9" class="preview-image shadow-3" @click="previewImage(currentDetailAd.ImageUrl)">
                      <template v-slot:error>
                        <div class="absolute-full flex flex-center bg-negative text-white">
                          <div class="text-center">
                            <q-icon name="broken_image" size="32px" />
                            <div class="text-caption q-mt-sm">加载失败</div>
                          </div>
                        </div>
                      </template>
                      <template v-slot:loading>
                        <div class="absolute-full flex flex-center">
                          <q-spinner color="primary" size="32px" />
                        </div>
                      </template>
                      <div class="absolute-top-right q-pa-sm">
                        <q-btn icon="visibility" size="sm" dense round color="white" text-color="primary" @click.stop="previewImage(currentDetailAd.ImageUrl)" />
                      </div>
                    </q-img>
                  </div>
                  <div v-else class="no-image-placeholder">
                    <div class="text-center q-pa-xl text-grey-4">
                      <q-icon name="image_not_supported" size="48px" />
                      <div class="q-mt-md">暂无图片</div>
                    </div>
                  </div>
                </div>
                <q-separator />
                <!-- 缩略图 -->
                <div>
                  <div class="text-body2 text-grey-7 q-mb-sm">
                    <q-icon name="crop_original" class="q-mr-xs" /> 缩略图
                  </div>
                  <div v-if="currentDetailAd.ThumbImgUrl" class="text-center">
                    <q-img :src="currentDetailAd.ThumbImgUrl?.startsWith('http') ? currentDetailAd.ThumbImgUrl : globalConfig.baseURL + currentDetailAd.ThumbImgUrl" :ratio="1" style="width: 120px; margin: 0 auto;" class="preview-image shadow-3" @click="previewImage(currentDetailAd.ThumbImgUrl)">
                      <template v-slot:error>
                        <div class="absolute-full flex flex-center bg-negative text-white">
                          <div class="text-center">
                            <q-icon name="broken_image" size="24px" />
                            <div class="text-caption">加载失败</div>
                          </div>
                        </div>
                      </template>
                      <div class="absolute-top-right q-pa-xs">
                        <q-btn icon="visibility" size="xs" dense round color="white" text-color="primary" @click.stop="previewImage(currentDetailAd.ThumbImgUrl)" />
                      </div>
                    </q-img>
                  </div>
                  <div v-else class="no-image-placeholder" style="height: 120px;">
                    <div class="text-center q-pa-lg text-grey-4">
                      <q-icon name="crop_original" size="32px" />
                      <div class="q-mt-sm">暂无缩略图</div>
                    </div>
                  </div>
                </div>
                <q-separator />
                <!-- 状态和时间信息 -->
                <div>
                  <div class="text-body2 text-grey-7 q-mb-sm">
                    <q-icon name="schedule" class="q-mr-xs" /> 时间信息
                  </div>
                  <div class="q-gutter-sm">
                    <div class="row items-center">
                      <q-icon name="add_circle" size="sm" class="q-mr-xs text-green-6" />
                      <span class="text-caption text-grey-7">创建时间：</span>
                      <span class="text-body2">{{ formatDate(currentDetailAd.CreateTime) }}</span>
                    </div>
                    <div class="row items-center">
                      <q-icon name="update" size="sm" class="q-mr-xs text-blue-6" />
                      <span class="text-caption text-grey-7">更新时间：</span>
                      <span class="text-body2">{{ formatDate(currentDetailAd.UpdateTime) }}</span>
                    </div>
                    <div class="row items-center">
                      <q-icon name="event" size="sm" class="q-mr-xs text-orange-6" />
                      <span class="text-caption text-grey-7">过期时间：</span>
                      <span class="text-body2">{{ formatDate(currentDetailAd.ExpireTime) }}</span>
                    </div>
                  </div>
                </div>
              </q-card-section>
            </q-card>
          </div>
        </div>
      </q-card-section>
      <q-card-actions align="right" class="bg-grey-1">
        <q-btn flat label="编辑" color="primary" icon="edit" @click="editFromDetail" class="q-px-lg" />
        <q-btn flat label="删除" color="negative" icon="delete" class="q-px-lg">
          <q-popup-proxy transition-show="scale" transition-hide="scale">
            <q-card>
              <q-card-section class="row items-center">
                <div class="text-h6">确认删除</div>
                <div class="text-subtitle2">确认删除广告【{{ currentDetailAd.Title }}】吗？</div>
              </q-card-section>
              <q-card-actions align="right">
                <q-btn flat label="确认" color="negative" v-close-popup @click="removeAdvertisement(currentDetailAd)" />
                <q-btn flat label="取消" color="primary" v-close-popup />
              </q-card-actions>
            </q-card>
          </q-popup-proxy>
        </q-btn>
        <q-btn flat label="关闭" color="grey" @click="showDetailDialogFlag = false" class="q-px-lg" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <!-- 延期对话框 -->
  <q-dialog v-model="showDelayDialogFlag">
    <q-card style="min-width: 400px;">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">延期广告</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="showDelayDialogFlag = false" />
      </q-card-section>
      <q-card-section v-if="currentAd">
        <div class="q-gutter-md">
          <div class="text-body1">广告：{{ currentAd.Title }}</div>
          <div class="text-body2">当前有效期延期至：</div>
          <q-input v-model="currentAd.ExpireTime" label="过期时间" dense outlined readonly>
            <template #append>
              <q-icon name="event" class="cursor-pointer">
                <q-popup-proxy cover transition-show="scale" transition-hide="scale">
                  <q-date v-model="currentAd.ExpireTime" mask="YYYY-MM-DD HH:mm:ss">
                    <div class="row items-center justify-end">
                      <q-btn v-close-popup label="确定" color="primary" flat />
                    </div>
                  </q-date>
                </q-popup-proxy>
              </q-icon>
            </template>
          </q-input>
        </div>
      </q-card-section>
      <q-card-actions align="right">
        <q-btn flat label="取消" @click="showDelayDialogFlag = false" />
        <q-btn color="primary" label="确定延期" @click="delayAdvertisement" :loading="saving" />
      </q-card-actions>
    </q-card>
  </q-dialog>
  <!-- 图片上传对话框 -->
  <q-dialog v-model="showUploadDialog">
    <q-card style="min-width: 400px;">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">上传图片</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="showUploadDialog = false" />
      </q-card-section>
      <q-card-section>
        <q-file v-model="uploadFile" label="选择图片" outlined accept="image/*" @update:model-value="handleFileUpload">
          <template #prepend>
            <q-icon name="attach_file" />
          </template>
        </q-file>
      </q-card-section>
    </q-card>
  </q-dialog>
  <!-- 数据洞察对话框 -->
  <q-dialog v-model="showInsightDialogFlag" maximized>
    <q-card>
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">广告洞察分析</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="showInsightDialogFlag = false" />
      </q-card-section>
      <q-card-section class="q-pa-none" style="height: calc(100vh - 100px)">
        <iframe v-if="insightUrl" :src="insightUrl" width="100%" height="100%" frameborder="0"></iframe>
      </q-card-section>
    </q-card>
  </q-dialog>
  <!-- 图片预览对话框 -->
  <q-dialog v-model="showImagePreview">
    <q-card style="min-width: 50vw;min-height: 50vh; max-width: 90vw; max-height: 90vh;">
      <q-card-section class="row items-center q-pb-none">
        <div class="text-h6">图片预览</div>
        <q-space />
        <q-btn icon="close" flat round dense @click="showImagePreview = false" />
      </q-card-section>
      <q-card-section class="q-pa-md">
        <div class="text-center">
          <q-img :src="previewImageUrl?.startsWith('http') ? previewImageUrl : globalConfig.baseURL + previewImageUrl" fit="contain" style="max-width: 100%; max-height: 73vh;">
            <template v-slot:error>
              <div class="absolute-full flex flex-center bg-grey-3 text-grey-7">
                <div class="text-center">
                  <q-icon name="broken_image" size="48px" />
                  <div class="text-h6 q-mt-md">图片加载失败</div>
                </div>
              </div>
            </template>
            <template v-slot:loading>
              <div class="absolute-full flex flex-center">
                <q-spinner color="primary" size="48px" />
              </div>
            </template>
          </q-img>
        </div>
      </q-card-section>
      <q-card-actions align="center">
        <q-btn flat icon="open_in_new" label="在新窗口打开" @click="openImageInNewTab" />
      </q-card-actions>
    </q-card>
  </q-dialog>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { toast } from 'vue3-toastify'
import api from '@/axios/AxiosConfig'
import dayjs from 'dayjs'
import globalConfig from '@/config'
import * as echarts from 'echarts'

// 定义接口类型
interface Advertisement {
  Id?: number
  Title: string
  Url: string
  Description: string
  ImageUrl: string
  ThumbImgUrl: string
  Price: number
  Weight: number
  Types: string
  CategoryIds: string
  RegionMode: number
  Regions: string
  ExpireTime: string
  Status: number
  DisplayCount: number
  ViewCount: number
  AverageViewCount: number
  ClickRate: number
  CreateTime: string
  UpdateTime: string
}

interface ApiResponse {
  Success: boolean
  Message: string
  Data?: any
  TotalCount?: number
}

interface Category {
  Id: number
  Name: string
  Children?: Category[]
}

// 响应式数据
const advertisements = ref<Advertisement[]>([])
const loading = ref(false)
const saving = ref(false)

// 搜索和排序
const searchKeyword = ref('')
const orderBy = ref(1)
let searchTimeout: NodeJS.Timeout

// 显示列配置
const showColumns = ref<string[]>([])
const columnOptions = [
  { label: "当前竞价", value: 'Price' },
  { label: "当月曝光量", value: 'DisplayCount' },
  { label: "当月点击", value: 'ViewCount' },
  { label: "日均点击", value: 'AverageViewCount' },
  { label: "点击率", value: 'ClickRate' },
  { label: "分类", value: 'CategoryIds' },
  { label: "创建时间", value: 'CreateTime' },
  { label: "修改时间", value: 'UpdateTime' }
]

// 分页数据
const pagination = ref({
  page: 1,
  itemsPerPage: 10,
  total: 0
})

// 对话框状态
const showEditDialog = ref(false)
const showDetailDialogFlag = ref(false)
const showDelayDialogFlag = ref(false)
const showUploadDialog = ref(false)
const showInsightDialogFlag = ref(false)
const showImagePreview = ref(false)
const isEditing = ref(false)

// 图片预览相关
const previewImageUrl = ref('')

// 当前操作的广告数据
const currentAd = ref<Advertisement>({
  Title: '',
  Url: '',
  Description: '',
  ImageUrl: '',
  ThumbImgUrl: '',
  Price: 0,
  Weight: 1,
  Types: '',
  CategoryIds: '',
  RegionMode: 0,
  Regions: '',
  ExpireTime: '2049-12-31 23:59:59',
  Status: 1,
  DisplayCount: 0,
  ViewCount: 0,
  AverageViewCount: 0,
  ClickRate: 0,
  CreateTime: '',
  UpdateTime: ''
})

const currentDetailAd = ref<Advertisement | null>(null)
const insightUrl = ref('')

// 选择相关
const selectedTypes = ref<string[]>([])
const selectedCategories = ref<number[]>([])
const categoryTree = ref<Category[]>([])
const categoryOptions = ref<{ label: string; value: number }[]>([])
const filteredCategoryOptions = ref<{ label: string; value: number }[]>([])

// 上传相关
const uploadFile = ref<File | null>(null)
let currentUploadField = ''

// 选项数据
const sortOptions = [
  { label: '默认', value: 0 },
  { label: '竞价', value: 1 },
  { label: '曝光量', value: 2 },
  { label: '点击量', value: 3 },
  { label: '日均点击', value: 4 },
  { label: '点击率', value: 5 }
]

const pageSizeOptions = [10, 15, 20, 30, 50, 100, 200]

const typeOptions = [
  { label: '轮播图', value: '1' },
  { label: '列表项', value: '2' },
  { label: '边栏', value: '3' },
  { label: '内页', value: '4' }
]

const regionOptions = [
  { label: '不限', value: 0 },
  { label: '以内', value: 1 },
  { label: '以外', value: 2 }
]

// 编辑器工具栏配置
const editorToolbar = {
  toolbars: [
    'bold',
    'italic',
    'strikethrough',
    'quote',
    'ul',
    'ol',
    'link',
    'h',
    'code',
    'save'
  ]
}

// 表格引用
const tableRef = ref(null)

// 图表相关
const chartContainer = ref<HTMLElement | null>(null)
const chartPeriod = ref(30)
let chartInstance: echarts.ECharts | null = null

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

// 方法
const formatDate = (dateStr: string): string => {
  return dayjs(dateStr).format('YYYY-MM-DD')
}

// 获取分类名称
const getCategoryNames = (categoryIds: string): string[] => {
  if (!categoryIds) return []

  const ids = categoryIds.split(',').map(id => parseInt(id.trim()))
  return ids.map(id => {
    const category = categoryOptions.value.find(opt => opt.value === id)
    return category ? category.label : `未知分类(${id})`
  }).filter(name => name !== undefined)
}

// 将字符串格式的分类ID转换为数组格式（用于q-select）
const getCategoryIdsArray = (categoryIds: string): number[] => {
  if (!categoryIds) return []
  return categoryIds.split(',').map(id => parseInt(id.trim())).filter(id => !isNaN(id))
}

// 将数组格式的分类ID转换为字符串格式（用于保存）
const getCategoryIdsString = (categoryIds: number[]): string => {
  if (!Array.isArray(categoryIds)) return ''
  return categoryIds.join(',')
}

// 搜索变化处理
const onSearchChange = () => {
  if (searchTimeout) clearTimeout(searchTimeout)
  searchTimeout = setTimeout(() => {
    pagination.value.page = 1
    loadPageData()
  }, 500)
}

// 排序变化处理
const onOrderByChange = () => {
  pagination.value.page = 1
  loadPageData()
}

// 分页变化处理
const onPageChange = () => {
  loadPageData()
}

const onPageSizeChange = () => {
  pagination.value.page = 1
  loadPageData()
}

// 表格排序处理
const onSortChange = ({ field, order }: { field: string; order: string }) => {
  // 处理表格排序逻辑
  loadPageData()
}

// 保存显示列配置
const saveShowColumns = () => {
  localStorage.setItem('advertisement-showColumns', showColumns.value.join(','))
}

// 加载分页数据
const loadPageData = async () => {
  loading.value = true
  try {
    const params = {
      page: pagination.value.page,
      size: pagination.value.itemsPerPage,
      kw: searchKeyword.value || '',
      orderby: orderBy.value || 0
    }

    const response = await api.get('/partner/getpagedata', { params }) as ApiResponse
    if (response?.TotalCount !== undefined) {
      advertisements.value = response.Data || []
      pagination.value.total = response.TotalCount
    } else {
      advertisements.value = []
      pagination.value.total = 0
    }
  } catch (error) {
    toast.error('加载广告列表失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error loading advertisements:', error)
    advertisements.value = []
    pagination.value.total = 0
  } finally {
    loading.value = false
  }
}

// 加载分类数据
const loadCategories = async () => {
  try {
    const response = await api.get('/category/getcategories') as ApiResponse
    if (response?.Data) {
      categoryTree.value = response.Data
      // 将树形结构转换为平铺的选项列表
      categoryOptions.value = flattenCategories(response.Data)
      filteredCategoryOptions.value = [...categoryOptions.value]
    }
  } catch (error) {
    console.error('Error loading categories:', error)
  }
}

// 将分类树形结构转换为平铺列表
const flattenCategories = (categories: Category[], prefix = ''): { label: string; value: number }[] => {
  const result: { label: string; value: number }[] = []

  categories.forEach(category => {
    const label = prefix ? `${prefix} > ${category.Name}` : category.Name
    result.push({ label, value: category.Id })

    if (category.Children && category.Children.length > 0) {
      result.push(...flattenCategories(category.Children, label))
    }
  })

  return result
}

// 分类筛选方法
const filterCategories = (val: string, update: (fn: () => void) => void) => {
  update(() => {
    if (val === '') {
      filteredCategoryOptions.value = categoryOptions.value
    } else {
      const needle = val.toLowerCase()
      filteredCategoryOptions.value = categoryOptions.value.filter(
        option => option.label.toLowerCase().includes(needle)
      )
    }
  })
}

// 显示添加对话框
const showAddDialog = () => {
  currentAd.value = {
    Title: '',
    Url: '',
    Description: '',
    ImageUrl: '',
    ThumbImgUrl: '',
    Price: 0,
    Weight: 1,
    Types: '',
    CategoryIds: '',
    RegionMode: 0,
    Regions: '',
    ExpireTime: '2049-12-31 23:59:59',
    Status: 1,
    DisplayCount: 0,
    ViewCount: 0,
    AverageViewCount: 0,
    ClickRate: 0,
    CreateTime: '',
    UpdateTime: ''
  }
  selectedTypes.value = []
  selectedCategories.value = []
  isEditing.value = false
  showEditDialog.value = true
}

// 编辑广告
const editAdvertisement = (ad: Advertisement) => {
  currentAd.value = { ...ad }
  selectedTypes.value = ad.Types ? ad.Types.split(',') : []
  selectedCategories.value = ad.CategoryIds ? ad.CategoryIds.split(',').map(id => parseInt(id)) : []
  isEditing.value = true
  showEditDialog.value = true
}

// 复制广告
const copyAdvertisement = (ad: Advertisement) => {
  currentAd.value = { ...ad }
  delete currentAd.value.Id
  selectedTypes.value = ad.Types ? ad.Types.split(',') : []
  selectedCategories.value = ad.CategoryIds ? ad.CategoryIds.split(',').map(id => parseInt(id)) : []
  isEditing.value = false
  showEditDialog.value = true
}

// 关闭编辑对话框
const closeEditDialog = () => {
  showEditDialog.value = false
}

// 保存广告
const saveAdvertisement = async () => {
  if (!currentAd.value.Title || !currentAd.value.Url) {
    toast.warning('请填写完整的广告信息', { autoClose: 2000, position: 'top-center' })
    return
  }

  // 组装数据
  currentAd.value.Types = selectedTypes.value.join(',')
  currentAd.value.CategoryIds = selectedCategories.value.join(',')

  saving.value = true
  try {
    const response = await api.post('/partner/save', currentAd.value) as ApiResponse
    if (response?.Success !== false) {
      toast.success(response?.Message || '保存成功', { autoClose: 2000, position: 'top-center' })
      closeEditDialog()
      loadPageData()
    } else {
      toast.error(response?.Message || '保存失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('保存失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error saving advertisement:', error)
  } finally {
    saving.value = false
  }
}

// 删除广告
const removeAdvertisement = async (ad: Advertisement) => {
  try {
    const response = await api.post('/partner/Delete/' + ad.Id) as ApiResponse
    if (response?.Success !== false) {
      toast.success(response?.Message || '删除成功', { autoClose: 2000, position: 'top-center' })
      loadPageData()
    } else {
      toast.error(response?.Message || '删除失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('删除失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error removing advertisement:', error)
  }
}

// 更改状态
const changeStatus = async (ad: Advertisement) => {
  try {
    const response = await api.post(`/partner/ChangeState/${ad.Id}`) as ApiResponse
    if (response?.Success !== false) {
      toast.success(response?.Message || '状态更新成功', { autoClose: 2000, position: 'top-center' })
      ad.Status = ad.Status === 1 ? 0 : 1
    } else {
      toast.error(response?.Message || '状态更新失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('状态更新失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error changing status:', error)
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
    const data = await api.get(`/partner/records-chart?compare=${chartPeriod.value > 0}&period=${chartPeriod.value}`)
    const xSeries: string[][] = []
    const ySeries: number[][] = []

    for (const series of data) {
      const x: string[] = []
      const y: number[] = []
      for (const item of series) {
        x.push(new Date(Date.parse(item.Date)).toLocaleDateString());
        y.push(item.Count);
      }
      xSeries.push(x)
      ySeries.push(y)
    }
    console.log(xSeries, ySeries);
    const colors = ['#0091ee', '#ccc']
    var option = {
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
                return params.value + (params.seriesData.length ? '：' + params.seriesData[0].data : '');
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
      series: ySeries.map(function (item, index) {
        return {
          type: 'line',
          symbol: 'none',
          xAxisIndex: index,
          areaStyle: {},
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
      })
    };

    chartInstance.setOption(option)
  } catch (error) {
    console.error('Error updating chart:', error)
    toast.error('加载图表数据失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 更改广告分类
const changeAdvertisementCategory = async (advertisement, categoryIds: number[]) => {
  const ids = Array.isArray(categoryIds) ? categoryIds.join(',') : ''
  advertisement.CategoryIds = ids
  const response = await api.post('/partner/save', advertisement) as ApiResponse
  if (response?.Success === false) {
    toast.error(response?.Message || '保存失败', { autoClose: 2000, position: 'top-center' })
  }
}

// 显示详情对话框
const showDetailDialog = (ad: Advertisement) => {
  currentDetailAd.value = { ...ad }
  showDetailDialogFlag.value = true
}

// 从详情编辑
const editFromDetail = () => {
  showDetailDialogFlag.value = false
  if (currentDetailAd.value) {
    editAdvertisement(currentDetailAd.value)
  }
}

// 从详情删除
const removeFromDetail = () => {
  showDetailDialogFlag.value = false
  if (currentDetailAd.value) {
    removeAdvertisement(currentDetailAd.value)
  }
}

// 显示延期对话框
const showDelayDialog = (ad: Advertisement) => {
  currentAd.value = { ...ad }
  currentAd.value.ExpireTime = dayjs(ad.ExpireTime).format('YYYY-MM-DD HH:mm:ss')
  showDelayDialogFlag.value = true
}

// 延期广告
const delayAdvertisement = async () => {
  if (!currentAd.value) return

  saving.value = true
  try {
    const response = await api.post('/partner/save', currentAd.value) as ApiResponse

    if (response?.Success !== false) {
      toast.success(response?.Message || '延期成功', { autoClose: 2000, position: 'top-center' })
      showDelayDialogFlag.value = false
      loadPageData()
    } else {
      toast.error(response?.Message || '延期失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('延期失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error delaying advertisement:', error)
  } finally {
    saving.value = false
  }
}

// 上传图片
const uploadImage = (field: string) => {
  currentUploadField = field
  showUploadDialog.value = true
}

// 处理文件上传
const handleFileUpload = async (file: File | null) => {
  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const response = await api.post('/Upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    }) as ApiResponse

    if (response?.Data) {
      (currentAd.value as any)[currentUploadField] = response.Data
      toast.success('图片上传成功', { autoClose: 2000, position: 'top-center' })
      showUploadDialog.value = false
    } else {
      toast.error('图片上传失败', { autoClose: 2000, position: 'top-center' })
    }
  } catch (error) {
    toast.error('图片上传失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error uploading image:', error)
  }
}

// 清除图片
const clearImage = (field: 'ImageUrl' | 'ThumbImgUrl') => {
  currentAd.value[field] = ''
}

// 预览图片
const previewImage = (imageUrl: string) => {
  if (!imageUrl) return
  previewImageUrl.value = imageUrl
  showImagePreview.value = true
}

// 在新窗口打开图片
const openImageInNewTab = () => {
  if (previewImageUrl.value) {
    window.open(globalConfig.baseURL + previewImageUrl.value, '_blank')
  }
}

// 显示洞察
const showInsight = (ad: Advertisement) => {
  insightUrl.value = globalConfig.baseURL + `/partner/${ad.Id}/insight`
  showInsightDialogFlag.value = true
}

// 生命周期钩子
onMounted(async () => {
  // 初始化显示列配置
  const savedColumns = localStorage.getItem('advertisement-showColumns')
  if (savedColumns) {
    showColumns.value = savedColumns.split(',').filter(col => col)
  } else {
    // 默认显示所有列
    showColumns.value = columnOptions.map(opt => opt.value)
  }

  await loadCategories()
  await loadPageData()

  // 初始化图表
  setTimeout(() => {
    initChart()
  }, 100)
})

onBeforeUnmount(() => {
  if (chartInstance) {
    chartInstance.dispose()
    chartInstance = null
  }
})
</script>
<style scoped lang="scss">
.advertisement-page {
  padding: 20px;
}

.description-content {
  max-width: 100%;
  overflow: auto;

  :deep(.v-md-editor-preview) {
    background: transparent;
    border: none;
    padding: 0;

    .v-md-editor-preview__content {
      font-size: 14px;
      line-height: 1.5;
    }
  }
}

.image-preview-container {
  display: inline-block;
  position: relative;

  &:hover {
    .q-img {
      transform: scale(1.02);
      transition: transform 0.2s ease-in-out;
    }
  }
}

// 编辑表单样式优化
.advertisement-edit-dialog {
  .q-card__section {
    .text-subtitle1 {
      font-weight: 600;
    }
  }

  .preview-image {
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.3s ease;

    &:hover {
      transform: scale(1.05);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
    }
  }

  .upload-placeholder {
    border: 2px dashed #e0e0e0;
    border-radius: 8px;
    background: #fafafa;
    transition: all 0.3s ease;
    cursor: pointer;

    &:hover {
      border-color: #1976d2;
      background: #f5f5f5;
    }
  }

  .q-card {
    &.flat {
      .q-card__section {
        &.bg-grey-1 {
          border-bottom: 1px solid #e0e0e0;
        }
      }
    }
  }

  // 表单字段样式
  .q-field {
    .q-field__prepend {
      .q-icon {
        color: #666;
      }
    }

    &.q-field--focused {
      .q-field__prepend .q-icon {
        color: #1976d2;
      }
    }
  }

  // 分组标题
  .text-primary {
    color: #1976d2 !important;
  }

  // 按钮组样式
  .q-card-actions {
    .q-btn {
      min-width: 100px;
      font-weight: 500;
    }
  }
}

// 详情对话框样式优化
.advertisement-detail-dialog {
  .q-card__section {
    &.bg-grey-1 {
      .text-subtitle1 {
        font-weight: 600;
      }
    }
  }

  .preview-image {
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.3s ease;

    &:hover {
      transform: scale(1.02);
      box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
    }
  }

  .no-image-placeholder {
    border: 2px dashed #e0e0e0;
    border-radius: 8px;
    background: #fafafa;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .q-card {
    &.flat.bordered {
      border: 1px solid #e0e0e0;
      border-radius: 8px;
    }
  }

  .q-chip {
    font-weight: 500;
  }

  .q-linear-progress {
    border-radius: 4px;
  }

  .q-circular-progress {
    margin: 0 auto;
  }

  // 信息行样式
  .text-h6 {
    font-weight: 600;
    line-height: 1.4;
  }

  // 链接样式
  a {
    &:hover {
      text-decoration: underline !important;
    }
  }

  // 按钮样式
  .q-card-actions {
    .q-btn {
      min-width: 100px;
      font-weight: 500;
      transition: all 0.3s ease;

      &:hover {
        transform: translateY(-1px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      }
    }
  }
}

// 响应式设计
// 显示列选择器样式
.advertisement-page {
  .show-columns-selector {
    .q-chip {
      max-width: 120px;
      font-size: 11px;

      .q-chip__content {
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
    }
  }
}

@media (max-width: 768px) {
  .advertisement-page {
    padding: 10px;

    // 响应式布局调整
    .row {

      .col-5,
      .col-7 {
        width: 100%;
      }
    }

    // 显示列选择器在小屏幕上的调整
    .show-columns-selector {
      .q-select {
        min-width: 200px !important;
      }
    }
  }

  .q-btn-group {
    flex-direction: column;

    .q-btn {
      margin-bottom: 8px;
    }
  }

  .row {
    flex-direction: column;

    .col-auto {
      margin-bottom: 12px;
    }
  }
}

// 表格行内编辑样式
:deep(.vxe-table) {
  .q-select {
    .q-field__control {
      min-height: 32px;
    }

    .q-chip {
      max-width: 120px;
      font-size: 11px;

      .q-chip__content {
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
    }

    &.q-field--borderless {
      .q-field__control {
        &:before {
          border: 1px solid transparent;
        }

        &:hover:before {
          border-color: #e0e0e0;
        }
      }

      &.q-field--focused {
        .q-field__control:before {
          border-color: #1976d2;
        }
      }
    }
  }
}

// 图表容器样式
.advertisement-page {
  .chart-container {
    border-radius: 8px;
    overflow: hidden;

    #chart {
      width: 100%;
      height: 400px;
    }
  }
}

// 图表加载状态样式
.chart-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 400px;
  background: #fafafa;
  border-radius: 8px;

  .q-spinner {
    margin-bottom: 16px;
  }
}
</style>