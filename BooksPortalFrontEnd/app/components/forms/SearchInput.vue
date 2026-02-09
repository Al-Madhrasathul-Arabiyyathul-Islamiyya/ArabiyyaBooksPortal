<template>
  <IconField>
    <InputIcon class="pi pi-search" />
    <InputText
      v-model="searchTerm"
      :placeholder="placeholder"
      class="w-full"
      @input="handleInput"
    />
    <InputIcon
      v-if="searchTerm"
      class="pi pi-times cursor-pointer"
      @click="clear"
    />
  </IconField>
</template>

<script setup lang="ts">
const props = withDefaults(defineProps<{
  modelValue?: string
  placeholder?: string
  debounce?: number
}>(), {
  modelValue: '',
  placeholder: 'Search...',
  debounce: 300,
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'search': [value: string]
}>()

const searchTerm = ref(props.modelValue)

let debounceTimer: ReturnType<typeof setTimeout> | null = null

function handleInput() {
  emit('update:modelValue', searchTerm.value)

  if (debounceTimer) {
    clearTimeout(debounceTimer)
  }

  debounceTimer = setTimeout(() => {
    emit('search', searchTerm.value)
  }, props.debounce)
}

function clear() {
  searchTerm.value = ''
  emit('update:modelValue', '')
  emit('search', '')
}

watch(() => props.modelValue, (newValue) => {
  searchTerm.value = newValue
})
</script>
