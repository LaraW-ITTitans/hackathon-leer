<template>
  <el-menu
    :router="true"
    :default-active="currentIndex"
    class="el-menu-demo"
    mode="horizontal"
  >
    <el-menu-item index="0">
      <img
        style="width: 150px"
        src="https://www.landkreis-leer.de/media/custom/3399_1_1_k.PNG?1647076132"
        alt="Landkreis Leer"
      />
    </el-menu-item>

    <template v-for="(item, index) in visibleItems" :key="item.to">
      <!-- leaf -->
      <el-menu-item
        v-if="!item.children?.length"
        :index="item.to"
        :disabled="item.disabled"
      >
        {{ item.label }}
      </el-menu-item>

      <!-- group with children -->
      <el-sub-menu v-else :index="index + 1">
        <template #title>{{ item.label }}</template>
        <template v-for="child in item.children" :key="child.to">
          <el-menu-item
            v-if="!child.children?.length"
            :index="child.to"
            :disabled="child.disabled"
          >
            {{ child.label }}
          </el-menu-item>

          <el-sub-menu v-else :index="child.to">
            <template #title>{{ child.label }}</template>
            <el-menu-item
              v-for="grand in child.children"
              :key="grand.to"
              :index="grand.to"
              :disabled="grand.disabled"
            >
              {{ grand.label }}
            </el-menu-item>
          </el-sub-menu>
        </template>
      </el-sub-menu>
    </template>
  </el-menu>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import useUserStore from '@/stores/user'
import type { MenuItem } from '@/components/page-navigation/types'

const items: MenuItem[] = [
  { label: 'Landing Page', to: '/' },

  // { label: 'Home', to: '/home', authOnly: true },

  // { label: 'Login', to: '/auth', guestOnly: true },
  { label: 'Profil', to: '/users/me', authOnly: true },

  { label: 'Administration', to: '/admin', authOnly: true },

  // { label: 'Info', to: '/info', disabled: true },

  { label: 'Datenquellen', to: '/admin/data-sources', authOnly: true }
]

const userStore = useUserStore()
const { isAuthenticated } = storeToRefs(userStore)

function filterByAuth(list: MenuItem[]): MenuItem[] {
  return list
    .filter(item => {
      if (item.authOnly && !isAuthenticated.value) return false
      if (item.guestOnly && isAuthenticated.value) return false
      return true
    })
    .map(item => ({
      ...item,
      children: item.children ? filterByAuth(item.children) : undefined,
    }))
}

const visibleItems = computed(() => filterByAuth(items))

const route = useRoute()

const currentIndex = computed(() => {
  const deepest = route.matched[route.matched.length - 1]
  return deepest?.path || route.path
})
</script>
