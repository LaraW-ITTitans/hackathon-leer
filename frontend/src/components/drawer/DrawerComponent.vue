<template>
  <el-drawer
    v-model="visible"
    :direction="'rtl'"
    :size="normalizedWidth"
    :with-header="!!title || !!$slots.header"
    :append-to-body="appendToBody"
    :lock-scroll="lockScroll"
    :show-close="showClose"
    :destroy-on-close="destroyOnClose"
    :close-on-click-modal="closeOnClickOverlay"
    :z-index="zIndex"
    @open="emit('open')"
    @opened="emit('opened')"
    @close="emit('close')"
    @closed="emit('closed')"
  >
    <!-- Custom header (optional) -->
    <template #header v-if="$slots.header || title">
      <div class="flex items-center justify-between w-full">
        <div class="text-base font-medium">
          <slot name="header">
            {{ title }}
          </slot>
        </div>
        <slot name="header-extra" />
      </div>
    </template>

    <!-- Main content -->
    <div class="p-2">
      <slot />
    </div>

    <!-- Footer (optional) -->
    <template #footer v-if="$slots.footer">
      <div class="w-full border-t border-gray-100 px-4 py-3">
        <slot name="footer" />
      </div>
    </template>
  </el-drawer>
</template>

<script setup lang="ts">
import {
  computed,
  toRef,
  defineEmits,
  defineExpose
} from 'vue'

const props = defineProps({
  /** v-model */
  modelValue: { type: Boolean, default: false },

  /** Drawer width, e.g. '420px' or 420 (px). */
  width: { type: [String, Number], default: '420px' },

  /** Optional title (you can also use #header slot). */
  title: { type: String, default: '' },

  /** Behavior & style tweaks mirroring el-drawer props */
  appendToBody: { type: Boolean, default: true },
  lockScroll: { type: Boolean, default: true },
  showClose: { type: Boolean, default: true },
  destroyOnClose: { type: Boolean, default: false },
  closeOnClickOverlay: { type: Boolean, default: true },
  zIndex: { type: Number, default: 2000 },
})

const emit = defineEmits<{
  (e: 'update:modelValue', value: boolean): void
  (e: 'open'): void
  (e: 'opened'): void
  (e: 'close'): void
  (e: 'closed'): void
}>()

/** Bridge for v-model */
const visible = computed({
  get: () => props.modelValue,
  set: (val: boolean) => emit('update:modelValue', val),
})

/** Normalize width to a string that el-drawer accepts */
const widthRef = toRef(props, 'width')
const normalizedWidth = computed(() => {
  const w = widthRef.value
  return typeof w === 'number' ? `${w}px` : w
})

/** Public methods */
function open() {
  emit('update:modelValue', true)
}
function close() {
  emit('update:modelValue', false)
}
function toggle() {
  emit('update:modelValue', !props.modelValue)
}

defineExpose({ open, close, toggle })
</script>

<style scoped>
/* Minimal tweaks; rely on Element Plus for most styling */
</style>
