import { ref } from 'vue'
import { skillApi } from '@/api'
import type {
  BasicSkillBindingModel,
  CreateSkillBindingModel,
  UpdateSkillBindingModel,
} from '@/api/codegen'

export const useSkills = () => {
  const skills = ref<BasicSkillBindingModel[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  const fetchSkills = async () => {
    loading.value = true
    error.value = null
    try {
      // codegen method name is get_All_Skills
      skills.value = await skillApi.get_All_Skills()
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch skills'
      console.error('Error fetching skills:', err)
    } finally {
      loading.value = false
    }
  }

  const fetchSkillById = async (id: number) => {
    loading.value = true
    error.value = null
    try {
      if (!skills.value?.length) {
        await fetchSkills()
      }
      return skills.value.find((s) => s.id === id) ?? null
    } catch (err: any) {
      error.value = err.message || `Failed to fetch skill with ID ${id}`
      console.error(`Error fetching skill with ID ${id}:`, err)
      return null
    } finally {
      loading.value = false
    }
  }

  const createSkill = async (model: CreateSkillBindingModel) => {
    loading.value = true
    error.value = null
    try {
      const response: BasicSkillBindingModel = await skillApi.createSkill(model)
      // Refresh list to stay in sync
      await fetchSkills()
      return response
    } catch (err: any) {
      error.value = err.message || 'Failed to create skill'
      console.error('Error creating skill:', err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const updateSkill = async (id: number, model: UpdateSkillBindingModel) => {
    loading.value = true
    error.value = null
    try {
      const response: BasicSkillBindingModel = await skillApi.updateSkill({ id, ...model })
      await fetchSkills()
      return response
    } catch (err: any) {
      error.value = err.message || `Failed to update skill with ID ${id}`
      console.error(`Error updating skill with ID ${id}:`, err)
      throw err
    } finally {
      loading.value = false
    }
  }

  const deleteSkill = async (id: number | string) => {
    loading.value = true
    error.value = null
    try {
      await skillApi.deleteSkill(id)
      skills.value = skills.value.filter((s) => s.id !== id)
    } catch (err: any) {
      error.value = err.message || `Failed to delete skill with ID ${id}`
      console.error(`Error deleting skill with ID ${id}:`, err)
      throw err
    } finally {
      loading.value = false
    }
  }

  return {
    skills,
    loading,
    error,
    fetchSkills,
    fetchSkillById,
    createSkill,
    updateSkill,
    deleteSkill,
  }
}
