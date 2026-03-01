export type ApiEnvelope<T> = {
  success: boolean
  message?: string
  data: T
}

export type Paginated<T> = {
  items: T[]
  totalCount: number
}

export type QueryParams = Record<string, string | number | boolean | undefined>

export type Student = {
  id: number
  fullName: string
  indexNo?: string
  parents: { parentId: number }[]
}

export type Teacher = {
  id: number
  fullName: string
  nationalId?: string | null
  email?: string | null
}

export type Book = {
  id: number
  available: number
}

export type DistributionCreateResponse = { id: number }
export type TeacherIssueCreateResponse = { id: number, referenceNo?: string }
export type DistributionDetail = { id: number, referenceNo: string }
export type TeacherIssueDetail = { id: number, referenceNo: string }
