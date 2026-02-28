<template>
  <div class="mx-auto w-full max-w-2xl py-6">
    <Card>
      <template #title>
        <div class="space-y-1">
          <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
            Initial System Setup
          </h1>
          <p class="text-sm text-surface-600 dark:text-surface-300">
            Create the first SuperAdmin account to unlock the system.
          </p>
        </div>
      </template>
      <template #content>
        <Message
          v-if="isReady"
          severity="success"
          :closable="false"
          class="mb-4"
        >
          Setup is already complete. Continue to login.
        </Message>

        <form
          class="grid gap-4 md:grid-cols-2"
          @submit.prevent="handleBootstrap"
        >
          <FormsFormField
            label="Username"
            field-id="userName"
            required
          >
            <InputText
              id="userName"
              v-model="form.userName"
              :disabled="isReady || isSubmitting"
              :invalid="!!errors.userName"
              @blur="touchField('userName')"
            />
          </FormsFormField>

          <FormsFormField
            label="Email"
            field-id="email"
            required
          >
            <InputText
              id="email"
              v-model="form.email"
              type="email"
              :disabled="isReady || isSubmitting"
              :invalid="!!errors.email"
              @blur="touchField('email')"
            />
          </FormsFormField>

          <FormsFormField
            label="Full Name"
            field-id="fullName"
            required
          >
            <InputText
              id="fullName"
              v-model="form.fullName"
              :disabled="isReady || isSubmitting"
              :invalid="!!errors.fullName"
              @blur="touchField('fullName')"
            />
          </FormsFormField>

          <FormsFormField
            label="Password"
            field-id="password"
            required
          >
            <Password
              id="password"
              v-model="form.password"
              toggle-mask
              :feedback="false"
              fluid
              :disabled="isReady || isSubmitting"
              :invalid="!!errors.password"
              @blur="touchField('password')"
            />
          </FormsFormField>

          <FormsFormField
            label="National ID"
            field-id="nationalId"
          >
            <InputText
              id="nationalId"
              v-model="form.nationalId"
              :disabled="isReady || isSubmitting"
              :invalid="!!errors.nationalId"
              @blur="touchField('nationalId')"
            />
          </FormsFormField>

          <FormsFormField
            label="Designation"
            field-id="designation"
          >
            <InputText
              id="designation"
              v-model="form.designation"
              :disabled="isReady || isSubmitting"
              :invalid="!!errors.designation"
              @blur="touchField('designation')"
            />
          </FormsFormField>

          <div class="md:col-span-2 flex flex-wrap items-center justify-end gap-2">
            <Button
              label="Go to Login"
              icon="pi pi-sign-in"
              severity="secondary"
              outlined
              :disabled="!isReady"
              @click="navigateTo('/login')"
            />
            <Button
              type="submit"
              label="Create SuperAdmin"
              icon="pi pi-user-plus"
              :loading="isSubmitting"
              :disabled="isReady"
            />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { BootstrapSuperAdminRequestSchema } from '~/types/forms'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'auth',
})

const { showSuccess, showError } = useAppToast()
const setupStore = useSetupReadinessStore()
const authStore = useAuthStore()

const {
  state: form,
  errors,
  touchField,
  validateWithSchema,
} = useAppValidation(
  {
    userName: '',
    email: '',
    password: '',
    fullName: '',
    nationalId: '',
    designation: '',
  },
  BootstrapSuperAdminRequestSchema,
)

const isSubmitting = ref(false)
const isReady = computed(() => setupStore.isCompleted)

async function loadStatus() {
  await authStore.initialize()
  if (authStore.isAuthenticated) {
    await navigateTo('/admin/settings/setup')
    return
  }

  await setupStore.fetchStatus(true)
  if (setupStore.isCompleted) {
    await navigateTo('/login')
  }
}

async function handleBootstrap() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  isSubmitting.value = true
  try {
    await setupStore.mutate('bootstrapSuperAdmin', {
      ...parsed.data,
      nationalId: parsed.data.nationalId || null,
      designation: parsed.data.designation || null,
    })
    showSuccess('SuperAdmin account created. Continue setup from Admin Settings.')
    await navigateTo('/login')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to create SuperAdmin account'))
  }
  finally {
    isSubmitting.value = false
  }
}

onMounted(() => {
  void loadStatus()
})
</script>
