<template>
  <el-row class="content-center">
    <el-text type="info">
      Daten Raum Ostfriesland &bull; Ein Projekt für Verwaltung, Wirtschaft, Wissenschaft & Zivilgesellschaft
    </el-text>
  </el-row>
  <el-row class="content-center">
    <el-text type="info">
      &copy; {{ year }} &nbsp;
      <el-link href="https://www.landkreis-leer.de" target="_blank" type="default">
        Landkreis Leer
      </el-link>
      &nbsp; - All rights reserved.
    </el-text>
  </el-row>
  <el-row class="quick-links" justify="center">
    <el-link href="#" target="_self" type="default">Impressum</el-link>
    &bull;
    <el-link href="#" target="_self" type="default">Datenschutz</el-link>
    &bull;
    <el-link href="#" target="_self" type="default">Kontakt</el-link>
  </el-row>
</template>

<script setup lang="ts">
const year = new Date().getFullYear();
</script>

<style lang="scss" scoped>
.content-center {
  display: flex;
  justify-content: center;
}

.quick-links {
  margin-top: 0.5rem;

  display: flex;
  justify-content: center;
  gap: 1.5rem;
  flex-wrap: wrap;

  .el-link {
    font-size: 0.9rem;
    color: var(--el-text-color-regular);
  }
}
</style>
