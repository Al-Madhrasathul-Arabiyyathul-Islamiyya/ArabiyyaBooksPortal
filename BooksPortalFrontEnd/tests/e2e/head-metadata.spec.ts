import { expect, test } from '@playwright/test'
import { loginAsAdmin } from '~~/e2e/helpers/auth'

const appTitle = 'Arabiyya Academic Books Portal'

test.describe('head metadata @smoke @head', () => {
  test('login page title follows public format', async ({ page }) => {
    await page.goto('/login')
    await expect(page).toHaveTitle(`Login - ${appTitle}`)
  })

  test('operations page title follows client format', async ({ page }) => {
    await loginAsAdmin(page)
    await expect(page).toHaveURL(/\/distribution$/)
    await expect(page).toHaveTitle(`Students Distribution - ${appTitle}`)
  })

  test('admin page title follows admin format', async ({ page }) => {
    await loginAsAdmin(page)
    await page.goto('/admin/reports')
    await expect(page).toHaveTitle(`Reports - Admin - ${appTitle}`)
  })
})
