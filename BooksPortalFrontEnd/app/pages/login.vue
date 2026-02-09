<template>
  <div class="w-full max-w-md">
    <Card>
      <template #title>
        <div class="flex flex-col items-center gap-3">
          <NuxtImg
            src="/logo.png"
            alt="Logo"
            class="h-16 w-16"
          />
          <h1 class="text-2xl font-bold text-surface-900 dark:text-surface-0">
            Books Portal
          </h1>
        </div>
      </template>
      <template #content>
        <form
          class="flex flex-col gap-4"
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
              type="email"
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

definePageMeta({
  layout: 'auth',
})

const { login } = useAuth()

const form = reactive({
  email: '',
  password: '',
})

const errors = reactive({
  email: '',
  password: '',
})

const touched = reactive({
  email: false,
  password: false,
})

const loginError = ref('')
const isLoading = ref(false)

function toFriendlyLoginError(message?: string) {
  if (!message) {
    return 'Unable to sign in right now. Please try again.'
  }

  const normalized = message.toLowerCase()
  if (
    normalized.includes('fetch failed')
    || normalized.includes('econnrefused')
    || normalized.includes('failed to fetch')
  ) {
    return 'Unable to reach the server. Please ensure the backend is running and try again.'
  }

  if (normalized.includes('csrf')) {
    return 'Security validation failed. Please refresh the page and try again.'
  }

  return message
}

function touchField(field: 'email' | 'password') {
  touched[field] = true
  validateField(field)
}

function validateField(field: 'email' | 'password') {
  const result = LoginRequestSchema.shape[field].safeParse(form[field])
  errors[field] = result.success ? '' : result.error.issues[0]?.message ?? 'Invalid'
}

function validateForm(): boolean {
  touched.email = true
  touched.password = true
  validateField('email')
  validateField('password')
  return !errors.email && !errors.password
}

async function handleLogin() {
  if (!validateForm()) return

  isLoading.value = true
  loginError.value = ''

  try {
    const response = await login(form)
    if (response.success) {
      navigateTo('/')
    }
    else {
      loginError.value = toFriendlyLoginError(response.message) || 'Login failed'
    }
  }
  catch (error: unknown) {
    const fetchError = error as { data?: { message?: string }, message?: string }
    loginError.value = toFriendlyLoginError(fetchError.data?.message || fetchError.message)
  }
  finally {
    isLoading.value = false
  }
}
</script>
