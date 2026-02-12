<template>
  <div class="grid gap-4">
    <Card>
      <template #content>
        <div class="flex flex-wrap gap-2">
          <InputText
            v-model.trim="searchTerm"
            placeholder="Search books by code or title"
            class="min-w-64 flex-1"
            @keyup.enter="searchBooks"
          />
          <Button
            label="Search"
            icon="pi pi-search"
            :loading="isSearching"
            @click="searchBooks"
          />
        </div>
      </template>
    </Card>

    <Card>
      <template #title>
        Search Results
      </template>
      <template #content>
        <DataTable
          :value="results"
          :loading="isSearching"
          size="small"
          responsive-layout="scroll"
          data-key="id"
          empty-message="Start typing to find books."
        >
          <Column
            field="code"
            header="Code"
            style="min-width: 8rem;"
          />
          <Column
            field="title"
            header="Title"
            style="min-width: 14rem;"
          />
          <Column
            header="Available"
            style="min-width: 9rem;"
          >
            <template #body="{ data }">
              <BooksStockBadge
                :available="data.available"
                :show-label="false"
              />
            </template>
          </Column>
          <Column
            header="Action"
            :exportable="false"
            style="width: 6rem;"
          >
            <template #body="{ data }">
              <Button
                icon="pi pi-plus"
                text
                rounded
                size="small"
                :disabled="isSelected(data.id) || data.available <= 0"
                @click="addBook(data)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <Card>
      <template #title>
        Selected Books
      </template>
      <template #content>
        <DataTable
          :value="modelValue"
          data-key="id"
          size="small"
          responsive-layout="scroll"
          empty-message="No books selected."
        >
          <Column
            field="code"
            header="Code"
            style="min-width: 8rem;"
          />
          <Column
            field="title"
            header="Title"
            style="min-width: 14rem;"
          />
          <Column
            header="Available"
            style="min-width: 9rem;"
          >
            <template #body="{ data }">
              <BooksStockBadge
                :available="data.available"
                :show-label="false"
              />
            </template>
          </Column>
          <Column
            header="Quantity"
            style="min-width: 10rem;"
          >
            <template #body="{ data }">
              <div class="max-w-40">
                <InputNumber
                  :model-value="data.quantity"
                  :min="1"
                  :max="Math.max(1, data.available)"
                  :use-grouping="false"
                  mode="decimal"
                  show-buttons
                  fluid
                  style="width: 6rem"
                  @update:model-value="(value) => updateQuantity(data.id, Number(value ?? 1))"
                />
              </div>
            </template>
          </Column>
          <Column
            header="Action"
            :exportable="false"
            style="width: 6rem;"
          >
            <template #body="{ data }">
              <Button
                icon="pi pi-trash"
                text
                rounded
                severity="danger"
                size="small"
                @click="removeBook(data.id)"
              />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import type { PaginatedList } from '~/types/api'
import type { Book } from '~/types/entities'
import { API } from '~/utils/constants'

export interface SelectedBook {
  id: number
  title: string
  code: string
  available: number
  quantity: number
}

const modelValue = defineModel<SelectedBook[]>({ default: () => [] })

const props = withDefaults(defineProps<{
  academicYearId?: number | null
}>(), {
  academicYearId: null,
})

const api = useApi()
const { showError } = useAppToast()

const searchTerm = ref('')
const isSearching = ref(false)
const results = ref<Book[]>([])

function isSelected(bookId: number) {
  return modelValue.value.some(book => book.id === bookId)
}

function addBook(book: Book) {
  if (isSelected(book.id)) return
  if (book.available <= 0) {
    showError('This book has no available stock.')
    return
  }
  modelValue.value = [
    ...modelValue.value,
    {
      id: book.id,
      title: book.title,
      code: book.code,
      available: book.available,
      quantity: 1,
    },
  ]
}

function removeBook(bookId: number) {
  modelValue.value = modelValue.value.filter(book => book.id !== bookId)
}

function updateQuantity(bookId: number, quantity: number) {
  modelValue.value = modelValue.value.map((book) => {
    if (book.id !== bookId) return book
    const normalized = Number.isFinite(quantity) ? quantity : 1
    const bounded = Math.max(1, Math.min(Math.max(1, book.available), normalized))
    return { ...book, quantity: bounded }
  })
}

async function searchBooks() {
  if (!searchTerm.value) {
    results.value = []
    return
  }
  isSearching.value = true
  try {
    const response = await api.get<Book[] | PaginatedList<Book>>(API.books.search, {
      q: searchTerm.value,
      academicYearId: props.academicYearId ?? undefined,
      page: 1,
      pageSize: 20,
    })
    if (!response.success) {
      showError(response.message ?? 'Failed to search books')
      return
    }
    if (Array.isArray(response.data)) {
      results.value = response.data
      return
    }
    results.value = response.data.items
  }
  catch {
    showError('Failed to search books')
  }
  finally {
    isSearching.value = false
  }
}

let searchDebounce: ReturnType<typeof setTimeout> | null = null

watch(
  () => searchTerm.value,
  (value) => {
    if (searchDebounce) clearTimeout(searchDebounce)
    searchDebounce = setTimeout(async () => {
      if (!value.trim()) {
        results.value = []
        return
      }
      await searchBooks()
    }, 300)
  },
)

watch(
  () => props.academicYearId,
  () => {
    results.value = []
  },
)
</script>
