// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  modules: [
    '@nuxt/eslint',
    '@nuxt/fonts',
    '@nuxt/icon',
    '@nuxt/hints',
    '@nuxt/image',
    '@nuxtjs/color-mode',
    '@primevue/nuxt-module',
    '@pinia/nuxt',
    'nuxt-csurf',
    'dayjs-nuxt',
    'nuxt-charts',
    '@regle/nuxt',
    '@pinia/colada-nuxt'
  ],
  colorMode: {
    preference: 'system', 
    fallback: 'light',
    componentName: 'ColorScheme',
    storage: 'localStorage',
    storageKey: 'nuxt-color-mode'
  },
  primevue: {},
  csurf: { // optional
    https: false, // default true if in production
    cookieKey: '', // "__Host-csrf" if https is true otherwise just "csrf"
    cookie: { // CookieSerializeOptions from unjs/cookie-es
      path: '/',
      httpOnly: true,
      sameSite: 'strict'
    },
    methodsToProtect: ['POST', 'PUT', 'PATCH'], // the request methods we want CSRF protection for
    encryptSecret: '/** a 32 bits secret */', // for stateless server (like serverless runtime), random bytes by default
    encryptAlgorithm: 'AES-CBC', // by default 'aes-256-cbc' (node), 'AES-CBC' (serverless)
    addCsrfTokenToEventCtx: true, // default false, to run useCsrfFetch on server set it to true
    headerName: 'csrf-token' // the header where the csrf token is stored
  },
  dayjs: {
    locales: ['en'],
    plugins: ['relativeTime', 'utc', 'timezone'],
    defaultLocale: 'en',
    defaultTimezone: 'Indian/Maldives',
  }
})