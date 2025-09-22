<template><q-breadcrumbs class="flex items-center" active-color="none" style="transform: translateX(10px);">
  <transition-group name="breadcrumb">
    <q-breadcrumbs-el v-for="(v, i) in filteredBreadcrumbs" :key="i + v.title" class="items-center" style="vertical-align: middle" :label="v.title" :icon="v.icon === '' ? undefined : v.icon">
      <template #default>
        <div v-if="filteredBreadcrumbs.length !== i + 1" style="margin:0px 8px 0px 8px"> / </div>
      </template>
    </q-breadcrumbs-el>
  </transition-group>
</q-breadcrumbs></template>
<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useMainStore } from '@/store/index'

const store = useMainStore()
const breadcrumbs = ref([])

const getBreadcrumbs = computed(() => store.getBreadcrumbs)
const filteredBreadcrumbs = computed(() => breadcrumbs.value.filter(v => v.title))

onMounted(() => {
  breadcrumbs.value = store.getBreadcrumbs
})

watch(getBreadcrumbs, (newVal) => {
  breadcrumbs.value = newVal
})
</script>
