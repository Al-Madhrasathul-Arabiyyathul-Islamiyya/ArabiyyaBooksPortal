import { defineVitestConfig } from '@nuxt/test-utils/config'

export default defineVitestConfig({
  test: {
    globals: true,
    environment: 'node',
    include: ['tests/unit/**/*.spec.ts'],
    setupFiles: ['tests/setup.ts'],
    hookTimeout: 120000,
  },
})
