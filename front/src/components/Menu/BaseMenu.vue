<template><q-scroll-area :visible="false" class="fit" :thumb-style="thumbStyleOfMenu">
  <div>
    <!-- 动态菜单 -->
    <q-list>
      <base-menu-item :my-router="menuList" :init-level="0" :bg-color="bgColor" :duration="300" :bg-color-level="1" />
    </q-list>
    <!-- 底部说明 -->
    <!-- <bottom-link/> -->
  </div>
</q-scroll-area></template>
<script setup>
import { ref, onMounted } from "vue";
import { thumbStyleOfMenu } from "../BaseContent/ThumbStyle";
import BaseMenuItem from "./BaseMenuItem";
import { useRouter } from "vue-router";
import asyncRoutes from "@/router/asyncRoutes";

defineOptions({
  name: "base-menu",
});

// Router
const router = useRouter();

// 响应式数据
const menuList = ref([]);
const bgColor = ref("bg-white");

// 组件挂载
onMounted(() => {
  // 直接使用 asyncRoutes 中的路由数据
  if (asyncRoutes && asyncRoutes[0] && asyncRoutes[0].children) {
    menuList.value = asyncRoutes[0].children;
  }
});
</script>
