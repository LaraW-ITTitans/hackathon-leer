<template>
  <section class="p-6">
    <!-- Profile details -->
    <el-card class="mb-6">
      <template #header>
        <div class="card-header">
          <span>Account</span>
          <el-button
            type="danger"
            :icon="Logout"
            @click="logout"
          >
            Logout
          </el-button>
        </div>
      </template>

      <el-descriptions :column="2" border size="default">
        <el-descriptions-item label="ID">
          <el-text tag="code">{{ user.id }}</el-text>
        </el-descriptions-item>
        <el-descriptions-item label="Username">
          <el-text>{{ user.username }}</el-text>
        </el-descriptions-item>
        <el-descriptions-item label="Anzeigename">
          <el-text>{{ user.displayName || '—' }}</el-text>
        </el-descriptions-item>
        <el-descriptions-item label="Sitzungsende">
          <el-space wrap>
            <el-tag :type="remainingSeconds === 0 ? 'danger' : remainingSeconds < 300 ? 'warning' : 'success'" round>
              {{ expiresLabel }}
            </el-tag>
            <el-text type="info">({{ localExpiry }})</el-text>
          </el-space>
        </el-descriptions-item>
        <el-descriptions-item label="Berechtigungen">
          <el-text tag="code">{{ claimsString }}</el-text>
        </el-descriptions-item>
      </el-descriptions>
    </el-card>

    <el-divider />

    <!-- Change password form -->
    <el-card class="">
      <template #header>
        <div class="card-header">
          <span>Passwort ändern</span>
        </div>
      </template>

      <form @submit.prevent="onSubmit">
        <el-form :model="form" label-position="top" class="grid gap-4">
          <el-form-item label="Aktuelles Passwort">
            <el-input
              v-model.trim="form.currentPassword"
              type="password"
              show-password
              autocomplete="current-password"
              clearable
              :disabled="loading"
            />
          </el-form-item>

          <el-form-item label="Neues Passwort">
            <el-input
              v-model.trim="form.newPassword"
              type="password"
              show-password
              autocomplete="new-password"
              clearable
              :disabled="loading"
            />
            <template #error>
              <el-text v-if="passwordHint" size="small" type="info">{{ passwordHint }}</el-text>
            </template>
          </el-form-item>

          <el-form-item label="Neues Passwort wiederholen">
            <el-input
              v-model.trim="form.confirmPassword"
              type="password"
              show-password
              autocomplete="new-password"
              clearable
              :disabled="loading"
            />
          </el-form-item>

          <el-form-item>
            <el-space>
              <el-button
                type="primary"
                native-type="submit"
                :disabled="!canSubmit || loading"
                :loading="loading"
              >
                Passwort ändern
              </el-button>
              <el-text v-if="error" type="danger">{{ error }}</el-text>
              <el-text v-if="success" type="success">Das Passwort wurde erfolgreich aktualisiert.</el-text>
            </el-space>
          </el-form-item>
        </el-form>
      </form>
    </el-card>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import useUserStore from '@/stores/user'
import { userApi } from '@/api'
import type { UserState } from '@/types'
import { ChangeOwnPasswordBindingModel } from '@/api/codegen'

const userStore: UserState = useUserStore()
const router = useRouter()

const user = computed(() => ({
  id: userStore.user.user.id,
  username: userStore.user.user.name,
  displayName: userStore.user.user.displayName,
  tokenExpiresAt: userStore.tokenExpiresAt,
  claims: userStore.user.user.claims
}))

const claimsString = computed(() => {
  if (!user.value.claims || user.value.claims.length === 0) {
    return ''
  }

  return user.value.claims.join(', ')
})

// --- Token expiry formatting ---
const now = ref(Date.now())
let timer: number | undefined

onMounted(() => {
  timer = window.setInterval(() => {
    now.value = Date.now()
  }, 1000)
})

onUnmounted(() => {
  if (timer) window.clearInterval(timer)
})

const remainingSeconds = computed(() =>
  Math.max(0, Math.floor(user.value.tokenExpiresAt - now.value / 1000))
)

const expiresLabel = computed(() => {
  const s = remainingSeconds.value
  if (s <= 0) return 'Expired'

  if (s < 60) return `${s}s`

  const m = Math.floor(s / 60)
  const sec = s % 60
  if (s < 3600) return `${m}m ${sec}s`

  const h = Math.floor(m / 60)
  const mm = m % 60
  if (s < 86400) return `${h}h ${mm}m`

  const d = Math.floor(h / 24)
  const hh = h % 24
  return `${d}d ${hh}h ${mm}m`
})

const localExpiry = computed(() => {
  if (!user.value.tokenExpiresAt) return '—'
  const d = new Date(user.value.tokenExpiresAt * 1000)
  return d.toLocaleString()
})

// --- Change password form ---
const form = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})

const loading = ref(false)
const error = ref<string | null>(null)
const success = ref(false)

const passwordHint = computed(() => {
  const p = form.value.newPassword
  if (!p) return ''
  const rules = [
    /[a-z]/.test(p),
    /[A-Z]/.test(p),
    /\d/.test(p),
    /[^\w\s]/.test(p),
    p.length >= 8,
  ]
  const score = rules.filter(Boolean).length
  const labels = ['very weak', 'weak', 'fair', 'good', 'strong']
  return `Strength: ${labels[Math.max(0, score - 1)]}`
})

const canSubmit = computed(() => {
  return (
    !!form.value.currentPassword &&
    !!form.value.newPassword &&
    form.value.newPassword === form.value.confirmPassword &&
    form.value.newPassword.length >= 8 &&
    !loading.value
  )
})

const logout = () => {
  userStore.logout()
  router.push('/')
}

const onSubmit = async () => {
  error.value = null
  success.value = false
  if (!canSubmit.value) {
    error.value = 'Please fix the form errors.'
    return
  }
  loading.value = true
  try {
    await userApi.changeOwnPassword(new ChangeOwnPasswordBindingModel({
      currentPassword: form.value.currentPassword,
      newPassword: form.value.newPassword,
    }))

    // Reset form and show success
    form.value.currentPassword = ''
    form.value.newPassword = ''
    form.value.confirmPassword = ''
    success.value = true
  } catch (e: any) {
    error.value = e.message || 'Something went wrong.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.card-header { display: flex; align-items: center; justify-content: space-between; }
section :deep(.el-descriptions__label) { white-space: nowrap; }
</style>
