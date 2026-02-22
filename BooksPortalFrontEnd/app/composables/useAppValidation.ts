import type { Ref } from 'vue'
import type { z } from 'zod/v4'
import { useRegleSchema } from '@regle/schemas'
import { normalizeBackendErrors } from '~/utils/validation/backend-errors'
import { toZodFieldErrors } from '~/utils/validation/zod-errors'

type FormState = Record<string, unknown>
type RegleFieldStatus = {
  $errors?: string[]
  $touch?: () => void
}
type RegleRootStatus = Record<string, RegleFieldStatus> & {
  $reset: () => void
  $touch: () => void
  $validate: () => Promise<unknown>
  $invalid: boolean
  $dirty: boolean
}

export function useAppValidation<TState extends FormState>(
  initialState: TState,
  schema: z.ZodType<TState>,
) {
  const state = reactive(structuredClone(initialState)) as TState
  const externalErrors = ref<Record<string, string[]>>({})
  const globalError = ref('')

  const { r$ } = useRegleSchema(state as never, schema, {
    externalErrors: externalErrors as Ref<Record<string, string[]>>,
  })
  const regle = r$ as unknown as RegleRootStatus

  const errors = computed(() => {
    const map: Record<string, string> = {}

    for (const key of Object.keys(state as FormState)) {
      map[key] = regle[key]?.$errors?.[0] ?? ''
    }

    return map as Partial<Record<keyof TState, string>>
  })

  function setGlobalError(message = '') {
    globalError.value = message
  }

  function resetValidation() {
    globalError.value = ''
    externalErrors.value = {}
    regle.$reset()
  }

  function touchAll() {
    regle.$touch()
  }

  function touchField(field: keyof TState) {
    regle[String(field)]?.$touch?.()
  }

  async function validate() {
    globalError.value = ''
    externalErrors.value = {}
    const result = await regle.$validate()
    return Boolean(result)
  }

  function applyBackendErrors(error: unknown) {
    const normalized = normalizeBackendErrors(error)
    externalErrors.value = normalized.fieldErrors
    globalError.value = normalized.globalErrors[0] ?? ''
  }

  function mapZodErrors(result: z.ZodError) {
    externalErrors.value = toZodFieldErrors(result)
  }

  async function validateWithSchema(values: unknown) {
    const parsed = await schema.safeParseAsync(values)
    if (parsed.success) return parsed
    mapZodErrors(parsed.error)
    touchAll()
    return parsed
  }

  function resetForm() {
    Object.assign(state as FormState, structuredClone(initialState))
    resetValidation()
  }

  return {
    state,
    r$: regle,
    errors,
    globalError,
    isValid: computed(() => !regle.$invalid),
    isDirty: computed(() => regle.$dirty),
    touchAll,
    touchField,
    validate,
    validateWithSchema,
    setGlobalError,
    applyBackendErrors,
    resetValidation,
    resetForm,
  }
}
