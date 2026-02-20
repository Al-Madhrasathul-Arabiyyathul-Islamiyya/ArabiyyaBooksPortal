import { expect, test, type Page } from '@playwright/test'
import { loginAsAdmin, loginAsUser } from '~~/e2e/helpers/auth'
import { bffGet, bffPost } from '~~/e2e/helpers/api'
import type {
  Book,
  DistributionCreateResponse,
  DistributionDetail,
  Paginated,
  Student,
  Teacher,
  TeacherIssueCreateResponse,
  TeacherIssueDetail,
} from '~~/e2e/helpers/types'

async function pickAcademicYearId(page: Page) {
  const active = await bffGet<{ id: number }>(page, '/AcademicYears/active')
  return active.id
}

async function pickStudentWithParent(page: Page, academicYearId: number) {
  const studentsPage = await bffGet<Paginated<Student>>(page, '/students', {
    pageNumber: 1,
    pageSize: 100,
    academicYearId,
  })
  const student = studentsPage.items.find(item => item.parents.length > 0)
  expect(student, 'Expected at least one seeded student with linked parent').toBeTruthy()
  return student!
}

async function pickTeacher(page: Page) {
  const teachers = await bffGet<Paginated<Teacher>>(page, '/teachers', {
    pageNumber: 1,
    pageSize: 50,
  })
  expect(teachers.items.length > 0, 'Expected seeded teachers to exist').toBeTruthy()
  return teachers.items[0]!
}

async function pickBookWithStock(page: Page) {
  const books = await bffGet<Paginated<Book>>(page, '/books', {
    pageNumber: 1,
    pageSize: 200,
  })
  const withStock = books.items.find(book => book.available > 0)
  expect(withStock, 'Expected at least one seeded book with available stock').toBeTruthy()
  return withStock!
}

async function seedDistribution(page: Page, csrfToken: string) {
  const academicYearId = await pickAcademicYearId(page)
  const student = await pickStudentWithParent(page, academicYearId)
  const book = await pickBookWithStock(page)

  const created = await bffPost<DistributionCreateResponse>(page, '/distributions', {
    academicYearId,
    term: 1,
    studentId: student.id,
    parentId: student.parents[0]!.parentId,
    notes: 'e2e report seed distribution',
    items: [{ bookId: book.id, quantity: 1 }],
  }, csrfToken)

  const detail = await bffGet<DistributionDetail>(page, `/distributions/${created.id}`)
  return {
    id: created.id,
    referenceNo: detail.referenceNo,
    academicYearId,
    student,
    bookId: book.id,
    parentId: student.parents[0]!.parentId,
  }
}

async function seedTeacherIssue(page: Page, csrfToken: string) {
  const academicYearId = await pickAcademicYearId(page)
  const teacher = await pickTeacher(page)
  const book = await pickBookWithStock(page)

  const created = await bffPost<TeacherIssueCreateResponse>(page, '/TeacherIssues', {
    academicYearId,
    teacherId: teacher.id,
    expectedReturnDate: null,
    notes: 'e2e report seed teacher issue',
    items: [{ bookId: book.id, quantity: 1 }],
  }, csrfToken)

  await bffPost<string>(page, `/TeacherIssues/${created.id}/finalize`, {}, csrfToken)
  const detail = await bffGet<TeacherIssueDetail>(page, `/TeacherIssues/${created.id}`)
  return { id: detail.id, referenceNo: detail.referenceNo, teacher }
}

test.describe('reports and settings smoke @smoke @reports @settings', () => {
  test.setTimeout(120000)

  test('distribution report apply/reset and seeded reference visibility', async ({ page }) => {
    const csrfToken = await loginAsAdmin(page)
    const seeded = await seedDistribution(page, csrfToken)

    await page.goto('/admin/reports/distributions')
    await expect(page.getByRole('heading', { name: 'Distribution Summary' })).toBeVisible()

    await page.getByRole('button', { name: 'Apply' }).click()
    await expect(page.getByRole('cell', { name: seeded.referenceNo }).first()).toBeVisible()

    await page.locator('button:has(.pi-refresh)').first().click()
    await expect(page.getByText('Academic year is required for this report.')).toBeVisible()
  })

  test('stock summary pagination and filter reset', async ({ page }) => {
    await loginAsAdmin(page)
    await page.goto('/admin/reports/stock-summary')
    await expect(page.getByRole('heading', { name: 'Stock Summary' })).toBeVisible()

    const gradeInput = page.getByPlaceholder('Filter by grade')
    await gradeInput.fill('Grade 1')
    await page.getByRole('button', { name: 'Apply' }).click()
    await expect(gradeInput).toHaveValue('Grade 1')

    await page.locator('button:has(.pi-refresh)').first().click()
    await expect(gradeInput).toHaveValue('')

    const rppDropdown = page.locator('.p-paginator-rpp-dropdown').first()
    await rppDropdown.click()
    await page.locator('.p-select-option', { hasText: '10' }).first().click()
    await expect(rppDropdown).toContainText('10')
  })

  test('teacher outstanding report shows seeded issue and teacher filter reset', async ({ page }) => {
    const csrfToken = await loginAsAdmin(page)
    const seeded = await seedTeacherIssue(page, csrfToken)

    await page.goto('/admin/reports/teacher-outstanding')
    await expect(page.getByRole('heading', { name: 'Teacher Outstanding' })).toBeVisible()
    await expect(page.getByRole('cell', { name: seeded.referenceNo }).first()).toBeVisible()

    const teacherFilter = page.getByRole('combobox', { name: 'Filter by teacher' })
    await teacherFilter.click()
    await page.locator('.p-select-option', { hasText: seeded.teacher.fullName }).first().click()
    await page.getByRole('button', { name: 'Apply' }).click()
    await expect(page.getByRole('cell', { name: seeded.referenceNo }).first()).toBeVisible()

    await page.locator('button:has(.pi-refresh)').first().click()
    await expect(teacherFilter).toContainText('Filter by teacher')
  })

  test('student history enforces explicit student selection before apply', async ({ page }) => {
    await loginAsAdmin(page)
    await page.goto('/admin/reports/student-history')
    await expect(page.getByRole('heading', { name: 'Student History' })).toBeVisible()
    await expect(page.getByRole('button', { name: 'Apply' })).toBeDisabled()
    await expect(page.getByText('Select a student to load history.')).toBeVisible()
  })

  test('audit log list loads and filter reset clears criteria', async ({ page }) => {
    const csrfToken = await loginAsAdmin(page)
    await seedDistribution(page, csrfToken)

    await page.goto('/admin/audit-log')
    await expect(page.getByRole('heading', { name: 'Audit Log' })).toBeVisible()

    const entityTypeInput = page.getByPlaceholder('Entity type')
    const actionInput = page.getByPlaceholder('Action')
    await entityTypeInput.fill('Distribution')
    await actionInput.fill('Create')
    await page.getByRole('button', { name: 'Apply' }).click()
    await expect(page.locator('tbody tr').first()).toBeVisible()

    await page.locator('button:has(.pi-refresh)').first().click()
    await expect(entityTypeInput).toHaveValue('')
    await expect(actionInput).toHaveValue('')
  })

  test('settings pages load for admin and are blocked for user role', async ({ page }) => {
    await loginAsAdmin(page)

    await page.goto('/admin/settings/users')
    await expect(page.getByRole('heading', { name: 'Users' })).toBeVisible()

    await page.goto('/admin/settings/reference-formats')
    await expect(page.getByRole('heading', { name: 'Reference Formats' })).toBeVisible()

    await page.goto('/admin/settings/slip-templates')
    await expect(page.getByRole('heading', { name: 'Slip Templates' })).toBeVisible()

    await page.goto('/admin/settings/profile')
    await expect(page.getByText('Change Password')).toBeVisible()
  })

  test('role gate blocks user from settings and audit log', async ({ page }) => {
    await loginAsUser(page)

    await page.goto('/admin/settings/users')
    await expect(page).toHaveURL(/\/distribution$/)

    await page.goto('/admin/audit-log')
    await expect(page).toHaveURL(/\/distribution$/)
  })
})
