import { ref } from 'vue';
import { roleApi } from '@/api'
import type {
  BasicRoleBindingModel,
  CreateRoleBindingModel,
  UpdateRoleBindingModel
} from '@/api/codegen'

export const useRoles = () => {
  const roles = ref<BasicRoleBindingModel[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  // Fetch all roles
  const fetchRoles = async () => {
    loading.value = true;
    error.value = null;
    try {
      roles.value = await roleApi.getAllRoles();
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch roles';
      console.error('Error fetching roles:', err);
    } finally {
      loading.value = false;
    }
  };

  // Fetch a single role by ID
  const fetchRoleById = async (id: number) => {
    loading.value = true;
    error.value = null;
    try {
      if (!roles.value)
      {
        await fetchRoles();
      }

      roles.value.find(role => role.id == id)
    } catch (err: any) {
      error.value = err.message || `Failed to fetch role with ID ${id}`;
      console.error(`Error fetching role with ID ${id}:`, err);
      return null;
    } finally {
      loading.value = false;
    }
  };

  // Create a new role
  const createRole = async (role: CreateRoleBindingModel) => {
    loading.value = true;
    error.value = null;
    try {
      const response: BasicRoleBindingModel = await roleApi.createRole(role);
      // Optionally, re-fetch roles or add the new role to the list
      await fetchRoles();
      return response;
    } catch (err: any) {
      error.value = err.message || 'Failed to create role';
      console.error('Error creating role:', err);
      throw err; // Re-throw to allow component to handle
    } finally {
      loading.value = false;
    }
  };

  // Update an existing role
  const updateRole = async (id: number, role: UpdateRoleBindingModel) => {
    loading.value = true;
    error.value = null;
    try {
      const response: BasicRoleBindingModel = await roleApi.updateRole(id, role);
      // Optionally, re-fetch roles or update the specific role in the list
      await fetchRoles();
      return response;
    } catch (err: any) {
      error.value = err.message || `Failed to update role with ID ${id}`;
      console.error(`Error updating role with ID ${id}:`, err);
      throw err;
    } finally {
      loading.value = false;
    }
  };

  // Delete a role
  const deleteRole = async (id: string) => {
    loading.value = true;
    error.value = null;

    try {
      await roleApi.deleteRole(id);

      // Remove the deleted role from the list
      roles.value = roles.value.filter(r => r.id !== id);
    } catch (err: any) {
      error.value = err.message || `Failed to delete role with ID ${id}`;
      console.error(`Error deleting role with ID ${id}:`, err);
      throw err;
    } finally {
      loading.value = false;
    }
  };

  return {
    roles,
    loading,
    error,
    fetchRoles,
    fetchRoleById,
    createRole,
    updateRole,
    deleteRole,
  };
}
