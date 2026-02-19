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
