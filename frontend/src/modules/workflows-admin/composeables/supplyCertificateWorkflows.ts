import { ref } from 'vue'
import axiosInstance from '@/api/axios'
import { certificateWorkflowApi, skillApi } from '@/api'
import type { BasicSupplyCertificateWorkflowBindingModel, BasicSkillBindingModel, ProcessSupplyCertificateWorkflowBindingModel } from '@/api/codegen'

export const useSupplyCertificateWorkflows = () => {
  const ownWorkflows = ref<BasicSupplyCertificateWorkflowBindingModel[]>([])
  const reviewableWorkflows = ref<BasicSupplyCertificateWorkflowBindingModel[]>([])
  const skills = ref<BasicSkillBindingModel[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchOwn = async () => {
    loading.value = true
    error.value = null
    try {
      ownWorkflows.value = await certificateWorkflowApi.getOwnSupplyCertificateWorkflows()
    } catch (err: any) {
      error.value = err.message || 'Fehler beim Laden eigener Workflows'
      console.error('fetchOwn error', err)
    } finally {
      loading.value = false
    }
  }

  const fetchReviewable = async () => {
    loading.value = true
    error.value = null
    try {
      reviewableWorkflows.value = await certificateWorkflowApi.getReviewableSupplyCertificateWorkflows()
    } catch (err: any) {
      error.value = err.message || 'Fehler beim Laden prüfbarer Workflows'
      console.error('fetchReviewable error', err)
    } finally {
      loading.value = false
    }
  }

  const fetchSkills = async () => {
    loading.value = true
    error.value = null
    try {
      skills.value = await skillApi.get_All_Skills()
    } catch (err: any) {
      error.value = err.message || 'Fehler beim Laden der Nachweise'
      console.error('fetchSkills error', err)
    } finally {
      loading.value = false
    }
  }

  const startWorkflow = async (skillId: string, file: File) => {
    loading.value = true
    error.value = null
    try {
      // codegen expects FileParameter; it extracts name internally when passing File directly as data.
      const created = await certificateWorkflowApi.startSupplyCertificateWorkflow(skillId, { data: file, fileName: file.name } as any)
      await fetchOwn()
      return created
    } catch (err: any) {
      error.value = err.message || 'Fehler beim Starten des Workflows'
      console.error('startWorkflow error', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const cancelWorkflow = async (id: string) => {
    loading.value = true
    error.value = null
    try {
      // Workaround: codegen is missing id parameter for cancel; call via axios directly
      const res = await axiosInstance.post(`/api/workflows/supply-certificates/${id}/cancel`)
      await fetchOwn()
      return res.data
    } catch (err: any) {
      error.value = err.message || 'Fehler beim Abbrechen des Workflows'
      console.error('cancelWorkflow error', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const processWorkflow = async (id: string, accept: boolean, reviewComment: string) => {
    loading.value = true
    error.value = null
    try {
      const payload: ProcessSupplyCertificateWorkflowBindingModel = { accept, reviewComment }
      // Workaround: codegen is missing id parameter for process; call via axios directly
      const res = await axiosInstance.post(`/api/workflows/supply-certificates/${id}/process`, payload)
      await Promise.all([fetchReviewable(), fetchOwn()])
      return res.data
    } catch (err: any) {
      error.value = err.message || 'Fehler beim Verarbeiten des Workflows'
      console.error('processWorkflow error', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const getFileUrl = (id: string) => {
    // Endpoint to download/view the file
    return `/api/workflows/supply-certificates/${id}/file`
  }

  const fetchFileBlob = async (id: string) => {
    try {
      const res = await axiosInstance.get(getFileUrl(id), { responseType: 'blob' })
      const contentType = (res.headers?.['content-type'] as string) || ''
      const disposition = (res.headers?.['content-disposition'] as string) || ''
      let fileName = ''
      const match = /filename\*=UTF-8''([^;]+)|filename="?([^";]+)"?/i.exec(disposition)
      if (match) {
        fileName = decodeURIComponent(match[1] || match[2] || '')
      }
      // Fallback from blob type or id
      if (!fileName) {
        const ext = contentType.split('/')[1]
        fileName = `datei-${id}${ext ? '.' + ext : ''}`
      }
      const blob: Blob = res.data
      const url = URL.createObjectURL(blob)
      return { blob, url, contentType, fileName }
    } catch (err) {
      console.error('fetchFileBlob error', err)
      throw err
    }
  }

  return {
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
    fetchFileBlob,
  }
}
