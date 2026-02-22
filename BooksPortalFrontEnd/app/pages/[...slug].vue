<template>
  <div class="w-full max-w-2xl px-4">
    <Card>
      <template #content>
        <div class="flex flex-col items-center gap-4 py-6 text-center">
          <NuxtImg
            src="/logo.svg"
            alt="Logo"
            class="h-16 w-16"
          />

          <Tag
            value="404"
            severity="contrast"
            rounded
          />

          <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
            Page Not Found
          </h1>
          <p class="max-w-lg text-sm text-surface-600 dark:text-surface-300">
            The page you requested does not exist or may have been moved.
          </p>

          <div class="mt-2 flex flex-wrap items-center justify-center gap-2">
            <Button
              label="Go Back"
              icon="pi pi-arrow-left"
              severity="secondary"
              @click="goBack"
            />
            <Button
              label="Go to Home"
              icon="pi pi-home"
              @click="goHome"
            />
          </div>

          <p class="mt-2 text-xs text-surface-500 dark:text-surface-400">
            Requested path: <span class="font-medium">{{ route.path }}</span>
          </p>
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: 'auth',
})

if (import.meta.server) {
  setResponseStatus(404, 'Page Not Found')
}

const route = useRoute()
const router = useRouter()
const { public: { appTitle } } = useRuntimeConfig()

const pageTitle = computed(() => `Page Not Found - ${appTitle}`)

useHead(() => ({
  title: pageTitle.value,
}))

useSeoMeta({
  title: pageTitle,
  ogTitle: pageTitle,
  twitterTitle: pageTitle,
  description: 'The requested page could not be found.',
  robots: 'noindex, nofollow',
})

function goHome() {
  navigateTo('/distribution')
}

async function goBack() {
  if (!import.meta.client) {
    await navigateTo('/distribution')
    return
  }

  const referrer = document.referrer
  const hasLocalReferrer = referrer.startsWith(window.location.origin)

  if (window.history.length > 1 && hasLocalReferrer) {
    await router.back()
    return
  }

  await navigateTo('/distribution')
}
</script>
