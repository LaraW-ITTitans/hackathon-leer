<template>
  <el-button type="primary" plain @click="toggleDark()" :icon="(isDark ? Sunny : Moon)" />
</template>

<script setup lang="ts">
import { useDark, useToggle } from '@vueuse/core'
import { Sunny, Moon } from '@element-plus/icons-vue'

const isDark = useDark({
  storageKey: 'theme-mode',
  attribute: 'class',
  valueDark: 'dark',
  valueLight: ''
})

const toggleDark = useToggle(isDark)
</script>
