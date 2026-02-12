<template>
  <form
    class="grid gap-4"
    @submit.prevent="submit"
  >
    <FormsFormField
      label="Full Name"
      required
      field-id="quickStudentFullName"
      :error="errors.fullName"
    >
      <InputText
        id="quickStudentFullName"
        v-model.trim="form.fullName"
        fluid
        :invalid="!!errors.fullName"
        @blur="touchField('fullName')"
      />
    </FormsFormField>

    <div class="grid gap-4 md:grid-cols-2">
      <FormsFormField
        label="Index No"
        required
        field-id="quickStudentIndexNo"
        :error="errors.indexNo"
      >
        <InputText
          id="quickStudentIndexNo"
          v-model.trim="form.indexNo"
          fluid
          :invalid="!!errors.indexNo"
          @blur="touchField('indexNo')"
        />
      </FormsFormField>

      <FormsFormField
        label="National ID"
        required
        field-id="quickStudentNationalId"
        :error="errors.nationalId"
      >
        <InputText
          id="quickStudentNationalId"
          v-model.trim="form.nationalId"
          fluid
          :invalid="!!errors.nationalId"
          @blur="touchField('nationalId')"
        />
      </FormsFormField>
    </div>

    <FormsFormField
      label="Class"
      required
      field-id="quickStudentClassSection"
      :error="errors.classSectionId"
    >
      <Select
        id="quickStudentClassSection"
        v-model="form.classSectionId"
        :options="classSectionOptions"
        option-label="label"
        option-value="value"
        placeholder="Select class"
        fluid
        :invalid="!!errors.classSectionId"
        @blur="touchField('classSectionId')"
      />
    </FormsFormField>

    <Message
      v-if="formError"
      severity="error"
      :closable="false"
    >
      {{ formError }}
    </Message>

    <div class="flex justify-end gap-2">
      <Button
        type="button"
        label="Back"
        severity="secondary"
        text
        @click="emit('cancel')"
      />
      <Button
        type="submit"
        label="Create Student"
        :loading="isSubmitting"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import type { Lookup, Student } from '~/types/entities'
import { CreateStudentRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

const props = defineProps<{
  academicYearId: number | null
}>()

const emit = defineEmits<{
  (e: 'created', student: Student): void
  (e: 'cancel'): void
}>()

const api = useApi()
const { showError, showSuccess } = useAppToast()

const isSubmitting = ref(false)
const classSectionOptions = ref<Array<{ label: string, value: number }>>([])

const {
  state: form,
  errors,
  globalError: formError,
  touchField,
  validateWithSchema,
  applyBackendErrors,
  setGlobalError,
} = useAppValidation(
  {
    fullName: '',
    indexNo: '',
    nationalId: '',
    classSectionId: null as number | null,
    parents: [] as Array<{ parentId: number, isPrimary: boolean }>,
  },
  CreateStudentRequestSchema.extend({
    classSectionId: CreateStudentRequestSchema.shape.classSectionId.nullable(),
  }),
)

async function loadClassSections() {
  try {
    const response = await api.get<Lookup[]>(API.lookups.classSections, {
      academicYearId: props.academicYearId ?? undefined,
    })
    if (!response.success) {
      showError(response.message ?? 'Failed to load class sections')
      return
    }
    classSectionOptions.value = response.data.map(section => ({
      label: section.name,
      value: section.id,
    }))
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load class sections'))
  }
}

async function submit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return
  if (parsed.data.classSectionId === null) {
    setGlobalError('Class is required')
    return
  }

  isSubmitting.value = true
  try {
    const response = await api.post<Student>(API.students.base, {
      ...parsed.data,
      classSectionId: parsed.data.classSectionId,
    })
    if (!response.success) {
      setGlobalError(response.message ?? 'Failed to create student')
      return
    }
    showSuccess('Student created successfully')
    emit('created', response.data)
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

watch(
  () => props.academicYearId,
  async () => {
    form.classSectionId = null
    await loadClassSections()
  },
  { immediate: true },
)
</script>
