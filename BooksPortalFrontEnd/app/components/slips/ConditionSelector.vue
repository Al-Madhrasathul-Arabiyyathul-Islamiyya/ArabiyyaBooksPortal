<template>
  <div class="grid gap-3">
    <FormsFormField
      label="Condition"
      required
      field-id="bookCondition"
      :error="conditionError"
    >
      <Select
        id="bookCondition"
        v-model="conditionModel"
        :options="conditionOptions"
        option-label="label"
        option-value="value"
        placeholder="Select condition"
        fluid
        :invalid="!!conditionError"
      />
    </FormsFormField>

    <FormsFormField
      label="Condition Notes"
      field-id="conditionNotes"
      :error="notesError"
    >
      <Textarea
        id="conditionNotes"
        v-model.trim="notesModel"
        rows="3"
        fluid
        :invalid="!!notesError"
        placeholder="Optional notes about condition"
      />
    </FormsFormField>
  </div>
</template>

<script setup lang="ts">
import { conditionOptions } from '~/utils/formatters'

const conditionModel = defineModel<number | null>('condition', { default: null })
const notesModel = defineModel<string>('notes', { default: '' })

withDefaults(defineProps<{
  conditionError?: string
  notesError?: string
}>(), {
  conditionError: '',
  notesError: '',
})
</script>
