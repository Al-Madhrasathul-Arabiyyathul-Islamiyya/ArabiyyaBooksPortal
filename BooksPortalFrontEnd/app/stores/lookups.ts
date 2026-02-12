import { useQuery, useQueryCache } from '@pinia/colada'
import type { Lookup } from '~/types/entities'
import { API } from '~/utils/constants'
import {
  termLabels,
  conditionLabels,
  movementTypeLabels,
  teacherIssueStatusLabels,
  slipTypeLabels,
} from '~/utils/formatters'

export const useLookupsStore = defineStore('lookups', () => {
  const api = useApi()
  const queryCache = useQueryCache()
  const scopedGrades = ref<Lookup[] | null>(null)

  async function fetchLookup(path: string, fallbackMessage: string) {
    const response = await api.get<Lookup[]>(path)
    if (!response.success) {
      throw new Error(response.message ?? fallbackMessage)
    }
    return response.data
  }

  const academicYearsQuery = useQuery({
    key: () => ['lookups', 'academic-years'],
    query: () => fetchLookup(API.lookups.academicYears, 'Failed to fetch academic years'),
  })

  const keystagesQuery = useQuery({
    key: () => ['lookups', 'keystages'],
    query: () => fetchLookup(API.lookups.keystages, 'Failed to fetch keystages'),
  })

  const subjectsQuery = useQuery({
    key: () => ['lookups', 'subjects'],
    query: () => fetchLookup(API.lookups.subjects, 'Failed to fetch subjects'),
  })

  const classSectionsQuery = useQuery({
    key: () => ['lookups', 'class-sections'],
    query: () => fetchLookup(API.lookups.classSections, 'Failed to fetch class sections'),
  })

  const termsQuery = useQuery({
    key: () => ['lookups', 'terms'],
    query: () => fetchLookup(API.lookups.terms, 'Failed to fetch terms'),
  })

  const bookConditionsQuery = useQuery({
    key: () => ['lookups', 'book-conditions'],
    query: () => fetchLookup(API.lookups.bookConditions, 'Failed to fetch book conditions'),
  })

  const movementTypesQuery = useQuery({
    key: () => ['lookups', 'movement-types'],
    query: () => fetchLookup(API.lookups.movementTypes, 'Failed to fetch movement types'),
  })

  const gradesQuery = useQuery({
    key: () => ['lookups', 'grades', 'all'],
    query: () => fetchLookup(API.lookups.grades, 'Failed to fetch grades'),
  })

  const academicYears = computed(() => academicYearsQuery.data.value ?? [])
  const keystages = computed(() => keystagesQuery.data.value ?? [])
  const subjects = computed(() => subjectsQuery.data.value ?? [])
  const classSections = computed(() => classSectionsQuery.data.value ?? [])
  const terms = computed(() => termsQuery.data.value ?? [])
  const bookConditions = computed(() => bookConditionsQuery.data.value ?? [])
  const movementTypes = computed(() => movementTypesQuery.data.value ?? [])

  const grades = computed(() => {
    if (scopedGrades.value) {
      return scopedGrades.value
    }
    return gradesQuery.data.value ?? []
  })

  const isLoading = computed(() =>
    academicYearsQuery.isPending.value
    || keystagesQuery.isPending.value
    || gradesQuery.isPending.value
    || subjectsQuery.isPending.value
    || classSectionsQuery.isPending.value
    || termsQuery.isPending.value
    || bookConditionsQuery.isPending.value
    || movementTypesQuery.isPending.value,
  )

  const error = computed(() =>
    academicYearsQuery.error.value
    ?? keystagesQuery.error.value
    ?? gradesQuery.error.value
    ?? subjectsQuery.error.value
    ?? classSectionsQuery.error.value
    ?? termsQuery.error.value
    ?? bookConditionsQuery.error.value
    ?? movementTypesQuery.error.value
    ?? null,
  )

  const isLoaded = computed(() =>
    Boolean(
      academicYearsQuery.data.value
      && keystagesQuery.data.value
      && gradesQuery.data.value
      && subjectsQuery.data.value
      && classSectionsQuery.data.value
      && termsQuery.data.value
      && bookConditionsQuery.data.value
      && movementTypesQuery.data.value,
    ),
  )

  async function fetchAcademicYears() {
    await academicYearsQuery.refetch()
  }

  async function fetchKeystages() {
    await keystagesQuery.refetch()
  }

  async function fetchSubjects() {
    await subjectsQuery.refetch()
  }

  async function fetchClassSections() {
    await classSectionsQuery.refetch()
  }

  async function fetchTerms() {
    await termsQuery.refetch()
  }

  async function fetchBookConditions() {
    await bookConditionsQuery.refetch()
  }

  async function fetchMovementTypes() {
    await movementTypesQuery.refetch()
  }

  async function fetchGrades(keystageId?: number) {
    if (!keystageId) {
      scopedGrades.value = null
      await gradesQuery.refetch()
      return
    }

    try {
      const response = await api.get<Lookup[]>(API.lookups.grades, { keystageId })
      if (response.success) {
        scopedGrades.value = response.data
      }
    }
    catch {
      scopedGrades.value = []
    }
  }

  async function fetchAll() {
    await Promise.all([
      fetchAcademicYears(),
      fetchKeystages(),
      fetchGrades(),
      fetchSubjects(),
      fetchClassSections(),
      fetchTerms(),
      fetchBookConditions(),
      fetchMovementTypes(),
    ])
  }

  async function refreshAll() {
    await queryCache.invalidateQueries({ key: ['lookups'] })
  }

  function getLookupLabel(list: Lookup[], id: number): string {
    return list.find(item => item.id === id)?.name ?? ''
  }

  function getEnumLabel(type: 'term' | 'condition' | 'movement' | 'teacherIssueStatus' | 'slipType', value: number): string {
    const maps = {
      term: termLabels,
      condition: conditionLabels,
      movement: movementTypeLabels,
      teacherIssueStatus: teacherIssueStatusLabels,
      slipType: slipTypeLabels,
    }
    return maps[type][value] ?? ''
  }

  return {
    academicYears,
    keystages,
    grades,
    subjects,
    classSections,
    terms,
    bookConditions,
    movementTypes,
    isLoaded,
    isLoading,
    error,
    fetchAll,
    fetchAcademicYears,
    fetchKeystages,
    fetchSubjects,
    fetchGrades,
    fetchClassSections,
    fetchTerms,
    fetchBookConditions,
    fetchMovementTypes,
    refreshAll,
    getLookupLabel,
    getEnumLabel,
  }
})
