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

  // Cached lookup data
  const academicYears = ref<Lookup[]>([])
  const keystages = ref<Lookup[]>([])
  const subjects = ref<Lookup[]>([])
  const classSections = ref<Lookup[]>([])
  const terms = ref<Lookup[]>([])
  const bookConditions = ref<Lookup[]>([])
  const movementTypes = ref<Lookup[]>([])

  const isLoaded = ref(false)

  async function fetchAcademicYears() {
    try {
      const response = await api.get<Lookup[]>(API.lookups.academicYears)
      if (response.success) {
        academicYears.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch academic years lookup:', error)
    }
  }

  async function fetchKeystages() {
    try {
      const response = await api.get<Lookup[]>(API.lookups.keystages)
      if (response.success) {
        keystages.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch keystages lookup:', error)
    }
  }

  async function fetchSubjects() {
    try {
      const response = await api.get<Lookup[]>(API.lookups.subjects)
      if (response.success) {
        subjects.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch subjects lookup:', error)
    }
  }

  async function fetchClassSections() {
    try {
      const response = await api.get<Lookup[]>(API.lookups.classSections)
      if (response.success) {
        classSections.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch class sections lookup:', error)
    }
  }

  async function fetchTerms() {
    try {
      const response = await api.get<Lookup[]>(API.lookups.terms)
      if (response.success) {
        terms.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch terms lookup:', error)
    }
  }

  async function fetchBookConditions() {
    try {
      const response = await api.get<Lookup[]>(API.lookups.bookConditions)
      if (response.success) {
        bookConditions.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch book conditions lookup:', error)
    }
  }

  async function fetchMovementTypes() {
    try {
      const response = await api.get<Lookup[]>(API.lookups.movementTypes)
      if (response.success) {
        movementTypes.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch movement types lookup:', error)
    }
  }

  async function fetchAll() {
    await Promise.all([
      fetchAcademicYears(),
      fetchKeystages(),
      fetchSubjects(),
      fetchClassSections(),
      fetchTerms(),
      fetchBookConditions(),
      fetchMovementTypes(),
    ])
    isLoaded.value = true
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
    subjects,
    classSections,
    terms,
    bookConditions,
    movementTypes,
    isLoaded,
    fetchAll,
    fetchAcademicYears,
    fetchKeystages,
    fetchSubjects,
    fetchClassSections,
    fetchTerms,
    fetchBookConditions,
    fetchMovementTypes,
    getLookupLabel,
    getEnumLabel,
  }
})
