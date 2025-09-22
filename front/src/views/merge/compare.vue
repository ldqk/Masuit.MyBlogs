<template>
<div class="merge-compare-container">
  <!-- 操作按钮组 -->
  <div class="q-mb-lg">
    <div class="q-gutter-md">
      <q-btn color="info" label="接受合并" :loading="loading">
        <q-popup-proxy transition-show="scale" transition-hide="scale">
          <q-card>
            <q-card-section class="row items-center">
              <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
              <div>
                <div class="text-h6">确认操作</div>
                <div class="text-subtitle2">此操作将直接合并该修改，是否继续？</div>
              </div>
            </q-card-section>
            <q-card-actions align="right">
              <q-btn flat label="确认" color="negative" v-close-popup @click="acceptMerge" />
              <q-btn flat label="取消" color="primary" v-close-popup />
            </q-card-actions>
          </q-card>
        </q-popup-proxy>
      </q-btn>
      <q-btn color="positive" label="编辑并合并" :to="`/posts/merge/edit?id=${mergeId}`" />
      <q-btn color="negative" label="拒绝合并" :loading="loading">
        <q-popup-proxy transition-show="scale" transition-hide="scale">
          <q-card>
            <q-card-section class="row items-center">
              <div style="width: 400px;">
                <div class="text-h6"><q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />拒绝合并理由：</div>
                <q-input dense autogrow outlined v-model="reason" placeholder="请填写拒绝理由" />
              </div>
            </q-card-section>
            <q-card-actions align="right">
              <q-btn flat label="确认" :disabled="!reason" color="negative" v-close-popup @click="rejectMerge" />
              <q-btn flat label="取消" color="primary" v-close-popup />
            </q-card-actions>
          </q-card>
        </q-popup-proxy>
      </q-btn>
      <q-btn color="warning" label="标记为恶意修改" :loading="loading">
        <q-popup-proxy transition-show="scale" transition-hide="scale">
          <q-card>
            <q-card-section class="row items-center">
              <q-icon name="warning" color="red" size="2rem" class="q-mr-sm" />
              <div>
                <div class="text-h6">确认操作</div>
                <div class="text-subtitle2">此操作将标记该修改为恶意修改，是否继续？</div>
              </div>
            </q-card-section>
            <q-card-actions align="right">
              <q-btn flat label="确认" color="negative" v-close-popup @click="blockMerge" />
              <q-btn flat label="取消" color="primary" v-close-popup />
            </q-card-actions>
          </q-card>
        </q-popup-proxy>
      </q-btn>
    </div>
  </div>
  <!-- 对比内容 -->
  <div class="row q-col-gutter-md">
    <!-- 原文 -->
    <div class="col-md-6 col-12">
      <q-card>
        <q-card-section>
          <h3 class="text-h5 q-mb-md">原文：</h3>
          <div v-if="oldPost">
            <div class="text-center q-mb-md">
              <h4 class="text-h6">{{ oldPost.Title }}</h4>
            </div>
            <div class="article-content" v-html="oldPost.Content"></div>
          </div>
        </q-card-section>
      </q-card>
    </div>
    <!-- 修改后 -->
    <div class="col-md-6 col-12">
      <q-card>
        <q-card-section>
          <h3 class="text-h5 q-mb-md">修改后：</h3>
          <div v-if="newPost">
            <div class="text-center q-mb-md">
              <h4 class="text-h6">{{ newPost.Title }}</h4>
            </div>
            <div class="article-content" v-html="newPost.Content"></div>
          </div>
        </q-card-section>
      </q-card>
    </div>
  </div>
  <q-inner-loading :showing="loading">
    <q-spinner color="primary" size="100px" />
  </q-inner-loading>
</div>
</template>
<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { toast } from 'vue3-toastify'
import { useRouter, useRoute } from 'vue-router'
import api from '@/axios/AxiosConfig'

// 类型定义
interface ApiResponse {
  Data?: any
  Message?: string
  Success?: boolean
}

interface PostData {
  Id: string
  Title: string
  Content: string
}

// 路由和Quasar实例
const router = useRouter()
const route = useRoute()

// 响应式数据
const loading = ref(false)
const oldPost = ref<PostData | null>(null)
const newPost = ref<PostData | null>(null)
const mergeId = ref('')

// 方法
const loadCompareData = async () => {
  loading.value = true
  try {
    mergeId.value = route.query.id as string
    if (!mergeId.value) {
      toast.error('缺少合并ID参数', { autoClose: 2000, position: 'top-center' })
      return
    }

    const data = await api.get(`/merge/compare/${mergeId.value}`) as ApiResponse

    if (data?.Data) {
      oldPost.value = data.Data.old
      newPost.value = data.Data.newer
    }
  } catch (error) {
    toast.error('加载对比数据失败', { autoClose: 2000, position: 'top-center' })
    console.error('Error loading compare data:', error)
  } finally {
    loading.value = false
  }
}

// 接受合并
const acceptMerge = async () => {
  loading.value = true
  const data = await api.post(`/merge/${mergeId.value}`) as ApiResponse
  toast.success(data?.Message || '合并成功', { autoClose: 2000, position: 'top-center' })
  router.push('/posts/merge')
  loading.value = false
}
const reason = ref('')
// 拒绝合并
const rejectMerge = async () => {
  if (!reason.value.trim()) {
    toast.error('请填写拒绝理由！', { autoClose: 2000, position: 'top-center' })
    return
  }

  loading.value = true
  const data = await api.post(`/merge/reject/${mergeId.value}`, { reason: reason.value }) as ApiResponse
  toast.success(data?.Message || '已拒绝合并', { autoClose: 2000, position: 'top-center' })
  router.push('/posts/merge')
  loading.value = false
}

// 标记恶意修改
const blockMerge = async () => {
  loading.value = true
  const data = await api.post(`/merge/block/${mergeId.value}`) as ApiResponse
  toast.success(data?.Message || '已标记为恶意修改', { autoClose: 2000, position: 'top-center' })
  router.push('/posts/merge')
  loading.value = false
}
// 生命周期钩子
onMounted(() => {
  loadCompareData()
})
</script>
<style scoped>
.merge-compare-container {
  padding: 16px;
}

.article-content {
  line-height: 1.6;
  word-wrap: break-word;
}

.article-content :deep(img) {
  max-width: 100%;
  height: auto;
}

.article-content :deep(ins) {
  background-color: #cfc;
  text-decoration: none;
}

.article-content :deep(del) {
  color: #999;
  background-color: #FEC8C8;
}

@media (max-width: 768px) {
  .merge-compare-container {
    padding: 8px;
  }
}
</style>