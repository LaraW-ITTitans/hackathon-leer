<template>
  <div class="user-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <span>Benutzerverwaltung</span>
          <el-button type="primary" @click="openDrawer()">Neuen Benutzer erstellen</el-button>
        </div>
      </template>

      <el-input
        v-model="searchQuery"
        placeholder="Benutzer nach Namen suchen"
        clearable
        class="search-input"
      />

      <el-alert v-if="error" type="error" :title="error" show-icon class="error-alert" />

      <el-skeleton v-if="loading && !users.length" :rows="5" animated />

      <el-table v-else :data="paginatedUsers" style="width: 100%" border stripe>
        <el-table-column prop="id" label="ID" />
        <el-table-column prop="userName" label="Username" sortable />
        <el-table-column prop="displayName" label="Displayname" sortable />
        <el-table-column label="Anzahl Nachweise" width="150">
          <template #default="">
            <!-- Placeholder for evidence count - would need separate API call -->
            <span>0</span>
          </template>
        </el-table-column>
        <el-table-column label="Aktionen" width="200">
          <template #default="scope">
            <el-button size="small" @click="openDrawer(scope.row.id)">Bearbeiten</el-button>
            <el-popconfirm
              title="Sind Sie sicher, dass Sie diesen Benutzer löschen möchten?"
              confirm-button-text="Ja"
              cancel-button-text="Nein"
              @confirm="handleDelete(scope.row.id)"
            >
              <template #reference>
                <el-button size="small" type="danger">Löschen</el-button>
              </template>
            </el-popconfirm>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-if="filteredUsers.length > pageSize"
        :page-size="pageSize"
        :current-page="currentPage"
        :page-sizes="[10, 20, 50, 100]"
        :small="false"
        :disabled="loading"
        :background="true"
        layout="total, sizes, prev, pager, next, jumper"
        :total="filteredUsers.length"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
        class="pagination-container"
      />
    </el-card>

    <DrawerComponent
      :title="formTitle"
      v-model="formOpen"
      :width="420"
      :show-close="true"
      :close-on-click-overlay="false"
      :lock-scroll="true"
      @closed="resetForm"
    >
      <el-form
        ref="userFormRef"
        :model="userForm"
        :rules="userFormRules"
        label-position="top"
        @submit.prevent="handleSubmit"
      >
        <el-form-item label="ID" prop="id" v-if="userForm.id">
          <el-input v-model="userForm.id" disabled />
        </el-form-item>
        <el-form-item label="Username" prop="userName">
          <el-input v-model="userForm.userName" placeholder="Username eingeben" />
        </el-form-item>
        <el-form-item label="Anrede" prop="title">
          <el-select v-model="userForm.title" placeholder="Anrede wählen" clearable>
            <el-option label="Herr" value="Herr" />
            <el-option label="Frau" value="Frau" />
            <el-option label="Divers" value="Divers" />
            <el-option label="Nicht gewählt" value="" />
          </el-select>
        </el-form-item>
        <el-form-item label="Vorname" prop="firstName">
          <el-input v-model="userForm.firstName" placeholder="Vorname eingeben" />
        </el-form-item>
        <el-form-item label="Nachname" prop="lastName">
          <el-input v-model="userForm.lastName" placeholder="Nachname eingeben" />
        </el-form-item>
        <el-form-item label="Passwort" prop="password" v-if="!userForm.id">
          <el-input
            v-model="userForm.password"
            type="password"
            placeholder="Passwort eingeben"
            show-password
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSubmit">Speichern</el-button>
          <el-button @click="formOpen = false">Abbrechen</el-button>
        </el-form-item>
      </el-form>
    </DrawerComponent>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, reactive } from 'vue'
import { useUsers } from './composeables/users'
import { ElNotification, ElMessage } from 'element-plus'
import type { CreateUserBindingModel, UpdateUserBindingModel } from '@/api/codegen'
import DrawerComponent from '@/components/drawer/DrawerComponent.vue'
import type { FormInstance, FormRules } from 'element-plus'
const { users, loading, error, fetchUsers, deleteUser, createUser, updateUser } = useUsers()

const searchQuery = ref('')
const currentPage = ref(1)
const pageSize = ref(10)
const formOpen = ref(false)

// User form data
const userForm = reactive({
  id: '',
  userName: '',
  title: '',
  firstName: '',
  lastName: '',
  password: '',
})

const userFormRef = ref<FormInstance>()

// Form validation rules
const userFormRules: FormRules = {
  userName: [
    { required: true, message: 'Username ist erforderlich', trigger: 'blur' },
    { min: 3, max: 50, message: 'Username muss zwischen 3 und 50 Zeichen lang sein', trigger: 'blur' }
  ],
  password: [
    {
      required: true,
      message: 'Passwort ist erforderlich',
      trigger: 'blur',
      validator: (rule, value, callback) => {
        if (!userForm.id && !value) {
          callback(new Error('Passwort ist bei neuen Benutzern erforderlich'))
        } else {
          callback()
        }
      }
    },
    { min: 6, message: 'Passwort muss mindestens 6 Zeichen lang sein', trigger: 'blur' }
  ]
}

// Computed properties
const filteredUsers = computed(() => {
  if (!searchQuery.value) return users.value

  return users.value.filter(user =>
    user.userName?.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
    user.displayName?.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

const paginatedUsers = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredUsers.value.slice(start, end)
})

const formTitle = computed(() => {
  return userForm.id ? 'Benutzer bearbeiten' : 'Neuen Benutzer erstellen'
})

// Generate display name from form data
const generateDisplayName = () => {
  const parts = []
  if (userForm.title) parts.push(userForm.title)
  if (userForm.firstName) parts.push(userForm.firstName)
  if (userForm.lastName) parts.push(userForm.lastName)
  return parts.join(' ') || userForm.userName
}

// Methods
const handleSizeChange = (val: number) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val: number) => {
  currentPage.value = val
}

const openDrawer = async (id?: string) => {
  if (id) {
    // Edit mode
    const user = users.value.find(u => u.id === id)
    if (user) {
      userForm.id = user.id
      userForm.userName = user.userName || ''
      userForm.title = ''
      userForm.firstName = ''
      userForm.lastName = ''
      userForm.password = ''
    }
  } else {
    // Create mode
    resetForm()
  }
  formOpen.value = true
}

const resetForm = () => {
  userForm.id = ''
  userForm.userName = ''
  userForm.title = ''
  userForm.firstName = ''
  userForm.lastName = ''
  userForm.password = ''
  userFormRef.value?.clearValidate()
}

const handleSubmit = async () => {
  if (!userFormRef.value) return

  const isValid = await userFormRef.value.validate().catch(() => false)
  if (!isValid) return

  try {
    const displayName = generateDisplayName()

    if (userForm.id) {
      // Update existing user
      const updateData: UpdateUserBindingModel = {
        id: userForm.id,
        userName: userForm.userName,
        displayName: displayName,
        password: userForm.password || undefined
      }
      await updateUser(updateData)
      ElNotification({
        title: 'Erfolg',
        message: 'Benutzer wurde erfolgreich aktualisiert',
        type: 'success',
      })
    } else {
      // Create new user
      const createData: CreateUserBindingModel = {
        userName: userForm.userName,
        displayName: displayName,
        password: userForm.password
      }
      await createUser(createData)
      ElNotification({
        title: 'Erfolg',
        message: 'Benutzer wurde erfolgreich erstellt',
        type: 'success',
      })
    }

    formOpen.value = false
  } catch (error: unknown) {
    ElNotification({
      title: 'Fehler',
      message: (error as Error).message || 'Ein Fehler ist aufgetreten',
      type: 'error',
    })
  }
}

const handleDelete = async (id: string) => {
  try {
    await deleteUser(id)
    ElMessage({
      type: 'success',
      message: 'Benutzer wurde erfolgreich gelöscht',
    })
  } catch (error: unknown) {
    ElNotification({
      title: 'Fehler',
      message: (error as Error).message || 'Fehler beim Löschen des Benutzers',
      type: 'error',
    })
  }
}

// Initialize
onMounted(() => {
  fetchUsers()
})
</script>

<style scoped>
.user-list {
  padding: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.search-input {
  margin-bottom: 20px;
}

.error-alert {
  margin-bottom: 20px;
}

.pagination-container {
  margin-top: 20px;
  display: flex;
  justify-content: center;
}
</style>
