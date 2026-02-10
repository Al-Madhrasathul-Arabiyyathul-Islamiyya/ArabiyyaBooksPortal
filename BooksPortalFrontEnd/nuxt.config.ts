import Aura from '@primeuix/themes/aura'
import tailwindcss from '@tailwindcss/vite'

export default defineNuxtConfig({

  modules: [
    '@nuxt/eslint',
    '@nuxt/fonts',
    '@nuxt/icon',
    '@nuxt/hints',
    '@nuxt/image',
    '@nuxtjs/color-mode',
    'nuxt-csurf',
    '@primevue/nuxt-module',
    '@pinia/nuxt',
    '@pinia/colada-nuxt',
    '@regle/nuxt',
    'dayjs-nuxt',
    'nuxt-charts',
  ],
  devtools: { enabled: true },

  css: ['./app/assets/css/main.css'],

  colorMode: {
    preference: 'system',
    fallback: 'light',
    classSuffix: '',
    storage: 'cookie',
    storageKey: 'nuxt-color-mode',
  },

  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5071/api',
      appTitle: 'Arabiyya Books Portal',
    },
  },

  routeRules: {
    // Nuxt hydration internals perform POSTs; do not apply CSRF checks there.
    '/__nuxt_hydration__/**': {
      ...({ csurf: false } as Record<string, unknown>),
    },
  },

  sourcemap: {
    server: false,
    client: false,
  },
  compatibilityDate: '2025-07-15',

  vite: {
    plugins: [
      tailwindcss(),
    ],
    build: {
      sourcemap: false, // Disable source maps in production
    },
  },

  csurf: {
    cookieKey: 'bp_csrf',
    cookie: {
      httpOnly: false,
      sameSite: 'lax',
      secure: !import.meta.dev,
    },
    methodsToProtect: ['POST', 'PUT', 'PATCH', 'DELETE'],
  },

  dayjs: {
    locales: ['en'],
    plugins: ['relativeTime', 'utc', 'timezone'],
    defaultLocale: 'en',
    defaultTimezone: 'Indian/Maldives',
  },

  eslint: {
    config: {
      stylistic: {
        indent: 2,
        quotes: 'single',
        semi: false,
      },
    },
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
})
