<template>
  <el-config-provider :size="size" :z-index="zIndex" :locale="german">
    <el-container>
      <el-header v-if="!isBlacklistedRoute" class="page-header">
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

import german from 'element-plus/es/locale/lang/de'

import PageHeaderComponent from '@/components/layout/PageHeaderComponent.vue'
import PageFooterComponent from '@/components/layout/PageFooterComponent.vue'

const zIndex = 3000
const size = 'default'

const blacklist = ['/auth', '/register']
const fullPages = ['/']

const route = useRoute()
const isBlacklistedRoute = computed(() => blacklist.includes(route.path))
const isFullPageRoute = computed(() => fullPages.includes(route.path))
</script>

<style lang="scss" scoped>
.app-header {
  position: sticky;
  top: 0;
  z-index: 1000;
  width: 100%;
  background: #fff;
  box-shadow: 0 2px 4px rgba(0,0,0,0.1);
  padding: 0;
}

.header-row {
  width: 100%;
  margin: 0;
}
</style>
