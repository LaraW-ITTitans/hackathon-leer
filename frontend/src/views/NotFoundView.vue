<template>
  <el-container class="not-found">
    <el-card class="not-found-card" shadow="hover">
      <div class="icon-wrap">
        <el-icon size="64" color="var(--el-color-danger)">
          <WarningFilled />
        </el-icon>
      </div>
      <h2 class="title">404 – Seite nicht gefunden</h2>
      <p class="subtitle">
        Die Seite, die du suchst, existiert nicht oder wurde verschoben.
      </p>
      <p class="countdown">
        Weiterleitung in <strong>{{ countdown }}</strong> sekunden..
      </p>
      <div class="actions">
        <el-button type="primary" round @click="goHome">
          Zur Startseite
        </el-button>
        <el-button round @click="cancelRedirect" v-if="!canceled">
          Weiterleitung abbrechen
        </el-button>
      </div>
      <p v-if="canceled" class="note">
        Weiterleitung abgebrochen. Nutze die Buttons oben, um zu navigieren.
      </p>
    </el-card>
  </el-container>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { WarningFilled } from '@element-plus/icons-vue'

const router = useRouter()
const timeout = 5000
const countdown = ref(timeout / 1000)
const canceled = ref(false)

let timerInterval: number | undefined
let timerRedirect: number | undefined

onMounted(() => {
  timerInterval = window.setInterval(() => {
    if (countdown.value > 0) {
      countdown.value -= 1
    }
  }, 1000)

  timerRedirect = window.setTimeout(() => {
    if (!canceled.value) {
      if (window.history.length > 1) {
        router.back()
      } else {
        router.push('/')
      }
    }
  }, timeout)
})

const cancelRedirect = () => {
  canceled.value = true
  if (timerInterval) clearInterval(timerInterval)
  if (timerRedirect) clearTimeout(timerRedirect)
}

const goHome = () => {
  router.push('/')
}

onUnmounted(() => {
  if (timerInterval) clearInterval(timerInterval)
  if (timerRedirect) clearTimeout(timerRedirect)
})
</script>

<style scoped>
.not-found {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 80vh;
  padding: 1rem;
}

.not-found-card {
  max-width: 500px;
  width: 100%;
  text-align: center;
  border-radius: 16px;
  padding: 2rem 1.5rem;
}

.icon-wrap {
  margin-bottom: 1rem;
}

.title {
  margin: 0.5rem 0;
  font-size: 1.8rem;
  font-weight: 600;
}

.subtitle {
  color: var(--el-text-color-secondary);
  margin-bottom: 1rem;
}

.countdown {
  font-size: 1rem;
  margin-bottom: 1.5rem;
}

.actions {
  display: flex;
  gap: 0.75rem;
  justify-content: center;
  flex-wrap: wrap;
  margin-bottom: 0.75rem;
}

.note {
  font-size: 0.85rem;
  color: var(--el-text-color-secondary);
}
</style>
