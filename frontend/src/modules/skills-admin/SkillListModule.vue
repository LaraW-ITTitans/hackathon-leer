<template>
  <div class="skill-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <span>Nachweisverwaltung</span>
          <el-button type="primary" @click="openDrawer()">Neuen Nachweis erstellen</el-button>
        </div>
      </template>

      <el-input
        v-model="searchQuery"
        placeholder="Nachweise nach Name durchsuchen"
        clearable
        class="search-input"
      />

      <el-alert v-if="error" type="error" :title="error" show-icon class="error-alert" />

      <el-skeleton v-if="loading && !skills.length" :rows="5" animated />

      <el-table v-else :data="paginatedSkills" style="width: 100%" border stripe>
        <el-table-column prop="id" label="ID" />
        <el-table-column prop="name" label="Name" sortable />
        <el-table-column prop="description" label="Beschreibung" />
        <el-table-column label="Aktionen" width="220">
          <template #default="scope">
            <el-button size="small" @click="openDrawer(scope.row.id)">Edit</el-button>
            <el-popconfirm
              title="Diesen Nachweis wirklich löschen?"
              confirm-button-text="Ja"
              cancel-button-text="Nein"
              @confirm="handleDelete(scope.row.id)"
            >
              <template #reference>
                <el-button size="small" type="danger">Delete</el-button>
              </template>
            </el-popconfirm>
          </template>
        </el-table-column>
      </el-table>

      <el-pagination
        v-if="filteredSkills.length > pageSize"
        :page-size="pageSize"
        :current-page="currentPage"
        :page-sizes="[10, 20, 50, 100]"
        :disabled="loading"
        :background="true"
        layout="total, sizes, prev, pager, next, jumper"
        :total="filteredSkills.length"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
        class="pagination-container"
      />
    </el-card>

    <DrawerComponent
      :title="formTitle"
      v-model="formOpen"
      :width="480"
      :show-close="true"
      :close-on-click-overlay="false"
      :lock-scroll="true"
      @closed="resetForm"
    >
      <el-form
        ref="skillFormRef"
        :model="skillForm"
        :rules="skillFormRules"
        label-position="top"
        @submit.prevent="handleSubmit"
      >
        <el-form-item label="ID" prop="id" v-if="skillForm.id">
          <el-input v-model="skillForm.id" disabled />
        </el-form-item>
        <el-form-item label="Name" prop="name">
          <el-input v-model="skillForm.name" placeholder="Name des Nachweises" />
        </el-form-item>
        <el-form-item label="Beschreibung" prop="description">
          <el-input
            v-model="skillForm.description"
            type="textarea"
            :rows="3"
            placeholder="Beschreibung des Nachweises"
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
import { useRouter } from 'vue-router'
import { useSkills } from './composeables/skills'
import { ElNotification, ElMessage } from 'element-plus'
import type { BasicSkillBindingModel } from '@/api/codegen'
import DrawerComponent from '@/components/drawer/DrawerComponent.vue'
import type { FormInstance, FormRules } from 'element-plus'

const router = useRouter()
const { skills, loading, error, fetchSkills, deleteSkill, createSkill, updateSkill } = useSkills()

const searchQuery = ref('')
const currentPage = ref(1)
const pageSize = ref(10)

interface SkillForm {
  id: number | null
  name: string
  description: string
}

const skillFormRef = ref<FormInstance>()
const skillForm = reactive<SkillForm>({
  id: null,
  name: '',
  description: '',
})

const skillFormRules = reactive<FormRules<SkillForm>>({
  name: [
    { required: true, message: 'Bitte Name eingeben', trigger: 'blur' },
    { min: 3, message: 'Mindestens 3 Zeichen', trigger: 'blur' },
  ],
  description: [
    { required: true, message: 'Bitte Beschreibung eingeben', trigger: 'blur' },
    { min: 5, message: 'Mindestens 5 Zeichen', trigger: 'blur' },
  ],
})

onMounted(async () => {
  await fetchSkills()
})

const filteredSkills = computed(() => {
  const currentSkills = skills.value || []
  if (!searchQuery.value?.length) return currentSkills
  const q = searchQuery.value.toLowerCase()
  return currentSkills.filter((s: BasicSkillBindingModel) =>
    (s.name || '').toLowerCase().includes(q) || (s.description || '').toLowerCase().includes(q)
  )
})

const paginatedSkills = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredSkills.value.slice(start, end)
})

const handleSizeChange = (val: number) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val: number) => {
  currentPage.value = val
}

const handleDelete = async (id: number) => {
  try {
    await deleteSkill(id)
    ElNotification({
      title: 'Erfolg',
      message: 'Nachweis gelöscht',
      type: 'success',
    })
  } catch (err: any) {
    ElNotification({
      title: 'Fehler',
      message: err.message || 'Löschen fehlgeschlagen',
      type: 'error',
    })
  }
}

const formTitle = computed(() => (skillForm.id ? 'Nachweis bearbeiten' : 'Neuen Nachweis erstellen'))
const formOpen = ref(false)

const openDrawer = (entityId?: number) => {
  if (entityId != null) {
    const skillToEdit = skills.value.find((s) => s.id === entityId)
    if (skillToEdit) {
      skillForm.id = (skillToEdit.id as number) ?? null
      skillForm.name = skillToEdit.name || ''
      skillForm.description = (skillToEdit as any).description || ''
    } else {
      ElMessage.error('Nachweis zum Bearbeiten nicht gefunden')
      return
    }
  } else {
    resetForm()
  }
  formOpen.value = true
}

const resetForm = () => {
  skillForm.id = null
  skillForm.name = ''
  skillForm.description = ''
  skillFormRef.value?.resetFields()
}

const handleSubmit = async () => {
  if (!skillFormRef.value) return

  await skillFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        if (skillForm.id != null) {
          await updateSkill(skillForm.id, { name: skillForm.name, description: skillForm.description })
          ElNotification({ title: 'Erfolg', message: 'Nachweis aktualisiert', type: 'success' })
        } else {
          await createSkill({ name: skillForm.name, description: skillForm.description })
          ElNotification({ title: 'Erfolg', message: 'Nachweis erstellt', type: 'success' })
        }
        formOpen.value = false
        await fetchSkills()
      } catch (err: any) {
        ElNotification({ title: 'Fehler', message: err.message || 'Speichern fehlgeschlagen', type: 'error' })
      }
    } else {
      ElMessage.error('Bitte die Fehler im Formular korrigieren')
    }
  })
}
</script>

<style scoped>
.skill-list { padding: 20px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-input { margin-bottom: 20px; max-width: 400px; }
.error-alert { margin-bottom: 20px; }
.pagination-container { margin-top: 20px; justify-content: flex-end; display: flex; }
</style>
