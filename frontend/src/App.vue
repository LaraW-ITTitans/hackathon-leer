<template>
  <el-config-provider :size="size" :z-index="zIndex" :locale="german">
    <el-container class="app-container">
      <el-header v-if="!(isBlacklistedRoute || isFullPageRoute)" class="page-header">
        <PageHeaderComponent />
      </el-header>
      <el-container class="main-content-container">
        <el-aside width="200px" v-if="showSideMenu">
          <!-- TODO: fill with elements of sub pages -->
        </el-aside>
        <el-container class="main-and-footer-container">
          <el-main class="app-main">
            <el-row :gutter="24" v-if="!(isBlacklistedRoute || isFullPageRoute)">
              <el-col :span="24">
                <PageNavigationButtonComponent />
              </el-col>
            </el-row>
            <RouterView />
          </el-main>
          <el-footer v-if="!isBlacklistedRoute" class="app-footer">
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
import PageNavigationButtonComponent from '@/components/page-navigation-buttons/PageNavigationButtonComponent.vue'

const zIndex = 3000
const size = 'default'

const blacklist = ['/auth', '/register']
const fullPages = ['', '/']

const route = useRoute()

const isBlacklistedRoute = computed(() => blacklist.includes(route.path))
const isFullPageRoute = computed(() => fullPages.includes(route.path))

// TODO calculate from flags
const showSideMenu = computed(() => false) // !(isBlacklistedRoute || isFullPageRoute))

</script>

<style lang="scss" scoped>
.app-container {
  display: flex;
  flex-direction: column;
  min-height: 100vh; /* Ensure the container takes at least the full viewport height */
}

.main-content-container {
  flex: 1;
  display: flex;
}

.main-and-footer-container {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.app-main {
  flex: 1;
  padding: 20px;
  padding-bottom: 0 !important;
}

.app-footer {
  padding: 20px;
  text-align: center;
  margin-bottom: 20px;
}

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
