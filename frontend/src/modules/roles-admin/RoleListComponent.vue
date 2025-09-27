<template>
  <div class="role-list">
    <el-card class="box-card">
      <template #header>
        <div class="card-header">
          <span>Rollenverwaltung</span>
          <el-button type="primary" @click="goToCreateRole">Neue Rolle erstellen</el-button>
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
            <el-button size="small" @click="goToEditRole(scope.row.id)">Edit</el-button>
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
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useRoles } from './composeables/roles'
import { ElNotification } from 'element-plus'
import type { BasicRoleBindingModel } from '@/api/codegen'

const router = useRouter();
const { roles, loading, error, fetchRoles, deleteRole } = useRoles()

const searchQuery = ref('')
const currentPage = ref(1)
const pageSize = ref(10)

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

// Navigate to create role page
const goToCreateRole = () => {
  router.push({ name: 'RoleCreate' })
}

// Navigate to edit role page
const goToEditRole = (id: number) => {
  router.push({ name: 'RoleEdit', params: { id } })
}

// Handle role deletion
const handleDelete = async (id: string) => {

  console.log("row scope lalala", id)
  //console.log("row scope lalala", id.row)
  //console.log("row scope lalala", id.row.id)

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
