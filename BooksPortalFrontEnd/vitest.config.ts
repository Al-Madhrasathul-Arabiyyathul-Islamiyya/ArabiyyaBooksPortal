import { defineConfig } from 'vitest/config'
import { defineVitestProject } from '@nuxt/test-utils/config'
import { fileURLToPath } from 'node:url'
import AutoImport from 'unplugin-auto-import/vite'

const alias = {
  '~': fileURLToPath(new URL('./app', import.meta.url)),
  '@': fileURLToPath(new URL('./app', import.meta.url)),
  '~~': fileURLToPath(new URL('./tests', import.meta.url)),
}

export default defineConfig({
  resolve: { alias },
  test: {
    globals: true,
    projects: [
      {
        plugins: [
          AutoImport({
            imports: ['vue'],
            dts: false,
          }),
        ],
        resolve: { alias },
        test: {
          name: 'unit',
          include: ['tests/unit/**/*.spec.ts'],
          environment: 'node',
          setupFiles: ['tests/setup.ts'],
        },
      },
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
