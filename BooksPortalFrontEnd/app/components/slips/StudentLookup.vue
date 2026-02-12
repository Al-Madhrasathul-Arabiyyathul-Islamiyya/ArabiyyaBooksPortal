<template>
  <Card>
    <template #title>
      <div class="flex items-center justify-between gap-2">
        <span class="text-base">Student Lookup</span>
        <Button
          v-if="props.showCreateAction"
          label="Add New Student"
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
        <div class="flex flex-wrap gap-2">
          <InputText
            v-model.trim="searchTerm"
            :placeholder="props.placeholder"
            class="min-w-64 flex-1"
          />
        </div>

        <DataTable
          :value="students"
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
          empty-message="Start typing to find students."
          @page="onPageChange"
          @row-click="onRowClick"
        >
          <Column
            field="fullName"
            header="Student"
            style="min-width: 14rem;"
          />
          <Column
            field="indexNo"
            header="Index No"
            style="min-width: 8rem;"
          />
          <Column
            field="classSectionDisplayName"
            header="Class"
            style="min-width: 10rem;"
          />
          <Column
            field="nationalId"
            header="National ID"
            style="min-width: 10rem;"
          />
          <Column
            header="Action"
            :exportable="false"
            style="width: 7rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-1">
                <CommonIconActionButton
                  icon="pi pi-check"
                  tooltip="Select student"
                  @click.stop="selectStudent(data)"
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
          v-if="!isSearching && hasSearched && students.length === 0"
          severity="warn"
          :closable="false"
        >
          No student found for this search.
        </Message>
      </div>
    </template>
  </Card>
</template>

<script setup lang="ts">
import type { PaginatedList } from '~/types/api'
import type { Student } from '~/types/entities'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

const modelValue = defineModel<Student | null>({ default: null })

const props = withDefaults(defineProps<{
  placeholder?: string
  showCreateAction?: boolean
  academicYearId?: number | null
}>(), {
  placeholder: 'Search by student name or index number',
  showCreateAction: true,
  academicYearId: null,
})

const emit = defineEmits<{
  (e: 'selected', student: Student): void
  (e: 'cleared'): void
  (e: 'create-requested', query: string): void
}>()

const api = useApi()
const { showError } = useAppToast()

const searchTerm = ref('')
const students = ref<Student[]>([])
const isSearching = ref(false)
const hasSearched = ref(false)
const page = ref(1)
const pageSize = ref(20)
const totalRecords = ref(0)

async function searchStudents() {
  isSearching.value = true
  hasSearched.value = true
  try {
    const response = await api.get<PaginatedList<Student>>(API.students.base, {
      pageNumber: page.value,
      pageSize: pageSize.value,
      academicYearId: props.academicYearId ?? undefined,
      search: searchTerm.value || undefined,
    })
    if (response.success) {
      students.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to search students')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to search students'))
  }
  finally {
    isSearching.value = false
  }
}

function selectStudent(student: Student) {
  modelValue.value = student
  emit('selected', student)
}

function clearSelection() {
  modelValue.value = null
  emit('cleared')
}

function getRowClass(student: Student) {
  if (modelValue.value?.id === student.id) {
    return 'bg-green-50 dark:bg-green-950/30 ring-1 ring-green-400'
  }
  return ''
}

function onRowClick(event: { data: Student }) {
  selectStudent(event.data)
}

async function onPageChange(event: { page: number, rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  await searchStudents()
}

let searchDebounce: ReturnType<typeof setTimeout> | null = null

watch(
  () => searchTerm.value,
  (value) => {
    if (searchDebounce) clearTimeout(searchDebounce)
    searchDebounce = setTimeout(async () => {
      page.value = 1
      if (!value.trim()) {
        students.value = []
        totalRecords.value = 0
        hasSearched.value = false
        return
      }
      await searchStudents()
    }, 300)
  },
)

watch(
  () => props.academicYearId,
  async () => {
    page.value = 1
    if (searchTerm.value.trim()) {
      await searchStudents()
      return
    }
    students.value = []
    totalRecords.value = 0
  },
)
</script>
