<template>
  <div class="flex flex-col gap-4">
    <Card>
      <template #content>
        <div class="flex flex-wrap items-start justify-between gap-4">
          <div>
            <h2 class="text-xl font-semibold text-surface-900 dark:text-surface-0">
              {{ title }}
            </h2>
            <p
              v-if="subtitle"
              class="text-sm text-surface-600 dark:text-surface-400"
            >
              {{ subtitle }}
            </p>
            <div
              v-if="$slots['header-meta']"
              class="mt-2"
            >
              <slot name="header-meta" />
            </div>
          </div>
          <div class="flex flex-wrap gap-2">
            <slot name="actions" />
          </div>
        </div>

        <div class="mt-4 grid gap-3 md:grid-cols-2 lg:grid-cols-4">
          <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
            <div class="text-xs text-surface-500">
              Reference No
            </div>
            <div class="text-sm font-semibold">
              {{ referenceNo || '-' }}
            </div>
          </div>
          <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
            <div class="text-xs text-surface-500">
              Academic Year
            </div>
            <div class="text-sm font-semibold">
              {{ academicYearName || '-' }}
            </div>
          </div>
          <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
            <div class="text-xs text-surface-500">
              Date
            </div>
            <div class="text-sm font-semibold">
              {{ formattedDate }}
            </div>
          </div>
          <div class="rounded-lg border border-surface-200 p-3 dark:border-surface-700">
            <div class="text-xs text-surface-500">
              Item Count
            </div>
            <div class="text-sm font-semibold">
              {{ items.length }}
            </div>
          </div>
        </div>

        <div
          v-if="notes"
          class="mt-3 rounded-lg border border-surface-200 p-3 text-sm dark:border-surface-700"
        >
          <span class="font-semibold">Notes:</span> {{ notes }}
        </div>
      </template>
    </Card>

    <Card>
      <template #title>
        Slip Items
      </template>
      <template #content>
        <DataTable
          :value="items"
          :loading="loading"
          size="small"
          responsive-layout="scroll"
          data-key="id"
          empty-message="No items available."
        >
          <slot name="items-columns">
            <Column
              v-for="column in columns"
              :key="column.field"
              :field="column.field"
              :header="column.header"
            />
          </slot>
        </DataTable>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
type SlipDetailColumn = {
  field: string
  header: string
}

const props = withDefaults(defineProps<{
  title: string
  subtitle?: string
  referenceNo?: string
  academicYearName?: string
  date?: string
  notes?: string | null
  items: Record<string, unknown>[]
  columns?: SlipDetailColumn[]
  loading?: boolean
}>(), {
  subtitle: '',
  referenceNo: '',
  academicYearName: '',
  date: '',
  notes: null,
  columns: () => [],
  loading: false,
})

const formattedDate = computed(() => {
  if (!props.date) return '-'
  return new Date(props.date).toLocaleString()
})
</script>
