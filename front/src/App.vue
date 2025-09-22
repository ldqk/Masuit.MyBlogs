<template>
<div id="app">
  <router-view></router-view>
</div>
</template>
<script setup>
defineOptions({
  name: 'App'
})
</script>
<style>
@import "./assets/css/main.css";
@import './assets/scss/transition.scss';
@import 'vue3-toastify/dist/index.css';
</style>
