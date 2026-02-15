<template>
  <Card>
    <template #content>
      <div class="grid gap-3 md:grid-cols-12">
        <div class="md:col-span-10">
          <slot />
        </div>
        <div class="md:col-span-2 flex items-end justify-end gap-2">
          <Button
            icon="pi pi-search"
            label="Apply"
            @click="emit('apply')"
          />
          <Button
            icon="pi pi-refresh"
            severity="secondary"
            outlined
            @click="emit('reset')"
          />
        </div>
      </div>
    </template>
  </Card>
</template>

<script setup lang="ts">
const emit = defineEmits<{
  apply: []
  reset: []
}>()
</script>
