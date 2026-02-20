import { expect, test } from '@playwright/test'

test.describe('auth guards @smoke @auth', () => {
  test('unauthenticated users are redirected from distribution to login', async ({ page }) => {
    await page.goto('/distribution')
    await expect(page).toHaveURL(/\/login$/)
    await expect(page.getByRole('button', { name: 'Sign In' })).toBeVisible()
  })

  test('unauthenticated users are redirected from admin reports to login', async ({ page }) => {
    await page.goto('/admin/reports/distributions')
    await expect(page).toHaveURL(/\/login$/)
  })

  test('login page renders expected fields and app title', async ({ page }) => {
    await page.goto('/login')

    await expect(page.getByRole('heading', { name: 'Arabiyya Books Portal' })).toBeVisible()
    await expect(page.getByLabel('Email')).toBeVisible()
    await expect(page.getByPlaceholder('Enter your password')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Sign In' })).toBeVisible()
  })
})
