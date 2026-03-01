<template>
  <div class="grid gap-4 lg:grid-cols-2">
    <Card>
      <template #title>
        Profile
      </template>
      <template #content>
        <div class="grid gap-4">
          <FormsFormField
            label="Username"
            field-id="profile-userName"
          >
            <InputText
              id="profile-userName"
              :model-value="user?.userName ?? ''"
              disabled
              fluid
            />
          </FormsFormField>

          <FormsFormField
            label="Full Name"
            field-id="profile-fullName"
          >
            <InputText
              id="profile-fullName"
              :model-value="user?.fullName ?? ''"
              disabled
              fluid
            />
          </FormsFormField>

          <FormsFormField
            label="Email"
            field-id="profile-email"
          >
            <InputText
              id="profile-email"
              :model-value="user?.email ?? ''"
              disabled
              fluid
            />
          </FormsFormField>

          <FormsFormField
            label="Roles"
            field-id="profile-roles"
          >
            <div class="flex flex-wrap gap-1">
              <Tag
                v-for="role in user?.roles ?? []"
                :key="role"
                :value="role"
              />
            </div>
          </FormsFormField>
        </div>
      </template>
    </Card>

    <Card>
      <template #title>
        Change Password
      </template>
      <template #content>
        <form
          class="grid gap-4"
          @submit.prevent="submitPasswordChange"
        >
          <FormsFormField
            label="Current Password"
            required
            field-id="currentPassword"
            :error="errors.currentPassword"
          >
            <Password
              id="currentPassword"
              v-model="form.currentPassword"
              toggle-mask
              :feedback="false"
              fluid
              :invalid="!!errors.currentPassword"
              @blur="touchField('currentPassword')"
            />
          </FormsFormField>

          <FormsFormField
            label="New Password"
            required
            field-id="newPassword"
            :error="errors.newPassword"
          >
            <Password
              id="newPassword"
              v-model="form.newPassword"
              toggle-mask
              fluid
              :invalid="!!errors.newPassword"
              @blur="touchField('newPassword')"
            />
          </FormsFormField>

          <FormsFormField
            label="Confirm Password"
            required
            field-id="confirmPassword"
            :error="errors.confirmPassword"
          >
            <Password
              id="confirmPassword"
              v-model="form.confirmPassword"
              toggle-mask
              :feedback="false"
              fluid
              :invalid="!!errors.confirmPassword"
              @blur="touchField('confirmPassword')"
            />
          </FormsFormField>

          <Message
            v-if="formError"
            severity="error"
            :closable="false"
          >
            {{ formError }}
          </Message>

          <div class="flex justify-end">
            <Button
              type="submit"
              label="Update Password"
              :loading="isSubmitting"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import { ChangePasswordRequestSchema } from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  title: 'Profile',
})

useLayoutPageHead('client')

type ProfileFormState = {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

const api = useApi()
const { user } = useAuth()
const { showSuccess, showError } = useAppToast()

const isSubmitting = ref(false)

const PasswordFormSchema = z.object({
  currentPassword: z.string().min(1, 'Current password is required'),
  newPassword: z.string().min(6, 'Password must be at least 6 characters'),
  confirmPassword: z.string().min(1, 'Confirm password is required'),
}).superRefine((values, ctx) => {
  if (values.newPassword !== values.confirmPassword) {
    ctx.addIssue({
      code: 'custom',
      message: 'Passwords do not match',
      path: ['confirmPassword'],
    })
  }
})

const {
  state: form,
  errors,
  globalError: formError,
  touchField,
  validateWithSchema,
  setGlobalError,
  applyBackendErrors,
  resetForm,
} = useAppValidation<ProfileFormState>(
  {
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  },
  PasswordFormSchema as z.ZodType<ProfileFormState>,
)

async function submitPasswordChange() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  const payload = {
    currentPassword: parsed.data.currentPassword,
    newPassword: parsed.data.newPassword,
  }
  const requestCheck = ChangePasswordRequestSchema.safeParse(payload)
  if (!requestCheck.success) {
    setGlobalError('Invalid password change payload.')
    return
  }

  isSubmitting.value = true
  try {
    const response = await api.post<boolean>(API.auth.changePassword, requestCheck.data)
    if (response.success) {
      showSuccess(response.message ?? 'Password updated successfully')
      resetForm()
      return
    }
    setGlobalError(response.message ?? 'Failed to change password')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
    if (!formError.value) {
      showError(getFriendlyErrorMessage(error, 'Failed to change password'))
    }
  }
  finally {
    isSubmitting.value = false
  }
}
</script>
