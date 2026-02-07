import { z } from 'zod/v4'

export const ApiErrorSchema = z.object({
  field: z.string(),
  message: z.string(),
})
export type ApiError = z.infer<typeof ApiErrorSchema>

export const ApiResponseSchema = <T extends z.ZodType>(dataSchema: T) =>
  z.object({
    success: z.boolean(),
    data: dataSchema,
    message: z.string().nullable().optional(),
    errors: z.array(ApiErrorSchema).nullable().optional(),
  })
export type ApiResponse<T> = {
  success: boolean
  data: T
  message?: string | null
  errors?: ApiError[] | null
}

export const PaginatedListSchema = <T extends z.ZodType>(itemSchema: T) =>
  z.object({
    items: z.array(itemSchema),
    totalCount: z.number(),
    pageNumber: z.number(),
    pageSize: z.number(),
    totalPages: z.number(),
    hasPrevious: z.boolean(),
    hasNext: z.boolean(),
  })
export type PaginatedList<T> = {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPrevious: boolean
  hasNext: boolean
}

export interface PaginationParams {
  pageNumber?: number
  pageSize?: number
}
