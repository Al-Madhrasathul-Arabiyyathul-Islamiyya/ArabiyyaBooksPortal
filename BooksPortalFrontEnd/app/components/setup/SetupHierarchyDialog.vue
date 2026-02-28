<template>
  <Dialog
    v-model:visible="model"
    modal
    header="Configure Academic Year & Master Data"
    :style="{ width: '74rem' }"
  >
    <div class="grid gap-4">
      <Tabs value="bulk">
        <TabList>
          <Tab value="bulk">
            Bulk JSON Upload
          </Tab>
          <Tab value="manual">
            Manual Entry
          </Tab>
        </TabList>
        <TabPanels>
          <TabPanel value="bulk">
            <div class="grid gap-4">
              <div class="flex justify-end">
                <Button
                  label="Download JSON Template"
                  icon="pi pi-download"
                  :loading="isDownloadingTemplate"
                  @click="downloadTemplate"
                />
              </div>
              <Message
                severity="info"
                :closable="false"
              >
                Upload the JSON template with academic years, keystages, grades, and class sections.
              </Message>
              <FileUpload
                mode="basic"
                choose-label="Choose JSON File"
                choose-icon="pi pi-upload"
                accept="application/json,.json"
                :custom-upload="true"
                :auto="false"
                @select="onFileSelected"
              />
              <div
                v-if="selectedFileName"
                class="text-sm text-surface-600 dark:text-surface-400"
              >
                Selected: <span class="font-medium">{{ selectedFileName }}</span>
              </div>
              <div class="flex justify-end gap-2">
                <Button
                  label="Clear"
                  severity="secondary"
                  text
                  :disabled="!selectedFile"
                  @click="clearSelection"
                />
                <Button
                  label="Upload & Apply"
                  icon="pi pi-check"
                  :loading="isSubmittingBulk"
                  :disabled="!selectedFile"
                  @click="submitBulk"
                />
              </div>
              <Card v-if="bulkResult">
                <template #title>
                  Upload Result
                </template>
                <template #content>
                  <div class="grid gap-3">
                    <div class="grid gap-2 sm:grid-cols-2 lg:grid-cols-5">
                      <Tag
                        :value="`Created: ${bulkResult.createdCount}`"
                        severity="success"
                      />
                      <Tag
                        :value="`Updated: ${bulkResult.updatedCount}`"
                        severity="info"
                      />
                      <Tag
                        :value="`Skipped: ${bulkResult.skippedCount}`"
                        severity="secondary"
                      />
                      <Tag
                        :value="`Failed: ${bulkResult.failedCount}`"
                        severity="danger"
                      />
                      <Tag
                        :value="`Rows: ${bulkResult.results.length}`"
                        severity="contrast"
                      />
                    </div>
                  </div>
                </template>
              </Card>
            </div>
          </TabPanel>

          <TabPanel value="manual">
            <div class="grid gap-4">
              <Message
                severity="info"
                :closable="false"
              >
                Use manual entry if you want to customize values before confirming setup.
              </Message>

              <Accordion
                :value="['ay']"
                multiple
              >
                <AccordionPanel value="ay">
                  <AccordionHeader>Academic Year</AccordionHeader>
                  <AccordionContent>
                    <div class="grid gap-4 md:grid-cols-2">
                      <FormsFormField
                        label="Name"
                        required
                        field-id="setupAyName"
                      >
                        <InputText
                          id="setupAyName"
                          v-model.trim="academicYearForm.name"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Year"
                        required
                        field-id="setupAyYear"
                      >
                        <InputNumber
                          id="setupAyYear"
                          v-model="academicYearForm.year"
                          :min="2000"
                          :max="2100"
                          :use-grouping="false"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Start Date"
                        required
                        field-id="setupAyStart"
                      >
                        <DatePicker
                          id="setupAyStart"
                          v-model="academicYearForm.startDate"
                          show-icon
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="End Date"
                        required
                        field-id="setupAyEnd"
                      >
                        <DatePicker
                          id="setupAyEnd"
                          v-model="academicYearForm.endDate"
                          show-icon
                          fluid
                        />
                      </FormsFormField>
                    </div>
                    <div class="mt-3 flex justify-end">
                      <Button
                        label="Add Academic Year"
                        :loading="isSavingAcademicYear"
                        @click="createAcademicYear"
                      />
                    </div>
                  </AccordionContent>
                </AccordionPanel>

                <AccordionPanel value="active">
                  <AccordionHeader>Set Active Academic Year</AccordionHeader>
                  <AccordionContent>
                    <div class="grid gap-4 md:grid-cols-[1fr_auto]">
                      <Select
                        v-model="selectedActiveAcademicYearId"
                        :options="academicYearOptions"
                        option-label="label"
                        option-value="value"
                        placeholder="Select academic year"
                        fluid
                      />
                      <Button
                        label="Set Active"
                        icon="pi pi-check-circle"
                        :loading="isActivatingAcademicYear"
                        :disabled="!selectedActiveAcademicYearId"
                        @click="activateAcademicYear"
                      />
                    </div>
                  </AccordionContent>
                </AccordionPanel>

                <AccordionPanel value="keystage">
                  <AccordionHeader>Keystage</AccordionHeader>
                  <AccordionContent>
                    <div class="grid gap-4 md:grid-cols-3">
                      <FormsFormField
                        label="Code"
                        required
                        field-id="setupKsCode"
                      >
                        <InputText
                          id="setupKsCode"
                          v-model.trim="keystageForm.code"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Name"
                        required
                        field-id="setupKsName"
                      >
                        <InputText
                          id="setupKsName"
                          v-model.trim="keystageForm.name"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Sort Order"
                        required
                        field-id="setupKsSortOrder"
                      >
                        <InputNumber
                          id="setupKsSortOrder"
                          v-model="keystageForm.sortOrder"
                          :min="0"
                          :use-grouping="false"
                          fluid
                        />
                      </FormsFormField>
                    </div>
                    <div class="mt-3 flex justify-end">
                      <Button
                        label="Add Keystage"
                        :loading="isSavingKeystage"
                        @click="createKeystage"
                      />
                    </div>
                  </AccordionContent>
                </AccordionPanel>

                <AccordionPanel value="grade">
                  <AccordionHeader>Grade</AccordionHeader>
                  <AccordionContent>
                    <div class="grid gap-4 md:grid-cols-4">
                      <FormsFormField
                        label="Keystage"
                        required
                        field-id="setupGradeKeystage"
                      >
                        <Select
                          id="setupGradeKeystage"
                          v-model="gradeForm.keystageId"
                          :options="keystageOptions"
                          option-label="label"
                          option-value="value"
                          placeholder="Select keystage"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Code"
                        required
                        field-id="setupGradeCode"
                      >
                        <InputText
                          id="setupGradeCode"
                          v-model.trim="gradeForm.code"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Name"
                        required
                        field-id="setupGradeName"
                      >
                        <InputText
                          id="setupGradeName"
                          v-model.trim="gradeForm.name"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Sort Order"
                        required
                        field-id="setupGradeSortOrder"
                      >
                        <InputNumber
                          id="setupGradeSortOrder"
                          v-model="gradeForm.sortOrder"
                          :min="0"
                          :use-grouping="false"
                          fluid
                        />
                      </FormsFormField>
                    </div>
                    <div class="mt-3 flex justify-end">
                      <Button
                        label="Add Grade"
                        :loading="isSavingGrade"
                        @click="createGrade"
                      />
                    </div>
                  </AccordionContent>
                </AccordionPanel>

                <AccordionPanel value="class">
                  <AccordionHeader>Class Section</AccordionHeader>
                  <AccordionContent>
                    <div class="grid gap-4 md:grid-cols-4">
                      <FormsFormField
                        label="Academic Year"
                        required
                        field-id="setupClassYear"
                      >
                        <Select
                          id="setupClassYear"
                          v-model="classSectionForm.academicYearId"
                          :options="academicYearOptions"
                          option-label="label"
                          option-value="value"
                          placeholder="Select year"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Keystage"
                        required
                        field-id="setupClassKeystage"
                      >
                        <Select
                          id="setupClassKeystage"
                          v-model="classSectionForm.keystageId"
                          :options="keystageOptions"
                          option-label="label"
                          option-value="value"
                          placeholder="Select keystage"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Grade"
                        required
                        field-id="setupClassGrade"
                      >
                        <Select
                          id="setupClassGrade"
                          v-model="classSectionForm.gradeId"
                          :options="gradeOptions"
                          option-label="label"
                          option-value="value"
                          placeholder="Select grade"
                          fluid
                        />
                      </FormsFormField>
                      <FormsFormField
                        label="Section"
                        required
                        field-id="setupClassSection"
                      >
                        <InputText
                          id="setupClassSection"
                          v-model.trim="classSectionForm.section"
                          fluid
                        />
                      </FormsFormField>
                    </div>
                    <div class="mt-3 flex justify-end">
                      <Button
                        label="Add Class Section"
                        :loading="isSavingClassSection"
                        @click="createClassSection"
                      />
                    </div>
                  </AccordionContent>
                </AccordionPanel>
              </Accordion>
            </div>
          </TabPanel>
        </TabPanels>
      </Tabs>
    </div>
  </Dialog>
</template>

<script setup lang="ts">
import type { AcademicYear, Grade, Keystage } from '~/types/entities'
import {
  CreateAcademicYearRequestSchema,
  CreateClassSectionRequestSchema,
  CreateGradeRequestSchema,
  CreateKeystageRequestSchema,
} from '~/types/forms'
import { API } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

type HierarchyBulkResultRow = {
  path: string
  entityType: string
  operation: string
  message: string
}

type HierarchyBulkResponse = {
  createdCount: number
  updatedCount: number
  skippedCount: number
  failedCount: number
  results: HierarchyBulkResultRow[]
}

const model = defineModel<boolean>('visible', { default: false })
const emit = defineEmits<{ refreshed: [] }>()

const api = useApi()
const { showSuccess, showError } = useAppToast()

const selectedFile = ref<File | null>(null)
const selectedFileName = ref('')
const isSubmittingBulk = ref(false)
const isDownloadingTemplate = ref(false)
const bulkResult = ref<HierarchyBulkResponse | null>(null)

const academicYears = ref<AcademicYear[]>([])
const keystages = ref<Keystage[]>([])
const grades = ref<Grade[]>([])
const selectedActiveAcademicYearId = ref<number | null>(null)

const isSavingAcademicYear = ref(false)
const isSavingKeystage = ref(false)
const isSavingGrade = ref(false)
const isSavingClassSection = ref(false)
const isActivatingAcademicYear = ref(false)

const academicYearForm = reactive({
  name: '',
  year: new Date().getFullYear(),
  startDate: null as Date | null,
  endDate: null as Date | null,
})

const keystageForm = reactive({
  code: '',
  name: '',
  sortOrder: 0,
})

const gradeForm = reactive({
  keystageId: null as number | null,
  code: '',
  name: '',
  sortOrder: 0,
})

const classSectionForm = reactive({
  academicYearId: null as number | null,
  keystageId: null as number | null,
  gradeId: null as number | null,
  section: '',
})

const academicYearOptions = computed(() =>
  academicYears.value.map(year => ({
    label: year.name,
    value: year.id,
  })),
)

const keystageOptions = computed(() =>
  keystages.value.map(stage => ({
    label: stage.name,
    value: stage.id,
  })),
)

const gradeOptions = computed(() =>
  grades.value.map(grade => ({
    label: grade.name,
    value: grade.id,
  })),
)

function onFileSelected(event: { files: File[] }) {
  const file = event.files?.[0]
  if (!file) return
  selectedFile.value = file
  selectedFileName.value = file.name
}

function clearSelection() {
  selectedFile.value = null
  selectedFileName.value = ''
}

async function downloadTemplate() {
  isDownloadingTemplate.value = true
  try {
    await api.downloadBlob(API.importTemplates.masterDataHierarchy, 'master-data-hierarchy-template.json')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to download template'))
  }
  finally {
    isDownloadingTemplate.value = false
  }
}

async function submitBulk() {
  if (!selectedFile.value) return
  isSubmittingBulk.value = true
  try {
    const text = await selectedFile.value.text()
    const payload = JSON.parse(text)
    const response = await api.post<HierarchyBulkResponse>(API.masterDataHierarchy.bulkUpsert, payload)
    if (response.success) {
      bulkResult.value = response.data
      showSuccess(response.message ?? 'Hierarchy bulk upload completed')
      emit('refreshed')
      await loadLookups()
      return
    }
    showError(response.message ?? 'Hierarchy bulk upload failed')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Invalid JSON or failed to upload hierarchy payload'))
  }
  finally {
    isSubmittingBulk.value = false
  }
}

async function loadLookups() {
  try {
    const [yearsResponse, keystagesResponse] = await Promise.all([
      api.get<AcademicYear[]>(API.academicYears.base),
      api.get<Keystage[]>(API.keystages.base),
    ])
    if (yearsResponse.success) academicYears.value = yearsResponse.data
    if (keystagesResponse.success) keystages.value = keystagesResponse.data

    if (classSectionForm.keystageId) {
      const gradesResponse = await api.get<Grade[]>(API.lookups.grades, { keystageId: classSectionForm.keystageId })
      if (gradesResponse.success) grades.value = gradesResponse.data
    }
  }
  catch {
    // non-blocking
  }
}

async function createAcademicYear() {
  if (!academicYearForm.startDate || !academicYearForm.endDate) {
    showError('Start and end dates are required')
    return
  }
  const parsed = CreateAcademicYearRequestSchema.safeParse({
    name: academicYearForm.name,
    year: academicYearForm.year,
    startDate: academicYearForm.startDate.toISOString(),
    endDate: academicYearForm.endDate.toISOString(),
  })
  if (!parsed.success) {
    showError(parsed.error.issues[0]?.message ?? 'Invalid academic year data')
    return
  }

  isSavingAcademicYear.value = true
  try {
    const response = await api.post<AcademicYear>(API.academicYears.base, parsed.data)
    if (response.success) {
      showSuccess('Academic year added')
      academicYearForm.name = ''
      academicYearForm.startDate = null
      academicYearForm.endDate = null
      emit('refreshed')
      await loadLookups()
      return
    }
    showError(response.message ?? 'Failed to add academic year')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to add academic year'))
  }
  finally {
    isSavingAcademicYear.value = false
  }
}

async function activateAcademicYear() {
  if (!selectedActiveAcademicYearId.value) return
  isActivatingAcademicYear.value = true
  try {
    const response = await api.post<string>(API.academicYears.activate(selectedActiveAcademicYearId.value))
    if (response.success) {
      showSuccess(response.message ?? 'Active academic year updated')
      emit('refreshed')
      await loadLookups()
      return
    }
    showError(response.message ?? 'Failed to activate academic year')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to activate academic year'))
  }
  finally {
    isActivatingAcademicYear.value = false
  }
}

async function createKeystage() {
  const parsed = CreateKeystageRequestSchema.safeParse(keystageForm)
  if (!parsed.success) {
    showError(parsed.error.issues[0]?.message ?? 'Invalid keystage data')
    return
  }
  isSavingKeystage.value = true
  try {
    const response = await api.post<Keystage>(API.keystages.base, parsed.data)
    if (response.success) {
      showSuccess('Keystage added')
      keystageForm.code = ''
      keystageForm.name = ''
      keystageForm.sortOrder = 0
      emit('refreshed')
      await loadLookups()
      return
    }
    showError(response.message ?? 'Failed to add keystage')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to add keystage'))
  }
  finally {
    isSavingKeystage.value = false
  }
}

async function createGrade() {
  const parsed = CreateGradeRequestSchema.safeParse({
    ...gradeForm,
    keystageId: gradeForm.keystageId ?? 0,
  })
  if (!parsed.success) {
    showError(parsed.error.issues[0]?.message ?? 'Invalid grade data')
    return
  }
  isSavingGrade.value = true
  try {
    const response = await api.post<Grade>(API.grades.base, parsed.data)
    if (response.success) {
      showSuccess('Grade added')
      gradeForm.code = ''
      gradeForm.name = ''
      gradeForm.sortOrder = 0
      emit('refreshed')
      await loadLookups()
      return
    }
    showError(response.message ?? 'Failed to add grade')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to add grade'))
  }
  finally {
    isSavingGrade.value = false
  }
}

async function createClassSection() {
  const parsed = CreateClassSectionRequestSchema.safeParse({
    ...classSectionForm,
    academicYearId: classSectionForm.academicYearId ?? 0,
    keystageId: classSectionForm.keystageId ?? 0,
    gradeId: classSectionForm.gradeId ?? 0,
  })
  if (!parsed.success) {
    showError(parsed.error.issues[0]?.message ?? 'Invalid class section data')
    return
  }
  isSavingClassSection.value = true
  try {
    const response = await api.post<string>(API.classSections.base, parsed.data)
    if (response.success) {
      showSuccess('Class section added')
      classSectionForm.section = ''
      emit('refreshed')
      return
    }
    showError(response.message ?? 'Failed to add class section')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to add class section'))
  }
  finally {
    isSavingClassSection.value = false
  }
}

watch(() => classSectionForm.keystageId, async (keystageId) => {
  classSectionForm.gradeId = null
  if (!keystageId) {
    grades.value = []
    return
  }
  const response = await api.get<Grade[]>(API.lookups.grades, { keystageId })
  if (response.success) grades.value = response.data
})

watch(model, (visible) => {
  if (visible) void loadLookups()
})
</script>
