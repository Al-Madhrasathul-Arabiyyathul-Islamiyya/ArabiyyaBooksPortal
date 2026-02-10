<template>
  <NuxtLoadingIndicator
    color="#10b981"
    :height="3"
    :throttle="80"
  />
  <NuxtLayout>
    <NuxtPage />
  </NuxtLayout>
  <Toast />
  <ConfirmDialog />
</template>
