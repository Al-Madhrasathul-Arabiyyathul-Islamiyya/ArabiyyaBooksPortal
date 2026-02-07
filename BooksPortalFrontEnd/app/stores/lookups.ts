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
  const teachers = ref<Lookup[]>([])
  const parents = ref<Lookup[]>([])

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

  async function fetchTeachers() {
    try {
      const response = await api.get<Lookup[]>(API.lookups.teachers)
      if (response.success) {
        teachers.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch teachers lookup:', error)
    }
  }

  async function fetchParents() {
    try {
      const response = await api.get<Lookup[]>(API.lookups.parents)
      if (response.success) {
        parents.value = response.data
      }
    }
    catch (error) {
      console.error('Failed to fetch parents lookup:', error)
    }
  }

  async function fetchAll() {
    await Promise.all([
      fetchAcademicYears(),
      fetchKeystages(),
      fetchSubjects(),
      fetchClassSections(),
      fetchTeachers(),
      fetchParents(),
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
    teachers,
    parents,
    isLoaded,
    fetchAll,
    fetchAcademicYears,
    fetchKeystages,
    fetchSubjects,
    fetchClassSections,
    fetchTeachers,
    fetchParents,
    getLookupLabel,
    getEnumLabel,
  }
})
