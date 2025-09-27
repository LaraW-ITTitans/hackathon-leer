<template>
  <el-space wrap>
    <el-page-header
      :icon="ArrowLeft"
      @back="onBack"
    >
      <template #breadcrumb>
        <el-breadcrumb separator="//">
          <el-breadcrumb-item :to="{ path: '/' }">
            Datenraum Ostfriesland
          </el-breadcrumb-item>
        </el-breadcrumb>
      </template>

      <template #content>
        <span class="text-large font-600 mr-3"> {{ pageName }} </span>
      </template>

      <template #extra v-if="hasDescription">
        {{ description }}
      </template>

      <p></p>
    </el-page-header>
  </el-space>
</template>

<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import { computed } from 'vue'
import { ArrowLeft } from '@element-plus/icons-vue'

const router = useRouter()
const route = useRoute()

const pageName = computed(() => {
  if (route.meta?.title) {
    return route.meta?.title
  }

  return ''
})

const description = computed(() => {
  if (route.meta?.description) {
    return route.meta?.description
  }

  return ''
})

const hasDescription = computed(() => {
  return !(!route.meta?.title)
})

const onBack = () => {
  router.back()
}
</script>
