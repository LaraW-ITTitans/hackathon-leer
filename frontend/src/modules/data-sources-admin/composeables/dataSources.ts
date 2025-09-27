import { ref } from 'vue'
import { dataSourceApi, skillApi } from '@/api'
import type {
  DataSourceBindingModel,
  CreateUpdateDataSourceBindingModel,
  BasicSkillBindingModel,
} from '@/api/codegen'

export const useDataSources = () => {
  const dataSources = ref<DataSourceBindingModel[]>([])
  const skills = ref<BasicSkillBindingModel[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchDataSources = async () => {
    loading.value = true
    error.value = null
    try {
      dataSources.value = await dataSourceApi.getDataSources()
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch data sources'
      console.error('Error fetching data sources:', err)
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
      error.value = err.message || 'Failed to fetch skills'
      console.error('Error fetching skills:', err)
    } finally {
      loading.value = false
    }
  }

  const createDataSource = async (payload: CreateUpdateDataSourceBindingModel) => {
    loading.value = true
    error.value = null
    try {
      const created = await dataSourceApi.createDataSource(payload)
      // refresh or push
      await fetchDataSources()
      return created
    } catch (err: any) {
      error.value = err.message || 'Failed to create data source'
      console.error('Error creating data source:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const updateDataSource = async (id: number, payload: CreateUpdateDataSourceBindingModel) => {
    loading.value = true
    error.value = null
    try {
      const updated = await dataSourceApi.updateDataSource(id, payload)
      await fetchDataSources()
      return updated
    } catch (err: any) {
      error.value = err.message || `Failed to update data source with ID ${id}`
      console.error(`Error updating data source with ID ${id}:`, err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const deleteDataSource = async (id: number) => {
    loading.value = true
    error.value = null
    try {
      await dataSourceApi.deleteDataSource(id)
      dataSources.value = dataSources.value.filter(ds => ds.id !== id)
    } catch (err: any) {
      error.value = err.message || `Failed to delete data source with ID ${id}`
      console.error(`Error deleting data source with ID ${id}:`, err)
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    dataSources,
    skills,
    loading,
    error,
    fetchDataSources,
    fetchSkills,
    createDataSource,
    updateDataSource,
    deleteDataSource,
  }
}
