import { ref } from 'vue';
import { userApi } from '@/api'
import type {
  BasicUserBindingModel,
  CreateUserBindingModel,
  UpdateUserBindingModel
} from '@/api/codegen'

export const useUsers = () => {
  const users = ref<BasicUserBindingModel[]>([]);
  const loading = ref(false);
  const error = ref<string | null>(null);

  // Fetch all users
  const fetchUsers = async () => {
    loading.value = true;
    error.value = null;
    try {
      users.value = await userApi.getUsers();
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch users';
      console.error('Error fetching users:', err);
    } finally {
      loading.value = false;
    }
  };

  // Fetch a single user by ID
  const fetchUserById = async (id: string) => {
    loading.value = true;
    error.value = null;
    try {
      if (!users.value.length) {
        await fetchUsers();
      }

      return users.value.find(user => user.id === id);
    } catch (err: any) {
      error.value = err.message || `Failed to fetch user with ID ${id}`;
      console.error(`Error fetching user with ID ${id}:`, err);
      return null;
    } finally {
      loading.value = false;
    }
  };

  // Create a new user
  const createUser = async (user: CreateUserBindingModel) => {
    loading.value = true;
    error.value = null;
    try {
      const response: BasicUserBindingModel = await userApi.createUser(user);
      // Re-fetch users to get updated list
      await fetchUsers();
      return response;
    } catch (err: any) {
      error.value = err.message || 'Failed to create user';
      console.error('Error creating user:', err);
      throw err; // Re-throw to allow component to handle
    } finally {
      loading.value = false;
    }
  };

  // Update an existing user
  const updateUser = async (user: UpdateUserBindingModel) => {
    loading.value = true;
    error.value = null;
    try {
      const response: BasicUserBindingModel = await userApi.update_User(user);
      // Re-fetch users to get updated list
      await fetchUsers();
      return response;
    } catch (err: any) {
      error.value = err.message || `Failed to update user with ID ${user.id}`;
      console.error(`Error updating user with ID ${user.id}:`, err);
      throw err;
    } finally {
      loading.value = false;
    }
  };

  // Delete a user
  const deleteUser = async (id: string) => {
    loading.value = true;
    error.value = null;

    try {
      // Note: Assuming there's a deleteUser method in userApi
      // If not available, we might need to check if it exists or handle differently
      if (userApi.deleteUser) {
        await userApi.deleteUser(id);
      } else {
        throw new Error('Delete user functionality not available in API');
      }

      // Remove the deleted user from the list
      users.value = users.value.filter(u => u.id !== id);
    } catch (err: any) {
      error.value = err.message || `Failed to delete user with ID ${id}`;
      console.error(`Error deleting user with ID ${id}:`, err);
      throw err;
    } finally {
      loading.value = false;
    }
  };

  return {
    users,
    loading,
    error,
    fetchUsers,
    fetchUserById,
    createUser,
    updateUser,
    deleteUser,
  };
}
