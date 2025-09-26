<template>
  <el-row
    type="flex"
    justify="center"
    align="middle"
  >
    <el-col>
      <el-card shadow="hover" style="max-width: 600px">
        <template #header>
          <div class="card-header">Authentifizierung</div>
        </template>

        <el-form
          ref="formRef"
          :model="form"
          :rules="rules"
          label-position="top"
          @keyup.enter="onSubmit"
        >
          <el-form-item label="E-Mail" prop="email">
            <el-input
              v-model="form.email"
              placeholder="you@example.com"
              clearable
              autofocus
            />
          </el-form-item>

          <el-form-item label="Passwort" prop="password">
            <el-input
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Dein Passwort"
              show-password
            />
          </el-form-item>

          <el-form-item>
            <el-checkbox v-model="form.remember">Eingeloggt bleiben</el-checkbox>
          </el-form-item>

          <el-form-item>
            <el-button
              type="primary"
              :loading="loading"
              @click="onSubmit"
              style="width: 100%"
            >
              {{ loading ? 'Sie werden angemeldet..' : 'Anmelden' }}
            </el-button>
          </el-form-item>
        </el-form>
      </el-card>
    </el-col>
  </el-row>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import { login } from '@/services/auth'
import useUserStore from '@/stores/user.ts'
import { useRouter } from 'vue-router'

const router = useRouter()

const formRef = ref<FormInstance>()
const loading = ref(false)
const showPassword = ref(false)

const form = reactive({
  email: '',
  password: '',
  remember: false,
})

const rules: FormRules = {
  email: [
    { required: true, message: 'E-Mail-Adresse ist erforderlich', trigger: 'blur' },
    { type: 'email', message: 'Bitte eine gültige E-Mail-Adresse eingeben', trigger: ['blur', 'change'] },
  ],
  password: [
    { required: true, message: 'Passwort ist erforderlich', trigger: 'blur' },
    { min: 6, message: 'Mindestens 6 Zeichen', trigger: 'blur' },
  ],
}

async function onSubmit() {
  if (!formRef.value) return
  try {
    await formRef.value.validate()
  } catch {
    return
  }

  loading.value = true
  try {
    const authResult = await login({
      email: form.email,
      password: form.password,
      remember: form.remember,
    })

    if (!authResult.token) {
      ElMessage.error('Login fehlgeschlagen. Prüfe deine Zugangsdaten.')
    } else {
      const userStore = useUserStore()
      userStore.login(authResult)

      ElMessage.success(`Willkommen zurück, ${authResult.user.name}!`)

      router.push('/home')
    }
  } catch (err: any) {
    const msg = err?.response?.data?.message || 'Login fehlgeschlagen. Prüfe deine Zugangsdaten.'
    ElMessage.error(msg)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.card-header {
  font-size: 1.2rem;
  font-weight: 600;
}
</style>
