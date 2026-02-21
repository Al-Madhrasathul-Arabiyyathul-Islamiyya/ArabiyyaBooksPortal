<template>
  <Card>
    <template #title>
      <div class="flex items-center justify-between gap-2">
        <span class="text-base">Parent / Guardian Lookup</span>
        <Button
          v-if="showCreateAction"
          label="Add New Parent"
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
        <Message
          v-if="student"
          severity="info"
          :closable="false"
        >
          Student selected: <strong>{{ student.fullName }}</strong>
        </Message>

        <div class="flex flex-wrap gap-2">
          <InputText
            v-model.trim="searchTerm"
            placeholder="Search parents by name or national ID"
            class="min-w-64 flex-1"
          />
        </div>

        <DataTable
          :value="parents"
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
          empty-message="Start typing to find parents."
          @page="onPageChange"
          @row-click="onRowClick"
        >
          <Column
            field="fullName"
            header="Parent / Guardian"
            style="min-width: 14rem;"
          />
          <Column
            field="nationalId"
            header="National ID"
            style="min-width: 10rem;"
          />
          <Column
            field="phone"
            header="Phone"
            style="min-width: 9rem;"
          />
          <Column
            header="Linked"
            style="min-width: 8rem;"
          >
            <template #body="{ data }">
              <Tag
                v-if="isLinkedToStudent(data.id)"
                severity="info"
                value="Linked"
              />
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
                  tooltip="Select parent"
                  @click.stop="selectParent(data)"
                />
                <CommonIconActionButton
                  icon="pi pi-times"
                  tooltip="Remove / unselect"
                  severity="secondary"
                  :disabled="modelValue !== data.id"
                  @click.stop="clearSelection"
                />
              </div>
            </template>
          </Column>
        </DataTable>

        <Message
          v-if="!isSearching && hasSearched && parents.length === 0"
          severity="warn"
          :closable="false"
        >
          No parent found for this search.
        </Message>

        <Message
          v-if="errorMessage"
          severity="error"
          :closable="false"
        >
          {{ errorMessage }}
        </Message>
      </div>
    </template>
  </Card>
</template>

<script setup lang="ts">
import type { PaginatedList } from '~/types/api'
import type { Parent, Student } from '~/types/entities'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

const modelValue = defineModel<number | null>({ default: null })

const props = withDefaults(defineProps<{
  student?: Student | null
  errorMessage?: string
  showCreateAction?: boolean
}>(), {
  student: null,
  errorMessage: '',
  showCreateAction: true,
})

const emit = defineEmits<{
  (e: 'selected', parent: Parent): void
  (e: 'cleared'): void
  (e: 'create-requested', query: string): void
}>()

const api = useApi()
const { showError } = useAppToast()

const searchTerm = ref('')
const parents = ref<Parent[]>([])
const isSearching = ref(false)
const hasSearched = ref(false)
const page = ref(1)
const pageSize = ref(20)
const totalRecords = ref(0)

function isLinkedToStudent(parentId: number) {
  return !!props.student?.parents.some(parent => parent.parentId === parentId)
}

function toParentEntityFromLinked(parentId: number): Parent | null {
  const linked = props.student?.parents.find(parent => parent.parentId === parentId)
  if (!linked) return null
  return {
    id: linked.parentId,
    fullName: linked.fullName,
    nationalId: linked.nationalId,
    phone: linked.phone,
    relationship: linked.relationship,
  }
}

function autoSelectLinkedParent() {
  const linkedParents = props.student?.parents ?? []
  if (linkedParents.length === 0) return

  const primary = linkedParents.find(parent => parent.isPrimary) ?? linkedParents[0]
  if (!primary) return

  modelValue.value = primary.parentId
  const mapped = toParentEntityFromLinked(primary.parentId)
  if (mapped) {
    emit('selected', mapped)
    if (!searchTerm.value.trim()) {
      searchTerm.value = mapped.fullName
    }
  }
}

async function searchParents() {
  isSearching.value = true
  hasSearched.value = true
  try {
    const response = await api.get<PaginatedList<Parent>>(API.parents.base, {
      pageNumber: page.value,
      pageSize: pageSize.value,
      search: searchTerm.value || undefined,
    })
    if (response.success) {
      parents.value = response.data.items
      totalRecords.value = response.data.totalCount
      return
    }
    showError(response.message ?? 'Failed to search parents')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to search parents'))
  }
  finally {
    isSearching.value = false
  }
}

function selectParent(parent: Parent) {
  modelValue.value = parent.id
  emit('selected', parent)
}

function clearSelection() {
  modelValue.value = null
  emit('cleared')
}

function onRowClick(event: { data: Parent }) {
  selectParent(event.data)
}

function getRowClass(parent: Parent) {
  if (modelValue.value === parent.id) {
    return 'bg-green-50 dark:bg-green-950/30 ring-1 ring-green-400'
  }
  return ''
}

async function onPageChange(event: { page: number, rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  await searchParents()
}

let parentSearchDebounce: ReturnType<typeof setTimeout> | null = null

watch(
  () => searchTerm.value,
  (value) => {
    if (parentSearchDebounce) clearTimeout(parentSearchDebounce)
    parentSearchDebounce = setTimeout(async () => {
      page.value = 1
      if (!value.trim()) {
        parents.value = []
        totalRecords.value = 0
        hasSearched.value = false
        return
      }
      await searchParents()
    }, 300)
  },
)

watch(
  () => props.student?.id,
  () => {
    autoSelectLinkedParent()
  },
  { immediate: true },
)
</script>
