<template>
  <div class="data-source-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <span>Datenquellenverwaltung</span>
          <el-button type="primary" @click="openDrawer()">Neue Datenquelle erstellen</el-button>
        </div>
      </template>

      <el-input
        v-model="searchQuery"
        placeholder="Datenquellen nach Name durchsuchen"
        clearable
        class="search-input"
      />

      <el-alert v-if="error" type="error" :title="error" show-icon class="error-alert" />

      <el-skeleton v-if="loading && !dataSources.length" :rows="5" animated />

      <el-table v-else :data="paginatedDataSources" style="width: 100%" border stripe>
        <el-table-column prop="name" label="Name" sortable />
        <el-table-column prop="description" label="Beschreibung" />
        <el-table-column label="Benötigte Nachweise">
          <template #default="scope">
            <el-tag
              v-for="skill in scope.row.requiredSkills || []"
              :key="skill.id"
              class="mr-4"
              type="info"
              effect="light"
            >
              {{ skill.name }}
            </el-tag>
            <span v-if="!scope.row.requiredSkills || scope.row.requiredSkills.length === 0" class="text-muted">–</span>
          </template>
        </el-table-column>
        <el-table-column label="Aktionen" width="220">
          <template #default="scope">
            <el-button size="small" @click="openDrawer(scope.row.id)">Edit</el-button>
            <el-popconfirm
              title="Diese Datenquelle wirklich löschen?"
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
        v-if="filteredDataSources.length > pageSize"
        :page-size="pageSize"
        :current-page="currentPage"
        :page-sizes="[10, 20, 50, 100]"
        :disabled="loading"
        :background="true"
        layout="total, sizes, prev, pager, next, jumper"
        :total="filteredDataSources.length"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
        class="pagination-container"
      />
    </el-card>

    <DrawerComponent
      :title="formTitle"
      v-model="formOpen"
      :width="560"
      :show-close="true"
      :close-on-click-overlay="false"
      :lock-scroll="true"
      @closed="resetForm"
    >
      <el-form
        ref="dataSourceFormRef"
        :model="dataSourceForm"
        :rules="dataSourceFormRules"
        label-position="top"
        @submit.prevent="handleSubmit"
      >
        <el-form-item label="Name" prop="name">
          <el-input v-model="dataSourceForm.name" placeholder="Name der Datenquelle" />
        </el-form-item>
        <el-form-item label="Beschreibung" prop="description">
          <el-input
            v-model="dataSourceForm.description"
            type="textarea"
            :rows="3"
            placeholder="Beschreibung der Datenquelle"
          />
        </el-form-item>
        <el-form-item label="Benötigte Nachweise" prop="requiredSkillIds">
          <el-select
            v-model="dataSourceForm.requiredSkillIds"
            placeholder="Nachweise auswählen"
            multiple
            filterable
            collapse-tags
            collapse-tags-tooltip
            :loading="loadingSkills"
          >
            <el-option
              v-for="s in skills"
              :key="s.id"
              :label="s.name"
              :value="s.id"
            />
          </el-select>
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
import { ElNotification, ElMessage } from 'element-plus'
import type { FormInstance, FormRules } from 'element-plus'
import DrawerComponent from '@/components/drawer/DrawerComponent.vue'
import { useDataSources } from './composeables/dataSources'
import type { DataSourceBindingModel, CreateUpdateDataSourceBindingModel, BasicSkillBindingModel } from '@/api/codegen'

const { dataSources, skills, loading, error, fetchDataSources, fetchSkills, deleteDataSource, createDataSource, updateDataSource } = useDataSources()

const searchQuery = ref('')
const currentPage = ref(1)
const pageSize = ref(10)

const loadingSkills = ref(false)

onMounted(async () => {
  await fetchDataSources()
  loadingSkills.value = true
  try {
    await fetchSkills()
  } finally {
    loadingSkills.value = false
  }
})

const filteredDataSources = computed(() => {
  const list = dataSources.value || []
  if (!searchQuery.value) return list
  const q = searchQuery.value.toLowerCase()
  return list.filter((ds: DataSourceBindingModel) => (ds.name || '').toLowerCase().includes(q))
})

const paginatedDataSources = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  const end = start + pageSize.value
  return filteredDataSources.value.slice(start, end)
})

const handleSizeChange = (val: number) => {
  pageSize.value = val
  currentPage.value = 1
}

const handleCurrentChange = (val: number) => {
  currentPage.value = val
}

const handleDelete = async (id: string) => {
  try {
    await deleteDataSource(Number(id) as any as number)
    ElNotification({ title: 'Erfolg', message: 'Datenquelle gelöscht', type: 'success' })
  } catch (err: any) {
    ElNotification({ title: 'Fehler', message: err.message || 'Löschen fehlgeschlagen', type: 'error' })
  }
}

interface DataSourceForm extends CreateUpdateDataSourceBindingModel {
  id?: string | null
}

const dataSourceFormRef = ref<FormInstance>()
const dataSourceForm = reactive<DataSourceForm>({
  id: null,
  name: '',
  description: '',
  requiredSkillIds: [] as string[],
})

const dataSourceFormRules = reactive<FormRules<DataSourceForm>>({
  name: [
    { required: true, message: 'Bitte Namen angeben', trigger: 'blur' },
    { min: 3, message: 'Mindestens 3 Zeichen', trigger: 'blur' },
  ],
  description: [
    { required: true, message: 'Bitte Beschreibung angeben', trigger: 'blur' },
    { min: 5, message: 'Mindestens 5 Zeichen', trigger: 'blur' },
  ],
})

const formOpen = ref(false)
const formTitle = computed(() => (dataSourceForm.id ? 'Datenquelle bearbeiten' : 'Neue Datenquelle erstellen'))

const openDrawer = (entityId?: string) => {
  if (entityId) {
    const item = dataSources.value.find(d => d.id === entityId)
    if (!item) {
      ElMessage.error('Datenquelle nicht gefunden.')
      return
    }
    dataSourceForm.id = item.id
    dataSourceForm.name = item.name || ''
    dataSourceForm.description = item.description || ''
    dataSourceForm.requiredSkillIds = (item.requiredSkills || []).map(s => s.id)
  } else {
    resetForm()
  }
  formOpen.value = true
}

const resetForm = () => {
  dataSourceForm.id = null
  dataSourceForm.name = ''
  dataSourceForm.description = ''
  dataSourceForm.requiredSkillIds = []
  dataSourceFormRef.value?.resetFields()
}

const handleSubmit = async () => {
  if (!dataSourceFormRef.value) return
  await dataSourceFormRef.value.validate(async (valid) => {
    if (!valid) {
      ElMessage.error('Bitte Formularfehler beheben.')
      return
    }
    try {
      const payload: CreateUpdateDataSourceBindingModel = {
        name: dataSourceForm.name,
        description: dataSourceForm.description,
        requiredSkillIds: dataSourceForm.requiredSkillIds,
      }
      if (dataSourceForm.id) {
        await updateDataSource(Number(dataSourceForm.id) as any as number, payload)
        ElNotification({ title: 'Erfolg', message: 'Datenquelle aktualisiert', type: 'success' })
      } else {
        await createDataSource(payload)
        ElNotification({ title: 'Erfolg', message: 'Datenquelle erstellt', type: 'success' })
      }
      formOpen.value = false
      await fetchDataSources()
    } catch (err: any) {
      ElNotification({ title: 'Fehler', message: err.message || 'Speichern fehlgeschlagen', type: 'error' })
    }
  })
}
</script>

<style scoped>
.data-source-list { padding: 20px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.search-input { margin-bottom: 20px; max-width: 400px; }
.error-alert { margin-bottom: 20px; }
.pagination-container { margin-top: 20px; justify-content: flex-end; display: flex; }
.mr-4 { margin-right: 4px; }
.text-muted { color: #999; }
</style>
