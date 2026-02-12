<template>
  <form
    class="grid gap-4"
    @submit.prevent="submit"
  >
    <FormsFormField
      label="Full Name"
      required
      field-id="quickParentFullName"
      :error="errors.fullName"
    >
      <InputText
        id="quickParentFullName"
        v-model.trim="form.fullName"
        fluid
        :invalid="!!errors.fullName"
        @blur="touchField('fullName')"
      />
    </FormsFormField>

    <FormsFormField
      label="National ID"
      required
      field-id="quickParentNationalId"
      :error="errors.nationalId"
    >
      <InputText
        id="quickParentNationalId"
        v-model.trim="form.nationalId"
        fluid
        :invalid="!!errors.nationalId"
        @blur="touchField('nationalId')"
      />
    </FormsFormField>

    <div class="grid gap-4 md:grid-cols-2">
      <FormsFormField
        label="Phone"
        field-id="quickParentPhone"
        :error="errors.phone"
      >
        <InputText
          id="quickParentPhone"
          v-model.trim="form.phone"
          fluid
          :invalid="!!errors.phone"
          @blur="touchField('phone')"
        />
      </FormsFormField>

      <FormsFormField
        label="Relationship"
        field-id="quickParentRelationship"
        :error="errors.relationship"
      >
        <InputText
          id="quickParentRelationship"
          v-model.trim="form.relationship"
          fluid
          :invalid="!!errors.relationship"
          @blur="touchField('relationship')"
        />
      </FormsFormField>
    </div>

    <div
      v-if="student"
      class="rounded-md border border-surface-200 p-3 text-sm dark:border-surface-700"
    >
      <div class="font-medium">
        Link to selected student
      </div>
      <div class="text-surface-600 dark:text-surface-400">
        {{ student.fullName }}
      </div>
      <div class="mt-2">
        <Checkbox
          v-model="linkToStudent"
          input-id="linkToStudent"
          binary
        />
        <label
          for="linkToStudent"
          class="ml-2"
        >Link parent to this student after create</label>
      </div>
    </div>

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
        label="Create Parent"
        :loading="isSubmitting"
      />
    </div>
  </form>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { Parent, Student } from '~/types/entities'
import { CreateParentRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

const props = defineProps<{
  student: Student | null
}>()

const emit = defineEmits<{
  (e: 'created', parent: Parent): void
  (e: 'cancel'): void
}>()

const api = useApi()
const { showError, showSuccess } = useAppToast()

const isSubmitting = ref(false)
const linkToStudent = ref(true)

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
    nationalId: '',
    phone: '',
    relationship: '',
  },
  CreateParentRequestSchema.extend({
    phone: CreateParentRequestSchema.shape.phone.or(z.string()),
    relationship: CreateParentRequestSchema.shape.relationship.or(z.string()),
  }),
)

function normalizeNullable(value: string | null | undefined) {
  const text = (value ?? '').trim()
  return text.length ? text : null
}

async function linkParentToStudent(parent: Parent) {
  if (!props.student || !linkToStudent.value) return

  const existingParents = props.student.parents.map(item => ({
    parentId: item.parentId,
    isPrimary: item.isPrimary,
  }))
  if (!existingParents.some(item => item.parentId === parent.id)) {
    existingParents.push({
      parentId: parent.id,
      isPrimary: existingParents.length === 0,
    })
  }

  await api.put(API.students.byId(props.student.id), {
    fullName: props.student.fullName,
    nationalId: props.student.nationalId,
    classSectionId: props.student.classSectionId,
    parents: existingParents,
  })
}

async function submit() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  isSubmitting.value = true
  try {
    const response = await api.post<Parent>(API.parents.base, {
      fullName: parsed.data.fullName,
      nationalId: parsed.data.nationalId,
      phone: normalizeNullable(parsed.data.phone),
      relationship: normalizeNullable(parsed.data.relationship),
    })
    if (!response.success) {
      setGlobalError(response.message ?? 'Failed to create parent')
      return
    }

    try {
      await linkParentToStudent(response.data)
    }
    catch (error: unknown) {
      showError(getFriendlyErrorMessage(error, 'Parent created, but linking to student failed'))
    }

    showSuccess('Parent created successfully')
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
  () => props.student?.id,
  () => {
    linkToStudent.value = !!props.student
  },
  { immediate: true },
)
</script>
