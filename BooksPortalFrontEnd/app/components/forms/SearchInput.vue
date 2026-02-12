<template>
  <IconField>
    <InputIcon class="pi pi-search" />
    <InputText
      :id="id"
      v-model="searchTerm"
      :name="name"
      :placeholder="placeholder"
      :autocomplete="autocomplete"
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
  id?: string
  modelValue?: string
  name?: string
  placeholder?: string
  debounce?: number
  autocomplete?: string
  persistKey?: string
}>(), {
  id: undefined,
  modelValue: '',
  name: undefined,
  placeholder: 'Search...',
  debounce: 300,
  autocomplete: 'on',
  persistKey: undefined,
})

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'search': [value: string]
}>()

const searchTerm = ref(props.modelValue)

let debounceTimer: ReturnType<typeof setTimeout> | null = null

function persistValue(value: string) {
  if (!import.meta.client || !props.persistKey) return
  if (!value.trim()) {
    localStorage.removeItem(props.persistKey)
    return
  }
  localStorage.setItem(props.persistKey, value)
}

function handleInput() {
  emit('update:modelValue', searchTerm.value)
  persistValue(searchTerm.value)

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
  persistValue('')
}

watch(() => props.modelValue, (newValue) => {
  searchTerm.value = newValue
})

onMounted(() => {
  if (!import.meta.client || !props.persistKey || props.modelValue) return
  const saved = localStorage.getItem(props.persistKey)
  if (!saved) return
  searchTerm.value = saved
  emit('update:modelValue', saved)
  emit('search', saved)
})
</script>
