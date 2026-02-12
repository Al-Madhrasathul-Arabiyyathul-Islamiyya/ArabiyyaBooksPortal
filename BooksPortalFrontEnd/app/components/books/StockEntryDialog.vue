<template>
  <Dialog
    :visible="visible"
    modal
    header="Add Stock Entry"
    :style="{ width: '34rem' }"
    @update:visible="onVisibleChange"
  >
    <form
      class="grid gap-4"
      @submit.prevent="emit('submit', { ...localForm })"
    >
      <FormsFormField
        label="Academic Year"
        required
        field-id="stockEntryYear"
        :error="errors.academicYearId"
      >
        <Select
          id="stockEntryYear"
          v-model="localForm.academicYearId"
          :options="academicYearOptions"
          option-label="label"
          option-value="value"
          placeholder="Select academic year"
          :invalid="!!errors.academicYearId"
          fluid
        >
          <template #option="{ option }">
            <div class="flex items-center gap-2">
              <span>{{ option.label }}</span>
              <Tag
                v-if="option.isActive"
                value="Active"
                severity="success"
              />
            </div>
          </template>
        </Select>
      </FormsFormField>

      <FormsFormField
        label="Quantity"
        required
        field-id="stockEntryQuantity"
        :error="errors.quantity"
      >
        <InputNumber
          id="stockEntryQuantity"
          v-model="localForm.quantity"
          :min="1"
          :use-grouping="false"
          :invalid="!!errors.quantity"
          fluid
        />
      </FormsFormField>

      <FormsFormField
        label="Source"
        field-id="stockEntrySource"
      >
        <InputText
          id="stockEntrySource"
          v-model.trim="localForm.source"
          fluid
        />
      </FormsFormField>

      <FormsFormField
        label="Notes"
        field-id="stockEntryNotes"
      >
        <Textarea
          id="stockEntryNotes"
          v-model.trim="localForm.notes"
          rows="3"
          fluid
        />
      </FormsFormField>

      <div class="flex justify-end gap-2">
        <Button
          type="button"
          label="Cancel"
          severity="secondary"
          text
          @click="emit('cancel')"
        />
        <Button
          type="submit"
          label="Add Stock"
          :loading="loading"
        />
      </div>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
interface OptionItem {
  label: string
  value: number
  isActive?: boolean
}

interface StockEntryFormModel {
  academicYearId: number | null
  quantity: number | null
  source?: string
  notes?: string
}

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'submit', payload: StockEntryFormModel): void
  (e: 'cancel' | 'dismiss'): void
}>()

const props = defineProps<{
  visible: boolean
  loading: boolean
  academicYearOptions: OptionItem[]
  form: StockEntryFormModel
  errors: Record<string, string>
}>()

const localForm = reactive<StockEntryFormModel>({
  academicYearId: props.form.academicYearId,
  quantity: props.form.quantity,
  source: props.form.source ?? '',
  notes: props.form.notes ?? '',
})

watch(
  () => props.visible,
  (visible) => {
    if (!visible) return
    localForm.academicYearId = props.form.academicYearId
    localForm.quantity = props.form.quantity
    localForm.source = props.form.source ?? ''
    localForm.notes = props.form.notes ?? ''
  },
)

function onVisibleChange(value: boolean) {
  emit('update:visible', value)
  if (!value) {
    emit('dismiss')
  }
}
</script>
