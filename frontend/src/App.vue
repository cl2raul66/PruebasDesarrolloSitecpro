<script setup lang="ts">
import { useRoute } from 'vue-router'
import AppNav from './components/AppNav.vue'
import ToastMessage from './components/ToastMessage.vue'

const route = useRoute()
const esLogin = (): boolean => route.name === 'login'
</script>

<template>
  <AppNav v-if="!esLogin()" />
  <router-view />
  <ToastMessage />
</template>
