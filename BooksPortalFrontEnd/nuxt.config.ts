import Aura from '@primeuix/themes/aura'
import tailwindcss from '@tailwindcss/vite'

export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  sourcemap: {
    server: false,
    client: false,
  },

  modules: [
    '@nuxt/eslint',
    '@nuxt/fonts',
    '@nuxt/icon',
    '@nuxt/hints',
    '@nuxt/image',
    '@nuxtjs/color-mode',
    '@primevue/nuxt-module',
    '@pinia/nuxt',
    '@pinia/colada-nuxt',
    '@regle/nuxt',
    'dayjs-nuxt',
    'nuxt-charts',
  ],

  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5071/api',
    },
  },

  css: ['./app/assets/css/main.css'],

  vite: {
    plugins: [
      tailwindcss(),
    ],
    build: {
      sourcemap: false, // Disable source maps in production
    },
  },

  primevue: {
    options: {
      theme: {
        preset: Aura,
        options: {
          darkModeSelector: '.dark',
          cssLayer: {
            name: 'primevue',
            order: 'theme, base, primevue',
          },
        },
      },
      ripple: true,
    },
    autoImport: true,
  },

  colorMode: {
    preference: 'system',
    fallback: 'light',
    classSuffix: '',
    storage: 'cookie',
    storageKey: 'nuxt-color-mode',
  },

  fonts: {
    families: [
      { name: 'Sofia Sans', provider: 'google' },
      { name: 'Playfair Display', provider: 'google' },
      { name: 'Geist Mono', provider: 'google' },
    ],
    defaults: {
      weights: [300, 400, 500, 600, 700],
      styles: ['normal', 'italic'],
    },
  },

  dayjs: {
    locales: ['en'],
    plugins: ['relativeTime', 'utc', 'timezone'],
    defaultLocale: 'en',
    defaultTimezone: 'Indian/Maldives',
  },
})
