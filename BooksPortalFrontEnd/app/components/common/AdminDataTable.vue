<template>
  <DataTable
    v-bind="resolvedAttrs"
    :class="['admin-data-table', attrs.class]"
  >
    <template
      v-for="(_, slotName) in $slots"
      #[slotName]="slotProps"
    >
      <slot
        :name="slotName"
        v-bind="slotProps ?? {}"
      />
    </template>
  </DataTable>
</template>

<script setup lang="ts">
import { computed, useAttrs } from 'vue'

const attrs = useAttrs()

const resolvedAttrs = computed(() => {
  const next = { ...attrs }

  if (next.scrollable === undefined) {
    next.scrollable = true
  }

  if (next.scrollHeight === undefined) {
    next.scrollHeight = 'flex'
  }

  return next
})
</script>
