import { defineConfig } from 'vitest/config'
import { defineVitestProject } from '@nuxt/test-utils/config'

export default defineConfig({
  test: {
    globals: true,
    projects: [
      await defineVitestProject({
        test: {
          name: 'unit',
          include: ['tests/unit/**/*.spec.ts'],
          environment: 'node',
          setupFiles: ['tests/setup.ts'],
        },
      }),
      await defineVitestProject({
        test: {
          name: 'nuxt',
          include: ['tests/nuxt/**/*.spec.ts'],
          environment: 'nuxt',
          setupFiles: ['tests/setup.ts'],
          hookTimeout: 120000,
        },
      }),
    ],
  },
})
