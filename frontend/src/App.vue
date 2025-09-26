<template>
  <el-config-provider :size="size" :z-index="zIndex" :locale="language">
    <el-container>
      <el-header v-if="!isBlacklistedRoute">
        <PageHeaderComponent />
      </el-header>
      <el-container>
        <el-aside width="200px" v-if="!(isBlacklistedRoute || isFullPageRoute)">
        </el-aside>
        <el-container>
          <el-main>
            <RouterView />
          </el-main>
          <el-footer v-if="!isBlacklistedRoute">
            <PageFooterComponent />
          </el-footer>
        </el-container>
      </el-container>
    </el-container>
  </el-config-provider>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ElConfigProvider } from 'element-plus'
import { RouterView, useRoute } from 'vue-router'

import PageHeaderComponent from '@/components/layout/PageHeaderComponent.vue'
import PageFooterComponent from '@/components/layout/PageFooterComponent.vue'

const zIndex = 3000
const size = 'default'
const language = 'de'

const blacklist = ['/auth', '/register']
const fullPages = ['/']

const route = useRoute()
const isBlacklistedRoute = computed(() => blacklist.includes(route.path))
const isFullPageRoute = computed(() => fullPages.includes(route.path))
</script>
