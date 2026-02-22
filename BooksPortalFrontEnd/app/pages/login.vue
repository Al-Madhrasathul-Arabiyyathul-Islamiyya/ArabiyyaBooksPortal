<template>
  <div class="w-full max-w-md">
    <Card>
      <template #title>
        <div class="flex flex-col items-center gap-3">
          <NuxtImg
            src="/logo.svg"
            alt="Logo"
            class="h-16 w-16"
          />
          <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
            {{ appTitle }}
          </h1>
        </div>
      </template>
      <template #content>
        <form
          class="flex flex-col gap-4"
          autocomplete="on"
          @submit.prevent="handleLogin"
        >
          <div class="flex flex-col gap-1">
            <label
              for="email"
              class="text-sm font-medium"
            >Email</label>
            <InputText
              id="email"
              v-model="form.email"
              name="email"
              type="email"
              autocomplete="username email"
              placeholder="Enter your email"
              :invalid="!!errors.email"
              @blur="touchField('email')"
            />
            <small
              v-if="errors.email"
              class="text-red-500"
            >{{ errors.email }}</small>
          </div>

          <div class="flex flex-col gap-1">
            <label
              for="password"
              class="text-sm font-medium"
            >Password</label>
            <Password
              id="password"
              v-model="form.password"
              placeholder="Enter your password"
              :feedback="false"
              toggle-mask
              fluid
              :input-props="{ autocomplete: 'current-password', name: 'password' }"
              :invalid="!!errors.password"
              @blur="touchField('password')"
            />
            <small
              v-if="errors.password"
              class="text-red-500"
            >{{ errors.password }}</small>
          </div>

          <Message
            v-if="loginError"
            severity="error"
            :closable="false"
          >
            {{ loginError }}
          </Message>

          <Button
            type="submit"
            label="Sign In"
            icon="pi pi-sign-in"
            :loading="isLoading"
            class="mt-2"
          />
        </form>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
import { LoginRequestSchema } from '~/types/forms'
import { toFriendlyLoginError } from '~/utils/auth-errors'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'auth',
})

const { login } = useAuth()
const { public: { appTitle } } = useRuntimeConfig()
const loginTitle = computed(() => `Login - ${appTitle}`)

useHead(() => ({
  title: loginTitle.value,
}))

useSeoMeta({
  title: loginTitle,
  ogTitle: loginTitle,
  twitterTitle: loginTitle,
  description: () => `Login page for ${appTitle}.`,
  ogDescription: () => `Login page for ${appTitle}.`,
  twitterDescription: () => `Login page for ${appTitle}.`,
})

const {
  state: form,
  errors,
  globalError: loginError,
  touchField,
  validateWithSchema,
  setGlobalError,
} = useAppValidation(
  {
    email: '',
    password: '',
  },
  LoginRequestSchema,
)

const isLoading = ref(false)
const savedEmailKey = 'bp.login.email'

async function handleLogin() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  isLoading.value = true
  setGlobalError('')

  try {
    const response = await login(parsed.data)
    if (response.success) {
      if (import.meta.client) {
        localStorage.setItem(savedEmailKey, form.email)
      }
      navigateTo('/')
    }
    else {
      setGlobalError(toFriendlyLoginError(response.message) || 'Login failed')
    }
  }
  catch (error: unknown) {
    const safeMessage = getFriendlyErrorMessage(error, 'Login failed')
    setGlobalError(toFriendlyLoginError(safeMessage))
  }
  finally {
    isLoading.value = false
  }
}

onMounted(() => {
  if (!import.meta.client) return
  if (form.email) return
  const savedEmail = localStorage.getItem(savedEmailKey)
  if (savedEmail) {
    form.email = savedEmail
  }
})
</script>
