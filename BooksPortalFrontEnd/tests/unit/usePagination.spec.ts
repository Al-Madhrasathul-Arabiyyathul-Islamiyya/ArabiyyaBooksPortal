import { describe, expect, it } from 'vitest'
import { usePagination } from '~/composables/usePagination'

describe('usePagination', () => {
  it('translates PrimeVue page event (0-based) to API query (1-based)', () => {
    const pagination = usePagination(20)

    pagination.onPage({ page: 2, rows: 50 })

    expect(pagination.page.value).toBe(3)
    expect(pagination.pageSize.value).toBe(50)
    expect(pagination.queryParams.value).toEqual({
      pageNumber: 3,
      pageSize: 50,
    })
  })

  it('resets page to first page while preserving page size', () => {
    const pagination = usePagination(25)
    pagination.onPage({ page: 3, rows: 25 })

    pagination.reset()

    expect(pagination.page.value).toBe(1)
    expect(pagination.pageSize.value).toBe(25)
  })
})
