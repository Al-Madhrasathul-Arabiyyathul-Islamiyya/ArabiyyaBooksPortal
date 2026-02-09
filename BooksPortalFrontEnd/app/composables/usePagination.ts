import { PAGINATION } from '~/utils/constants'
import type { PaginationParams } from '~/types/api'

export function usePagination(initialPageSize: number = PAGINATION.defaultPageSize) {
  const page = ref(1)
  const pageSize = ref(initialPageSize)
  const totalRecords = ref(0)

  function onPage(event: { page: number, rows: number }) {
    page.value = event.page + 1 // PrimeVue uses 0-based, API uses 1-based
    pageSize.value = event.rows
  }

  function reset() {
    page.value = 1
  }

  const queryParams = computed<PaginationParams>(() => ({
    pageNumber: page.value,
    pageSize: pageSize.value,
  }))

  return {
    page,
    pageSize,
    totalRecords,
    onPage,
    reset,
    queryParams,
  }
}
