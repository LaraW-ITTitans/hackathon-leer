<template>
  <div class="role-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <span>Rollenverwaltung</span>
          <el-button type="primary" @click="openDrawer()">Neue Rolle erstellen</el-button>
        </div>
      </template>

      <el-input
        v-model="searchQuery"
        placeholder="Search roles by name"
        clearable
        class="search-input"
      />

      <el-alert v-if="error" type="error" :title="error" show-icon class="error-alert" />

      <el-skeleton v-if="loading && !roles.length" :rows="5" animated />

      <el-table v-else :data="paginatedRoles" style="width: 100%" border stripe>
        <el-table-column prop="id" label="ID" />
        <el-table-column prop="name" label="Name" sortable />
        <el-table-column label="Aktionen" width="200">
          <template #default="scope">
            <el-button size="small" @click="openDrawer(scope.row.id)">Edit</el-button>
            <el-popconfirm
              title="Are you sure to delete this role?"
              confirm-button-text="Yes"
              cancel-button-text="No"
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
        v-if="filteredRoles.length > pageSize"
        :page-size="pageSize"
        :current-page="currentPage"
        :page-sizes="[10, 20, 50, 100]"
        :small="false"
        :disabled="loading"
        :background="true"
        layout="total, sizes, prev, pager, next, jumper"
        :total="filteredRoles.length"
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
        ref="roleFormRef"
        :model="roleForm"
        :rules="roleFormRules"
        label-position="top"
        @submit.prevent="handleSubmit"
      >
        <el-form-item label="ID" prop="id" v-if="roleForm.id">
          <el-input v-model="roleForm.id" disabled />
        </el-form-item>
        <el-form-item label="Name" prop="name">
          <el-input v-model="roleForm.name" placeholder="Enter role name" />
        </el-form-item>
        <el-form-item label="Description" prop="description">
          <el-input
            v-model="roleForm.description"
            type="textarea"
            :rows="3"
            placeholder="Enter role description"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="handleSubmit">Save</el-button>
          <el-button @click="formOpen = false">Cancel</el-button>
        </el-form-item>
      </el-form>
    </DrawerComponent>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, reactive } from 'vue'
import { useRouter } from 'vue-router'
import { useRoles } from './composeables/roles'
import { ElNotification, ElMessage } from 'element-plus'
import type { BasicRoleBindingModel } from '@/api/codegen'
import DrawerComponent from '@/components/drawer/DrawerComponent.vue'
import type { FormInstance, FormRules } from 'element-plus'

const router = useRouter();
const { roles, loading, error, fetchRoles, deleteRole, createRole, updateRole } = useRoles()

const searchQuery = ref('')
const currentPage = ref(1)
const pageSize = ref(10)

// 1. Define form data structure
interface RoleForm {
  id: string | null;
  name: string;
  description: string;
}

const roleFormRef = ref<FormInstance>()
const roleForm = reactive<RoleForm>({
  id: null,
  name: '',
  description: '',
})

// 2. Implement form validation rules
const roleFormRules = reactive<FormRules<RoleForm>>({
  name: [
    { required: true, message: 'Please enter role name', trigger: 'blur' },
    { min: 3, message: 'Length should be at least 3', trigger: 'blur' },
  ],
  description: [
    { required: true, message: 'Please enter role description', trigger: 'blur' },
    { min: 5, message: 'Length should be at least 5', trigger: 'blur' },
  ],
})

// Fetch roles when the component is mounted
onMounted(async () => {
  await fetchRoles()
})

// Filter roles based on search query
const filteredRoles = computed(() => {
  const currentRoles = roles.value || [];

  if (!searchQuery.value || searchQuery.value === '') {
    return currentRoles
  }
  const query = searchQuery.value.toLowerCase()
  return currentRoles.filter((role: BasicRoleBindingModel) =>
    role.name?.toLowerCase().includes(query)
  )
})

// Paginate filtered roles
const paginatedRoles = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value;
  const end = start + pageSize.value
  return filteredRoles.value.slice(start, end)
})

// Handle page size change
const handleSizeChange = (val: number) => {
  pageSize.value = val
  currentPage.value = 1 // Reset to first page when page size changes
}

// Handle current page change
const handleCurrentChange = (val: number) => {
  currentPage.value = val
}

// Handle role deletion
const handleDelete = async (id: string) => {
  try {
    await deleteRole(id)
    ElNotification({
      title: 'Success',
      message: 'Role deleted successfully!',
      type: 'success',
    })
  } catch (err: any) {
    ElNotification({
      title: 'Error',
      message: err.message || 'Failed to delete role.',
      type: 'error',
    })
  }
}

const formTitle = computed(() => roleForm.id ? 'Rolle bearbeiten' : 'Neue Rolle erstellen')
const formOpen = ref(false)

// 5. Populate form for editing and 6. Reset form on close
const openDrawer = (entityId?: string) => {
  if (entityId) {
    const roleToEdit = roles.value.find(role => role.id === entityId);
    if (roleToEdit) {
      roleForm.id = roleToEdit.id || null;
      roleForm.name = roleToEdit.name || '';
      roleForm.description = roleToEdit.description || '';
    } else {
      ElMessage.error('Role not found for editing.');
      return;
    }
  } else {
    resetForm(); // Reset for new role creation
  }
  formOpen.value = true;
}

const resetForm = () => {
  roleForm.id = null;
  roleForm.name = '';
  roleForm.description = '';
  roleFormRef.value?.resetFields(); // Reset validation state
}

// 4. Handle form submission
const handleSubmit = async () => {
  if (!roleFormRef.value) return;

  await roleFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        if (roleForm.id) {
          // Update existing role
          await updateRole(roleForm.id, { name: roleForm.name, description: roleForm.description });
          ElNotification({
            title: 'Success',
            message: 'Role updated successfully!',
            type: 'success',
          });
        } else {
          // Create new role
          await createRole({ name: roleForm.name, description: roleForm.description });
          ElNotification({
            title: 'Success',
            message: 'Role created successfully!',
            type: 'success',
          });
        }
        formOpen.value = false;
        await fetchRoles(); // Refresh the list
      } catch (err: any) {
        ElNotification({
          title: 'Error',
          message: err.message || 'Failed to save role.',
          type: 'error',
        });
      }
    } else {
      ElMessage.error('Please correct the errors in the form.');
    }
  });
}
</script>

<style scoped>
.role-list {
  padding: 20px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.search-input {
  margin-bottom: 20px;
  max-width: 400px;
}

.error-alert {
  margin-bottom: 20px;
}

.pagination-container {
  margin-top: 20px;
  justify-content: flex-end;
  display: flex;
}
</style>
