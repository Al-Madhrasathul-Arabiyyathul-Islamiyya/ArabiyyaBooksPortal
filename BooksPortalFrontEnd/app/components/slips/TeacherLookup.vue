<template>
  <Card>
    <template #title>
      <div class="flex items-center justify-between gap-2">
        <span class="text-base">Teacher Lookup</span>
        <Button
          v-if="props.showCreateAction"
          label="Add New Teacher"
          icon="pi pi-user-plus"
          severity="secondary"
          outlined
          size="small"
          @click="emit('create-requested', searchTerm)"
        />
      </div>
    </template>
    <template #content>
      <div class="grid gap-3">
        <InputText
          v-model.trim="searchTerm"
          :placeholder="props.placeholder"
          class="min-w-64 flex-1"
        />

        <DataTable
          :value="teachers"
          :loading="isSearching"
          data-key="id"
          :row-class="getRowClass"
          size="small"
          responsive-layout="scroll"
          paginator
          :rows="pageSize"
          :first="(page - 1) * pageSize"
          :total-records="totalRecords"
          :rows-per-page-options="[10, 20, 50]"
          current-page-report-template="Showing {first} to {last} of {totalRecords}"
          paginator-template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          empty-message="Start typing to find teachers."
          @page="onPageChange"
          @row-click="onRowClick"
        >
          <Column
            field="fullName"
            header="Teacher"
            style="min-width: 14rem;"
          />
          <Column
            field="nationalId"
            header="National ID"
            style="min-width: 10rem;"
          />
          <Column
            field="email"
            header="Email"
            style="min-width: 12rem;"
          />
          <Column
            header="Assignments"
            style="min-width: 7rem;"
          >
            <template #body="{ data }">
              {{ data.assignments?.length ?? 0 }}
            </template>
          </Column>
          <Column
            header="Action"
            :exportable="false"
            style="width: 7rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-1">
                <CommonIconActionButton
                  icon="pi pi-check"
                  tooltip="Select teacher"
                  @click.stop="selectTeacher(data)"
                />
                <CommonIconActionButton
                  icon="pi pi-times"
                  tooltip="Remove / unselect"
                  severity="secondary"
                  :disabled="modelValue?.id !== data.id"
                  @click.stop="clearSelection"
                />
              </div>
            </template>
          </Column>
        </DataTable>

        <Message
          v-if="!isSearching && hasSearched && teachers.length === 0"
          severity="warn"
          :closable="false"
        >
          No teacher found for this search.
        </Message>
      </div>
    </template>
  </Card>
</template>

<script setup lang="ts">
import type { PaginatedList } from '~/types/api'
import type { Teacher } from '~/types/entities'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

const modelValue = defineModel<Teacher | null>({ default: null })

const props = withDefaults(defineProps<{
  placeholder?: string
  showCreateAction?: boolean
}>(), {
  placeholder: 'Search by teacher name or national ID',
  showCreateAction: true,
})

const emit = defineEmits<{
  (e: 'selected', teacher: Teacher): void
  (e: 'cleared'): void
  (e: 'create-requested', query: string): void
}>()

const api = useApi()
const { showError } = useAppToast()

const searchTerm = ref('')
const teachers = ref<Teacher[]>([])
const isSearching = ref(false)
const hasSearched = ref(false)
const page = ref(1)
const pageSize = ref(20)
const totalRecords = ref(0)

async function searchTeachers() {
  isSearching.value = true
  hasSearched.value = true
  try {
    const response = await api.get<PaginatedList<Teacher>>(API.teachers.base, {
      pageNumber: page.value,
      pageSize: pageSize.value,
      search: searchTerm.value || undefined,
    })
    if (response.success) {
      teachers.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to search teachers')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to search teachers'))
  }
  finally {
    isSearching.value = false
  }
}

function selectTeacher(teacher: Teacher) {
  modelValue.value = teacher
  emit('selected', teacher)
}

function clearSelection() {
  modelValue.value = null
  emit('cleared')
}

function getRowClass(teacher: Teacher) {
  if (modelValue.value?.id === teacher.id) {
    return 'bg-green-50 dark:bg-green-950/30 ring-1 ring-green-400'
  }
  return ''
}

function onRowClick(event: { data: Teacher }) {
  selectTeacher(event.data)
}

async function onPageChange(event: { page: number, rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  await searchTeachers()
}

let searchDebounce: ReturnType<typeof setTimeout> | null = null

watch(
  () => searchTerm.value,
  (value) => {
    if (searchDebounce) clearTimeout(searchDebounce)
    searchDebounce = setTimeout(async () => {
      page.value = 1
      if (!value.trim()) {
        teachers.value = []
        totalRecords.value = 0
        hasSearched.value = false
        return
      }
      await searchTeachers()
    }, 300)
  },
)
</script>
