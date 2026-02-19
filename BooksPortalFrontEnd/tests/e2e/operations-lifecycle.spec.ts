import { expect, test, type Page } from '@playwright/test'
import { loginAsAdmin } from '~~/e2e/helpers/auth'
import { bffDelete, bffGet, bffPost } from '~~/e2e/helpers/api'
import type { Paginated } from '~~/e2e/helpers/types'

type Student = {
  id: number
  fullName: string
  parents: { parentId: number }[]
}

type Teacher = { id: number, fullName: string }
type Book = { id: number, title: string, available: number }

type DistributionCreateResponse = { id: number, referenceNo?: string }
type ReturnCreateResponse = { id: number, referenceNo?: string }
type TeacherIssueCreateResponse = { id: number, referenceNo?: string }
type TeacherReturnCreateResponse = { id: number, referenceNo?: string }

type TeacherIssueItem = { id: number, outstandingQuantity: number }
type TeacherIssueDetail = { id: number, items: TeacherIssueItem[] }
type DistributionDetail = { id: number, referenceNo: string }
type ReturnDetail = { id: number, referenceNo: string }
type TeacherIssueRead = { id: number, referenceNo: string }

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

async function createDistribution(page: Page, csrfToken: string) {
  const academicYearId = await pickAcademicYearId(page)
  const student = await pickStudentWithParent(page, academicYearId)
  const parentId = student.parents[0]!.parentId
  const book = await pickBookWithStock(page)

  const created = await bffPost<DistributionCreateResponse>(page, '/distributions', {
    academicYearId,
    term: 1,
    studentId: student.id,
    parentId,
    notes: 'e2e distribution lifecycle smoke',
    items: [{ bookId: book.id, quantity: 1 }],
  }, csrfToken)

  const detail = await bffGet<DistributionDetail>(page, `/distributions/${created.id}`)
  return { id: created.id, referenceNo: detail.referenceNo, bookId: book.id, studentId: student.id, parentId, academicYearId }
}

async function createReturn(page: Page, seed: { bookId: number, studentId: number, parentId: number, academicYearId: number }, csrfToken: string) {
  const created = await bffPost<ReturnCreateResponse>(page, '/returns', {
    academicYearId: seed.academicYearId,
    studentId: seed.studentId,
    returnedById: seed.parentId,
    notes: 'e2e return lifecycle smoke',
    items: [{
      bookId: seed.bookId,
      quantity: 1,
      condition: 1,
      conditionNotes: null,
    }],
  }, csrfToken)
  const detail = await bffGet<ReturnDetail>(page, `/returns/${created.id}`)
  return { id: created.id, referenceNo: detail.referenceNo }
}

async function createTeacherIssue(page: Page, csrfToken: string) {
  const academicYearId = await pickAcademicYearId(page)
  const teacher = await pickTeacher(page)
  const book = await pickBookWithStock(page)

  const created = await bffPost<TeacherIssueCreateResponse>(page, '/TeacherIssues', {
    academicYearId,
    teacherId: teacher.id,
    expectedReturnDate: null,
    notes: 'e2e teacher issue lifecycle smoke',
    items: [{ bookId: book.id, quantity: 1 }],
  }, csrfToken)

  const detail = await bffGet<TeacherIssueRead>(page, `/TeacherIssues/${created.id}`)
  return { id: created.id, referenceNo: detail.referenceNo }
}

async function createTeacherReturn(page: Page, teacherIssueId: number, csrfToken: string) {
  const issue = await bffGet<TeacherIssueDetail>(page, `/TeacherIssues/${teacherIssueId}`)
  const returnItem = issue.items.find(item => item.outstandingQuantity > 0)
  expect(returnItem, 'Expected outstanding item after finalizing teacher issue').toBeTruthy()

  const created = await bffPost<TeacherReturnCreateResponse>(page, `/TeacherIssues/${teacherIssueId}/return`, {
    notes: 'e2e teacher return lifecycle smoke',
    items: [{ teacherIssueItemId: returnItem!.id, quantity: 1 }],
  }, csrfToken)

  return { id: created.id }
}

async function expectSlipStatus(page: Page, url: string, heading: string, status: 'Processing' | 'Finalized' | 'Cancelled') {
  await page.goto(url)
  await expect(page.getByRole('heading', { name: heading })).toBeVisible()
  await expect(page.getByText(status).first()).toBeVisible()
  await expect(page.getByRole('button', { name: 'Print' })).toBeVisible()
}

test.describe('operations lifecycle smoke', () => {
  test.setTimeout(120000)
  let csrfToken: string

  test.beforeEach(async ({ page }) => {
    csrfToken = await loginAsAdmin(page)
  })

  test('distribution lifecycle: create processing -> finalize finalized', async ({ page }) => {
    const distribution = await createDistribution(page, csrfToken)
    await expectSlipStatus(page, `/distribution/${distribution.id}`, 'Distribution Slip', 'Processing')

    await bffPost<string>(page, `/distributions/${distribution.id}/finalize`, {}, csrfToken)
    await expectSlipStatus(page, `/distribution/${distribution.id}`, 'Distribution Slip', 'Finalized')
  })

  test('returns lifecycle: create processing -> finalize finalized', async ({ page }) => {
    const distribution = await createDistribution(page, csrfToken)
    const returned = await createReturn(page, distribution, csrfToken)
    await expectSlipStatus(page, `/returns/${returned.id}`, 'Return Slip', 'Processing')

    await bffPost<string>(page, `/returns/${returned.id}/finalize`, {}, csrfToken)
    await expectSlipStatus(page, `/returns/${returned.id}`, 'Return Slip', 'Finalized')
  })

  test('teacher issue lifecycle: create processing -> finalize finalized', async ({ page }) => {
    const issue = await createTeacherIssue(page, csrfToken)
    await expectSlipStatus(page, `/teacher-issues/${issue.id}`, 'Teacher Issue Slip', 'Processing')

    await bffPost<string>(page, `/TeacherIssues/${issue.id}/finalize`, {}, csrfToken)
    await expectSlipStatus(page, `/teacher-issues/${issue.id}`, 'Teacher Issue Slip', 'Finalized')
  })

  test('teacher return lifecycle: create processing -> finalize finalized', async ({ page }) => {
    const issue = await createTeacherIssue(page, csrfToken)
    await bffPost<string>(page, `/TeacherIssues/${issue.id}/finalize`, {}, csrfToken)
    const teacherReturn = await createTeacherReturn(page, issue.id, csrfToken)

    await expectSlipStatus(page, `/teacher-returns/${teacherReturn.id}`, 'Teacher Return Slip', 'Processing')

    await bffPost<string>(page, `/TeacherReturns/${teacherReturn.id}/finalize`, {}, csrfToken)
    await expectSlipStatus(page, `/teacher-returns/${teacherReturn.id}`, 'Teacher Return Slip', 'Finalized')
  })

  test('distribution lifecycle: cancel hides record from default list', async ({ page }) => {
    const distribution = await createDistribution(page, csrfToken)
    await bffDelete<string>(page, `/distributions/${distribution.id}`, csrfToken)

    const list = await bffGet<Paginated<DistributionDetail>>(page, '/distributions', {
      pageNumber: 1,
      pageSize: 50,
      search: distribution.referenceNo,
      includeCancelled: false,
    })
    expect(list.items.some(item => item.referenceNo === distribution.referenceNo)).toBeFalsy()
  })
})
