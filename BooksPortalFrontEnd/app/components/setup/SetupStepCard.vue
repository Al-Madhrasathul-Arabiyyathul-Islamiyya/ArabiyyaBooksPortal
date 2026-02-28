<template>
  <div class="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-surface-200 p-3 dark:border-surface-700">
    <div class="grid gap-1">
      <div class="flex items-center gap-2">
        <i
          class="pi text-sm"
          :class="step.isComplete ? 'pi-check-circle text-green-500' : 'pi-clock text-amber-500'"
        />
        <span class="font-semibold">{{ step.title }}</span>
      </div>
      <div class="text-sm text-surface-600 dark:text-surface-300">
        {{ step.hint || 'No additional instructions.' }}
      </div>
      <div
        v-if="step.completedAtUtc"
        class="text-xs text-surface-500"
      >
        Completed: {{ new Date(step.completedAtUtc).toLocaleString() }}
      </div>
    </div>

    <div class="flex flex-wrap items-center justify-end gap-2">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup lang="ts">
import type { SetupStep } from '~/types/setup'

defineProps<{
  step: SetupStep
}>()
</script>
