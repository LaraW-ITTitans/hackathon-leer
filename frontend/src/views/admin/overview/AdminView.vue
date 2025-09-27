<template>
  <div class="admin-overview">
    <!-- Toolbar -->
    <div class="toolbar">
      <el-input
        v-model="query"
        placeholder="Bereiche durchsuchen.."
        clearable
        :prefix-icon="Search"
        class="search-input"
      />
      <el-select v-model="sortBy" placeholder="Sortierung" class="sort-select">
        <el-option label="A → Z" value="az" />
        <el-option label="Z → A" value="za" />
      </el-select>
    </div>

    <!-- Tiles Grid -->
    <el-row :gutter="16" class="grid">
      <el-col
        v-for="tile in filteredTiles"
        :key="tile.key"
        :xs="24"
        :sm="12"
        :md="8"
        :lg="6"
        :xl="6"
      >
        <router-link
          :to="computeTo(tile)"
          class="tile-link"
          custom
          v-slot="{ navigate }"
        >
          <el-card
            shadow="hover"
            class="tile"
            :body-style="{ padding: '16px' }"
            @click="navigate"
            @keydown.enter.prevent="navigate()"
            tabindex="0"
            role="link"
            :aria-label="`${tile.title} – ${tile.description}`"
          >
            <div class="tile-inner">
              <div class="tile-icon">
                <component :is="iconComponent(tile.icon)" class="icon" />
              </div>
              <div class="tile-content">
                <h3 class="tile-title">{{ tile.title }}</h3>
                <p class="tile-desc">{{ tile.description }}</p>
              </div>
            </div>
          </el-card>
        </router-link>
      </el-col>
    </el-row>

    <div v-if="filteredTiles.length === 0" class="empty">
      <el-empty description="Keine Treffer" />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import {
  User,
  Document,
  Star,
  Avatar,
  Search
} from '@element-plus/icons-vue'

/** Tile model */
type Tile = {
  key: string
  title: string
  description: string
  /** icon name from the map below */
  icon:
    | 'User'
    | 'Document'
    | 'Star'
    | 'Avatar'
  /** relative route under the admin prefix, e.g. "users" => "/admin/users" */
  path: string
}

/**
 * Props
 * - tiles: optional custom tiles; defaults provided
 * - routePrefix: defaults to "/admin"
 */
const props = withDefaults(
  defineProps<{
    tiles?: Tile[]
    routePrefix?: string
  }>(),
  {
    routePrefix: '/admin',
  }
)

// Fallback demo tiles (only used when no tiles prop is passed)
const defaultTiles: Tile[] = [
  {
    key: 'users',
    title: 'Benutzer',
    description: 'Verwalte Accounts',
    icon: 'User',
    path: 'users',
  },
  {
    key: 'roles',
    title: 'Rollen',
    description: 'Verwalte Rollen',
    icon: 'Avatar',
    path: 'roles',
  },
  {
    key: 'sources',
    title: 'Datenquellen',
    description: 'Verwalte Quellen',
    icon: 'Document',
    path: 'data-sources',
  },
  {
    key: 'skills',
    title: 'Nachweise',
    description: 'Verwalte Nachweise, Zertifikate, etc.',
    icon: 'Star',
    path: 'skills',
  },
  {
    key: 'workflows',
    title: 'Workflows',
    description: 'Nachweise beantragen und prüfen',
    icon: 'Document',
    path: 'workflows',
  },
]

const tiles = computed<Tile[]>(() => props.tiles?.length ? props.tiles : defaultTiles)

const router = useRouter()
const query = ref('')
const sortBy = ref<'az' | 'za'>('az')

const filteredTiles = computed(() => {
  const q = query.value.trim().toLowerCase()
  const base = tiles.value.filter((t) =>
    !q ||
    t.title.toLowerCase().includes(q) ||
    t.description.toLowerCase().includes(q)
  )
  const sorted = base.sort((a, b) => a.title.localeCompare(b.title))
  return sortBy.value === 'az' ? sorted : [...sorted].reverse()
})

function computeTo(tile: Tile) {
  // Ensure single slash between prefix and path
  const prefix = props.routePrefix!.replace(/\/$/, '')
  const sub = tile.path.replace(/^\//, '')
  return `${prefix}/${sub}`
}

// Map string keys to icon components
const ICONS = {
  User,
  Document,
  Star,
  Avatar,
}

function iconComponent(name: keyof typeof ICONS) {
  return ICONS[name] || Setting
}
</script>

<style scoped>
.admin-overview {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.toolbar {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
}

.search-input {
  flex: 1 1 280px;
}

.sort-select {
  width: 160px;
}

.grid {
  margin-top: 4px;
}

.tile-link {
  text-decoration: none;
}

.tile {
  border-radius: 14px;
  transition: transform 0.15s ease, box-shadow 0.15s ease;
  cursor: pointer;
}

.tile:focus-visible {
  outline: 2px solid var(--el-color-primary);
  outline-offset: 2px;
}

.tile:hover {
  transform: translateY(-2px);
}

.tile-inner {
  display: grid;
  grid-template-columns: 48px 1fr;
  gap: 12px;
  align-items: center;
}

.tile-icon {
  width: 48px;
  height: 48px;
  display: grid;
  place-items: center;
  border-radius: 12px;
  background: var(--el-fill-color-light);
}

.icon {
  width: 24px;
  height: 24px;
}

.tile-title {
  margin: 0 0 4px;
  font-size: 16px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.tile-desc {
  margin: 0;
  font-size: 13px;
  line-height: 1.4;
  color: var(--el-text-color-regular);
}

.empty {
  margin-top: 24px;
}
</style>
