<template>
  <el-card class="max-w-xl w-full mx-auto">
    <template #header>
      <div class="card-header">
        <span>Erstellen Sie Ihr Konto</span>
      </div>
    </template>

    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-position="top"
      status-icon
      require-asterisk-position="right"
      :validate-on-rule-change="false"
    >
      <el-form-item label="Vorname" prop="firstName">
        <el-input
          v-model="form.firstName"
          placeholder="Jane"
          autocomplete="given-name"
          :prefix-icon="User"
          clearable
        />
      </el-form-item>

      <el-form-item label="Nachname" prop="lastName">
        <el-input
          v-model="form.lastName"
          placeholder="Doe"
          autocomplete="family-name"
          :prefix-icon="User"
          clearable
        />
      </el-form-item>

      <el-form-item label="E-Mail" prop="email">
        <el-input
          v-model="form.email"
          placeholder="jane@example.com"
          autocomplete="email"
          :prefix-icon="Message"
          clearable
        />
      </el-form-item>

      <el-form-item label="Passwort" prop="password">
        <el-input
          v-model="form.password"
          show-password
          autocomplete="new-password"
          placeholder="Mindestens 8 Zeichen"
          :prefix-icon="Lock"
          @input="updateStrength"
          @change="updateStrength"
        />
        <div class="mt-2 w-full">
          <el-progress
            :percentage="strength.percent"
            :text-inside="true"
            :stroke-width="16"
            :format="() => strength.label"
          />
          <small class="text-gray-500 block mt-1">
            Verwenden Sie Groß- und Kleinbuchstaben, Zahlen und Symbole.
          </small>
        </div>
      </el-form-item>

      <el-form-item label="Passwort bestätigen" prop="confirmPassword">
        <el-input
          v-model="form.confirmPassword"
          show-password
          autocomplete="new-password"
          :prefix-icon="Lock"
        />
      </el-form-item>

      <el-form-item prop="agree">
        <el-checkbox v-model="form.agree" label="Ich stimme den Nutzungsbedingungen und Datenschutzbestimmungen zu." />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" :loading="submitting" @click="onSubmit" class="w-full">
          Account erstellen
        </el-button>
      </el-form-item>

      <el-form-item>
        <el-button :disabled="submitting" @click="onReset" class="w-full" plain>
          Reset
        </el-button>
      </el-form-item>
    </el-form>
  </el-card>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import type { FormInstance, FormRules } from 'element-plus'
import { ElMessage } from 'element-plus'
import { User, Message, Lock } from '@element-plus/icons-vue'

type RegisterPayload = {
  firstName: string
  lastName: string
  email: string
  password: string
}

const props = withDefaults(defineProps<{
  submitting?: boolean
}>(), {
  submitting: false
})

const emit = defineEmits<{
  (e: 'register', payload: RegisterPayload): void
  (e: 'error', message: string): void
  (e: 'validated', valid: boolean): void
}>()

// TODO: api call for registration / no endpoint yet

const formRef = ref<FormInstance>()

const form = reactive({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  confirmPassword: '',
  agree: false
})

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

const rules = reactive<FormRules>({
  firstName: [
    { required: true, message: 'First name is required', trigger: 'blur' },
    { min: 2, message: 'Must be at least 2 characters', trigger: 'blur' }
  ],
  lastName: [
    { required: true, message: 'Last name is required', trigger: 'blur' },
    { min: 2, message: 'Must be at least 2 characters', trigger: 'blur' }
  ],
  email: [
    { required: true, message: 'Email is required', trigger: ['blur', 'change'] },
    { validator: (_r, v, cb) => (emailRegex.test(v) ? cb() : cb(new Error('Enter a valid email'))), trigger: ['blur', 'change'] }
  ],
  password: [
    { required: true, message: 'Password is required', trigger: 'blur' },
    {
      validator: (_r, v: string, cb) => {
        // At least 8 chars, 1 upper, 1 lower, 1 digit
        const ok = /[A-Z]/.test(v) && /[a-z]/.test(v) && /\d/.test(v) && v?.length >= 8
        (ok ? cb() : cb(new Error('Min 8 chars with upper, lower & number')))
      },
      trigger: ['blur', 'change']
    }
  ],
  confirmPassword: [
    { required: true, message: 'Please confirm your password', trigger: 'blur' },
    {
      validator: (_r, v: string, cb) => {
        (v === form.password ? cb() : cb(new Error("Passwords don't match")))
      },
      trigger: ['blur', 'change']
    }
  ],
  agree: [
    {
      validator: (_r, v: boolean, cb) => (v ? cb() : cb(new Error('You must agree to continue'))),
      trigger: 'change'
    }
  ]
})

const strength = reactive({ percent: 0, label: 'Weak' })
const updateStrength = () => {
  const p = form.password || ''
  let score = 0
  if (p.length >= 8) score += 1
  if (/[A-Z]/.test(p)) score += 1
  if (/[a-z]/.test(p)) score += 1
  if (/\d/.test(p)) score += 1
  if (/[^A-Za-z0-9]/.test(p)) score += 1
  const map = [
    { max: 1, label: 'Very weak', percent: 10 },
    { max: 2, label: 'Weak', percent: 30 },
    { max: 3, label: 'Fair', percent: 55 },
    { max: 4, label: 'Good', percent: 75 },
    { max: 5, label: 'Strong', percent: 100 }
  ]
  const bucket = map.find(m => score <= m.max) || map[map.length - 1]
  strength.percent = bucket.percent
  strength.label = bucket.label
}

const onSubmit = async () => {
  try {
    const valid = await formRef.value?.validate()
    emit('validated', !!valid)
    if (valid) {
      const payload: RegisterPayload = {
        firstName: form.firstName.trim(),
        lastName: form.lastName.trim(),
        email: form.email.trim(),
        password: form.password
      }
      emit('register', payload)
      ElMessage.success('Form valid — submitting…')
    }
  } catch (err: any) {
    emit('error', err?.message ?? 'Validation failed')
  }
}

const onReset = () => {
  formRef.value?.resetFields()
  updateStrength()
}
</script>

<style scoped>
.card-header {
  display: flex;
  align-items: center;
  font-weight: 600;
  font-size: 1.125rem;
}
.max-w-xl {
  max-width: 36rem;
}
.w-full {
  width: 100%;
}
.mx-auto {
  margin-left: auto;
  margin-right: auto;
}
.mt-2 { margin-top: .5rem; }
.mt-1 { margin-top: .25rem; }
.text-gray-500 { color: #6b7280; }
.block { display: block; }
</style>
