<template>
  <el-row>
    <el-col :span="20"></el-col>
    <el-col :span="4" class="quick-actions">
      <el-button v-if="!isAuthenticated" :icon="Key" plain @click="onLoginClick"/>
      <ThemeToggle />
    </el-col>
  </el-row>
</template>

<script setup lang="ts">
import { Key } from '@element-plus/icons-vue'
import ThemeToggle from '@/components/theme-toggle/ThemeToggleComponent.vue'
import router from '@/router'
import useUserStore from '@/stores/user.ts'

const { isAuthenticated } = useUserStore();

const onLoginClick = () => {
  router.push('/auth');
};

</script>

<style scoped>
.quick-actions {
  margin-left: auto;
  flex-basis: unset;
}
</style>
