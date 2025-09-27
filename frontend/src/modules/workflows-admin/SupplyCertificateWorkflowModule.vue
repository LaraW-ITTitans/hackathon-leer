<template>
  <div class="workflow-admin">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <span>Workflows: Nachweis beantragen</span>
          <el-button type="primary" @click="openDrawer()">Neuen Workflow starten</el-button>
        </div>
      </template>

      <el-alert v-if="error" type="error" :title="error" show-icon class="error-alert" />

      <div class="tables">
        <!-- Own Workflows -->
        <template v-if="ownWorkflows.length">
          <h3>Meine Workflows</h3>
          <el-table :data="ownWorkflows" style="width: 100%" border stripe>
            <el-table-column prop="id" label="ID" />
            <el-table-column label="Status">
              <template #default="scope">
                <el-tag :type="stateTagType(scope.row.state)">{{ stateText(scope.row.state) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="Aktionen" width="280">
              <template #default="scope">
                <el-button size="small" @click="viewFile(scope.row.id)">Datei ansehen</el-button>
                <el-popconfirm
                  v-if="scope.row.state === 1"
                  title="Diesen Workflow wirklich abbrechen?"
                  confirm-button-text="Ja"
                  cancel-button-text="Nein"
                  @confirm="handleCancel(scope.row.id)"
                >
                  <template #reference>
                    <el-button size="small" type="danger">Abbrechen</el-button>
                  </template>
                </el-popconfirm>
              </template>
            </el-table-column>
          </el-table>
        </template>

        <!-- Reviewable Workflows -->
        <template v-if="reviewableWorkflows.length">
          <h3 class="mt-24">Zu prüfen</h3>
          <el-table :data="reviewableWorkflows" style="width: 100%" border stripe>
            <el-table-column prop="id" label="ID" />
            <el-table-column label="Status">
              <template #default="scope">
                <el-tag :type="stateTagType(scope.row.state)">{{ stateText(scope.row.state) }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="Datei" width="140">
              <template #default="scope">
                <el-button link type="primary" @click="viewFile(scope.row.id)">Ansehen</el-button>
              </template>
            </el-table-column>
            <el-table-column label="Entscheidung" width="420">
              <template #default="scope">
                <div class="decision-row">
                  <el-radio-group v-model="decisions[scope.row.id].accept" size="small">
                    <el-radio-button :label="true">Akzeptieren</el-radio-button>
                    <el-radio-button :label="false">Ablehnen</el-radio-button>
                  </el-radio-group>
                  <el-input
                    v-model="decisions[scope.row.id].comment"
                    placeholder="Kommentar"
                    class="ml-8"
                    style="width: 220px"
                  />
                  <el-button type="primary" size="small" class="ml-8" @click="handleProcess(scope.row.id)">Ausführen</el-button>
                </div>
              </template>
            </el-table-column>
          </el-table>
        </template>

        <el-empty v-if="!ownWorkflows.length && !reviewableWorkflows.length && !loading" description="Keine Workflows vorhanden." />
      </div>
    </el-card>

    <!-- Drawer with stepper to create workflow -->
    <DrawerComponent
      :title="'Neuen Workflow starten'"
      v-model="drawerOpen"
      :width="720"
      :show-close="true"
      :close-on-click-overlay="false"
      :lock-scroll="true"
      @closed="resetForm"
    >
      <div class="stepper">
        <el-steps :active="activeStep" finish-status="success" align-center>
          <el-step title="Nachweis auswählen" />
          <el-step title="Datei hochladen" />
          <el-step title="Zusammenfassung" />
        </el-steps>

        <div v-show="activeStep === 0" class="step-content">
          <el-form label-position="top">
            <el-form-item label="Nachweis (Skill)">
              <el-select v-model="selectedSkillId" placeholder="Nachweis auswählen" filterable :loading="loadingSkills">
                <el-option v-for="s in skills" :key="s.id" :label="s.name" :value="s.id" />
              </el-select>
            </el-form-item>
          </el-form>
        </div>

        <div v-show="activeStep === 1" class="step-content">
          <el-upload
            drag
            action="#"
            :http-request="onCustomUpload"
            :limit="1"
            :auto-upload="false"
            :on-change="onFileChange"
            :on-remove="onFileRemove"
            :file-list="fileList"
          >
            <el-icon class="el-icon--upload"><UploadFilled /></el-icon>
            <div class="el-upload__text">Datei hierher ziehen oder <em>klicken</em></div>
            <template #tip>
              <div class="el-upload__tip">Nur eine Datei auswählen.</div>
            </template>
          </el-upload>
        </div>

        <div v-show="activeStep === 2" class="step-content">
          <el-descriptions title="Zusammenfassung" :column="1" border>
            <el-descriptions-item label="Nachweis">{{ skillName(selectedSkillId) || '—' }}</el-descriptions-item>
            <el-descriptions-item label="Datei">{{ fileList[0]?.name || '—' }}</el-descriptions-item>
          </el-descriptions>
          <el-alert v-if="!selectedSkillId || !fileList.length" type="warning" show-icon class="mt-12" title="Bitte Nachweis auswählen und Datei hochladen." />
        </div>

        <div class="step-actions">
          <el-button :disabled="activeStep === 0" @click="prevStep">Zurück</el-button>
          <el-button v-if="activeStep < 2" type="primary" :disabled="nextDisabled" @click="nextStep">Weiter</el-button>
          <el-button v-else type="success" :disabled="!selectedSkillId || !fileList.length" :loading="loading" @click="start">Workflow starten</el-button>
        </div>
      </div>
    </DrawerComponent>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive, computed, watch } from 'vue'
import { ElNotification } from 'element-plus'
import DrawerComponent from '@/components/drawer/DrawerComponent.vue'
import { useSupplyCertificateWorkflows } from './composeables/supplyCertificateWorkflows'
import { UploadFilled } from '@element-plus/icons-vue'

const {
  ownWorkflows,
  reviewableWorkflows,
  skills,
  loading,
  error,
  fetchOwn,
  fetchReviewable,
  fetchSkills,
  startWorkflow,
  cancelWorkflow,
  processWorkflow,
  getFileUrl,
} = useSupplyCertificateWorkflows()

const decisions = reactive<Record<string, { accept: boolean | null, comment: string }>>({})

// Initialize decision state for reviewable workflows to avoid undefined bindings
const initDecisions = (list: any[]) => {
  for (const w of list || []) {
    const id = (w && w.id) as string
    if (id && !decisions[id]) {
      decisions[id] = { accept: null, comment: '' }
    }
  }
}

watch(reviewableWorkflows, (list) => {
  initDecisions(list as any[])
}, { immediate: true })

const loadingSkills = ref(false)

onMounted(async () => {
  await Promise.all([fetchOwn(), fetchReviewable()])
  loadingSkills.value = true
  try { await fetchSkills() } finally { loadingSkills.value = false }
})

const stateText = (state: number) => {
  switch (state) {
    case 1: return 'InReview'
    case 2: return 'Akzeptiert'
    case 3: return 'Abgelehnt'
    case -1: return 'Abgebrochen'
    default: return String(state)
  }
}
const stateTagType = (state: number) => {
  switch (state) {
    case 1: return 'warning'
    case 2: return 'success'
    case 3: return 'danger'
    case -1: return 'info'
    default: return ''
  }
}

const openDrawer = () => {
  resetForm()
  drawerOpen.value = true
}

const drawerOpen = ref(false)
const activeStep = ref(0)
const selectedSkillId = ref<string>('')
const fileList = ref<any[]>([])

const nextDisabled = computed(() => {
  if (activeStep.value === 0) return !selectedSkillId.value
  if (activeStep.value === 1) return !fileList.value.length
  return false
})

const prevStep = () => { if (activeStep.value > 0) activeStep.value-- }
const nextStep = () => { if (activeStep.value < 2) activeStep.value++ }

const onFileChange = (file: any, files: any[]) => { fileList.value = files }
const onFileRemove = (_: any, files: any[]) => { fileList.value = files }
const onCustomUpload = () => { /* handled manually on start */ }

const skillName = (id: string) => skills.value.find(s => s.id === id)?.name

const start = async () => {
  if (!selectedSkillId.value || !fileList.value.length) return
  try {
    await startWorkflow(selectedSkillId.value, fileList.value[0].raw)
    ElNotification({ title: 'Erfolg', message: 'Workflow gestartet', type: 'success' })
    drawerOpen.value = false
    await fetchOwn()
  } catch (e: any) {
    ElNotification({ title: 'Fehler', message: e.message || 'Start fehlgeschlagen', type: 'error' })
  }
}

const handleCancel = async (id: string) => {
  try {
    await cancelWorkflow(id)
    ElNotification({ title: 'Erfolg', message: 'Workflow abgebrochen', type: 'success' })
  } catch (e: any) {
    ElNotification({ title: 'Fehler', message: e.message || 'Abbrechen fehlgeschlagen', type: 'error' })
  }
}

const viewFile = (id: string) => {
  const url = getFileUrl(id)
  window.open(url, '_blank')
}

const handleProcess = async (id: string) => {
  const d = decisions[id] || { accept: null, comment: '' }
  if (d.accept === null) {
    ElNotification({ title: 'Hinweis', message: 'Bitte Entscheidung wählen', type: 'info' })
    return
  }
  try {
    await processWorkflow(id, !!d.accept, d.comment || '')
    ElNotification({ title: 'Erfolg', message: 'Workflow verarbeitet', type: 'success' })
  } catch (e: any) {
    ElNotification({ title: 'Fehler', message: e.message || 'Verarbeitung fehlgeschlagen', type: 'error' })
  }
}

const resetForm = () => {
  activeStep.value = 0
  selectedSkillId.value = ''
  fileList.value = []
}
</script>

<style scoped>
.workflow-admin { padding: 20px; }
.card-header { display: flex; justify-content: space-between; align-items: center; }
.error-alert { margin-bottom: 16px; }
.mt-24 { margin-top: 24px; }
.stepper { margin-top: 4px; }
.step-content { margin: 16px 0; }
.step-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 12px; }
.decision-row { display: flex; align-items: center; }
.ml-8 { margin-left: 8px; }
.mt-12 { margin-top: 12px; }
</style>
