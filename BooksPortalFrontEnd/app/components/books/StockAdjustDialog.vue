<template>
  <Dialog
    :visible="visible"
    modal
    header="Adjust Stock"
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
        field-id="adjustYear"
        :error="errors.academicYearId"
      >
        <Select
          id="adjustYear"
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
        label="Movement Type"
        required
        field-id="movementType"
        :error="errors.movementType"
      >
        <Select
          id="movementType"
          v-model="localForm.movementType"
          :options="movementTypeOptions"
          option-label="label"
          option-value="value"
          placeholder="Select movement type"
          :invalid="!!errors.movementType"
          fluid
        />
      </FormsFormField>

      <FormsFormField
        label="Quantity"
        required
        field-id="adjustQty"
        :error="errors.quantity"
      >
        <InputNumber
          id="adjustQty"
          v-model="localForm.quantity"
          :min="1"
          :use-grouping="false"
          :invalid="!!errors.quantity"
          fluid
        />
      </FormsFormField>

      <FormsFormField
        label="Notes"
        field-id="adjustNotes"
      >
        <Textarea
          id="adjustNotes"
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
          label="Apply"
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

interface AdjustStockFormModel {
  academicYearId: number | null
  movementType: number | null
  quantity: number | null
  notes?: string
}

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'submit', payload: AdjustStockFormModel): void
  (e: 'cancel' | 'dismiss'): void
}>()

const props = defineProps<{
  visible: boolean
  loading: boolean
  academicYearOptions: OptionItem[]
  movementTypeOptions: OptionItem[]
  form: AdjustStockFormModel
  errors: Record<string, string>
}>()

const localForm = reactive<AdjustStockFormModel>({
  academicYearId: props.form.academicYearId,
  movementType: props.form.movementType,
  quantity: props.form.quantity,
  notes: props.form.notes ?? '',
})

watch(
  () => props.visible,
  (visible) => {
    if (!visible) return
    localForm.academicYearId = props.form.academicYearId
    localForm.movementType = props.form.movementType
    localForm.quantity = props.form.quantity
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
