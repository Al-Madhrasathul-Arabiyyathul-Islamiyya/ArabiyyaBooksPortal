import type { AcademicYear } from '~/types/entities'
import { API } from '~/utils/constants'

const ACADEMIC_YEAR_KEY = Symbol('academicYear') as InjectionKey<Ref<AcademicYear | null>>

export function provideAcademicYear() {
  const activeYear = ref<AcademicYear | null>(null)
  const isLoading = ref(false)

  async function fetchActiveYear() {
    const api = useApi()
    isLoading.value = true
    try {
      const response = await api.get<AcademicYear>(API.academicYears.active)
      if (response.success) {
        activeYear.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch active academic year:', error)
    }
    finally {
      isLoading.value = false
    }
  }

  provide(ACADEMIC_YEAR_KEY, activeYear)

  return {
    activeYear,
    isLoading,
    fetchActiveYear,
  }
}

export function useAcademicYear() {
  const activeYear = inject(ACADEMIC_YEAR_KEY)
  if (!activeYear) {
    throw new Error('useAcademicYear must be used within a component that calls provideAcademicYear')
  }
  return activeYear
}
