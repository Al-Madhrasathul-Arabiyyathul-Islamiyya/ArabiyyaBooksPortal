<template>
  <Card>
    <template #title>
      Add New Teacher
    </template>
    <template #content>
      <form
        class="grid gap-3"
        @submit.prevent="submit"
      >
        <FormsFormField
          label="Full Name"
          required
          field-id="quickTeacherFullName"
          :error="errors.fullName"
        >
          <InputText
            id="quickTeacherFullName"
            v-model.trim="form.fullName"
            fluid
            :invalid="!!errors.fullName"
            @blur="touchField('fullName')"
          />
        </FormsFormField>

        <FormsFormField
          label="National ID"
          required
          field-id="quickTeacherNationalId"
          :error="errors.nationalId"
        >
          <InputText
            id="quickTeacherNationalId"
            v-model.trim="form.nationalId"
            fluid
            :invalid="!!errors.nationalId"
            @blur="touchField('nationalId')"
          />
        </FormsFormField>

        <FormsFormField
          label="Email"
          field-id="quickTeacherEmail"
          :error="errors.email"
        >
          <InputText
            id="quickTeacherEmail"
            v-model.trim="form.email"
            fluid
            :invalid="!!errors.email"
            @blur="touchField('email')"
          />
        </FormsFormField>

        <FormsFormField
          label="Phone"
          field-id="quickTeacherPhone"
          :error="errors.phone"
        >
          <InputText
            id="quickTeacherPhone"
            v-model.trim="form.phone"
            fluid
            :invalid="!!errors.phone"
            @blur="touchField('phone')"
          />
        </FormsFormField>

        <Message
          v-if="globalError"
          severity="error"
          :closable="false"
        >
          {{ globalError }}
        </Message>

        <div class="mt-1 flex justify-end gap-2">
          <Button
            type="button"
            label="Back to Lookup"
            severity="secondary"
            text
            @click="emit('cancel')"
          />
          <Button
            type="submit"
            label="Create Teacher"
            :loading="isSubmitting"
          />
        </div>
      </form>
    </template>
  </Card>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { Teacher } from '~/types/entities'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

const emit = defineEmits<{
  (e: 'created', teacher: Teacher): void
  (e: 'cancel'): void
}>()

const api = useApi()
const { showSuccess, showError } = useAppToast()
const isSubmitting = ref(false)

const FormSchema = z.object({
  fullName: z.string().min(1, 'Full name is required'),
  nationalId: z.string().min(1, 'National ID is required'),
  email: z.email('Invalid email').optional().or(z.literal('')),
  phone: z.string().optional(),
})

const {
  state: form,
  errors,
  globalError,
  touchField,
  validateWithSchema,
  applyBackendErrors,
  setGlobalError,
  resetForm,
} = useAppValidation(
  {
    fullName: '',
    nationalId: '',
    email: '',
    phone: '',
  },
  FormSchema,
)

function normalizeNullable(value: string) {
  const trimmed = value.trim()
  return trimmed.length ? trimmed : null
}

async function submit() {
  setGlobalError('')
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  isSubmitting.value = true
  try {
    const response = await api.post<Teacher>(API.teachers.base, {
      fullName: form.fullName.trim(),
      nationalId: form.nationalId.trim(),
      email: normalizeNullable(form.email ?? ''),
      phone: normalizeNullable(form.phone ?? ''),
    })

    if (response.success) {
      showSuccess(response.message ?? 'Teacher created')
      emit('created', response.data)
      resetForm()
      return
    }
    setGlobalError(response.message ?? 'Failed to create teacher')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
    showError(getFriendlyErrorMessage(error, 'Failed to create teacher'))
  }
  finally {
    isSubmitting.value = false
  }
}
</script>
