<template>
  <el-menu>
    <el-row :gutter="24" align="middle" class="header-row">
        <el-col :span="16">
          <PageNavigationComponent />
        </el-col>

        <el-col :span="8" class="text-right">
          <el-row :gutter="10" align="middle">
            <el-col :span="16">
              <el-input
                class="sedgwick-ave-regular"
                placeholder="Datenraum durchsuchen"
                :suffix-icon="Search"
                clearable
                v-model="searchInput"
              />
            </el-col>
            <el-col :span="8">
              <el-row align="middle">
                <el-col>
                  <el-button v-if="!isAuthenticated" :icon="Key" plain @click="onLoginClick"/>
                  <ThemeToggle />
                </el-col>
                <el-col>
                </el-col>
              </el-row>
            </el-col>
          </el-row>
        </el-col>
    </el-row>
  </el-menu>

</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { Key, Search } from '@element-plus/icons-vue'
import useUserStore from '@/stores/user.ts'

import ThemeToggle from '@/components/theme-toggle/ThemeToggleComponent.vue'
import PageNavigationComponent from '@/components/page-navigation/PageNavigationComponent.vue'

const router = useRouter()
const { isAuthenticated } = useUserStore();

const searchInput = ref<string>('')

const onLoginClick = () => {
  router.push('/auth');
};
</script>
