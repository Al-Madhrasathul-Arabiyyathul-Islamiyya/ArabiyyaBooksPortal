# Books Portal - Frontend Structure

## Technology Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| Nuxt | 4.3.0 | Vue framework with SSR support |
| Vue | 3.x | Reactive UI framework |
| TypeScript | 5.x | Type safety |
| PrimeVue | 4.x | UI component library |
| Pinia | 2.x | State management |
| VueUse | Latest | Composition utilities |
| Tailwind CSS | 3.x | Utility-first CSS |
| Day.js | Latest | Date manipulation |
| Zod | Latest | Schema validation |

---

## Project Structure

```
client/
├── assets/
│   ├── css/
│   │   ├── main.css              # Global styles
│   │   └── primevue-theme.css    # PrimeVue customizations
│   └── images/
│       └── logo.svg
│
├── components/
│   ├── common/
│   │   ├── AppHeader.vue
│   │   ├── AppSidebar.vue
│   │   ├── AppBreadcrumb.vue
│   │   ├── DataTable.vue         # Wrapper with common config
│   │   ├── ConfirmDialog.vue
│   │   ├── LoadingSpinner.vue
│   │   └── EmptyState.vue
│   │
│   ├── forms/
│   │   ├── FormField.vue
│   │   ├── SearchInput.vue
│   │   ├── DatePicker.vue
│   │   └── YearSelector.vue
│   │
│   ├── books/
│   │   ├── BookCard.vue
│   │   ├── BookSearch.vue
│   │   ├── BookSelector.vue      # Multi-select for slips
│   │   ├── StockBadge.vue
│   │   ├── StockEntryDialog.vue
│   │   └── StockAdjustDialog.vue
│   │
│   ├── distribution/
│   │   ├── DistributionForm.vue
│   │   ├── StudentLookup.vue
│   │   ├── ParentLookup.vue
│   │   ├── SlipItemsTable.vue
│   │   └── SlipPreview.vue
│   │
│   ├── returns/
│   │   ├── ReturnForm.vue
│   │   ├── ConditionSelector.vue
│   │   └── ReturnItemsTable.vue
│   │
│   ├── teachers/
│   │   ├── TeacherIssueForm.vue
│   │   ├── TeacherLookup.vue
│   │   └── OutstandingTable.vue
│   │
│   └── reports/
│       ├── StockSummaryCard.vue
│       ├── DistributionChart.vue
│       ├── MovementTimeline.vue
│       └── ExportButton.vue
│
├── composables/
│   ├── useApi.ts                 # API client
│   ├── useAuth.ts                # Auth utilities
│   ├── usePagination.ts          # Pagination helpers
│   ├── useToast.ts               # Toast notifications
│   ├── useConfirm.ts             # Confirmation dialogs
│   ├── useAcademicYear.ts        # Active year context
│   └── usePrint.ts               # Print utilities
│
├── layouts/
│   ├── default.vue               # Main app layout
│   ├── auth.vue                  # Login page layout
│   └── print.vue                 # Print-only layout
│
├── middleware/
│   ├── auth.ts                   # Authentication check
│   └── role.ts                   # Role-based access
│
├── pages/
│   ├── index.vue                 # Dashboard
│   ├── login.vue                 # Login page
│   │
│   ├── books/
│   │   ├── index.vue             # Book list
│   │   ├── [id].vue              # Book details
│   │   └── create.vue            # Create book
│   │
│   ├── distribution/
│   │   ├── index.vue             # Distribution list
│   │   ├── [id].vue              # Distribution details
│   │   ├── create.vue            # Create distribution
│   │   └── print/
│   │       └── [id].vue          # Print view
│   │
│   ├── returns/
│   │   ├── index.vue
│   │   ├── [id].vue
│   │   ├── create.vue
│   │   └── print/
│   │       └── [id].vue
│   │
│   ├── teacher-issues/
│   │   ├── index.vue
│   │   ├── [id].vue
│   │   ├── create.vue
│   │   └── print/
│   │       └── [id].vue
│   │
│   ├── master-data/
│   │   ├── academic-years/
│   │   │   ├── index.vue
│   │   │   └── [id].vue
│   │   ├── keystages/
│   │   │   ├── index.vue
│   │   │   └── [id].vue
│   │   ├── subjects/
│   │   │   ├── index.vue
│   │   │   └── [id].vue
│   │   ├── class-sections/
│   │   │   ├── index.vue
│   │   │   └── [id].vue
│   │   ├── students/
│   │   │   ├── index.vue
│   │   │   └── [id].vue
│   │   ├── parents/
│   │   │   ├── index.vue
│   │   │   └── [id].vue
│   │   └── teachers/
│   │       ├── index.vue
│   │       └── [id].vue
│   │
│   ├── reports/
│   │   ├── index.vue             # Report hub
│   │   ├── stock-summary.vue
│   │   ├── distributions.vue
│   │   ├── returns.vue
│   │   ├── teacher-outstanding.vue
│   │   └── movements.vue
│   │
│   ├── audit-log/
│   │   └── index.vue
│   │
│   └── settings/
│       ├── users/
│       │   ├── index.vue
│       │   └── [id].vue
│       └── profile.vue
│
├── plugins/
│   ├── primevue.ts               # PrimeVue setup
│   └── dayjs.ts                  # Day.js setup
│
├── stores/
│   ├── auth.ts                   # Auth state
│   ├── app.ts                    # App settings (sidebar, theme)
│   ├── academicYear.ts           # Active year
│   └── lookups.ts                # Cached lookups
│
├── types/
│   ├── api.ts                    # API response types
│   ├── entities.ts               # Entity interfaces
│   ├── forms.ts                  # Form types
│   └── enums.ts                  # Enum types
│
├── utils/
│   ├── formatters.ts             # Date, number formatters
│   ├── validators.ts             # Zod schemas
│   └── constants.ts              # App constants
│
├── app.vue
├── nuxt.config.ts
├── tailwind.config.ts
└── tsconfig.json
```

---

## Key Components

### AppSidebar.vue

```vue
<template>
  <aside class="sidebar" :class="{ collapsed: isCollapsed }">
    <div class="sidebar-header">
      <img src="~/assets/images/logo.svg" alt="Logo" class="logo" />
      <span v-if="!isCollapsed" class="app-name">Books Portal</span>
    </div>
    
    <nav class="sidebar-nav">
      <Menu :model="menuItems" />
    </nav>
    
    <div class="sidebar-footer">
      <div class="academic-year-selector">
        <Dropdown 
          v-model="activeYear" 
          :options="academicYears"
          optionLabel="name"
          optionValue="id"
          @change="onYearChange"
        />
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
const menuItems = ref([
  {
    label: 'Dashboard',
    icon: 'pi pi-home',
    to: '/'
  },
  {
    label: 'Books',
    icon: 'pi pi-book',
    to: '/books'
  },
  {
    label: 'Distribution',
    icon: 'pi pi-send',
    to: '/distribution'
  },
  {
    label: 'Returns',
    icon: 'pi pi-replay',
    to: '/returns'
  },
  {
    label: 'Teacher Issues',
    icon: 'pi pi-users',
    to: '/teacher-issues'
  },
  {
    label: 'Reports',
    icon: 'pi pi-chart-bar',
    items: [
      { label: 'Stock Summary', to: '/reports/stock-summary' },
      { label: 'Distributions', to: '/reports/distributions' },
      { label: 'Returns', to: '/reports/returns' },
      { label: 'Teacher Outstanding', to: '/reports/teacher-outstanding' },
      { label: 'Stock Movements', to: '/reports/movements' }
    ]
  },
  {
    label: 'Master Data',
    icon: 'pi pi-database',
    items: [
      { label: 'Academic Years', to: '/master-data/academic-years' },
      { label: 'Keystages', to: '/master-data/keystages' },
      { label: 'Subjects', to: '/master-data/subjects' },
      { label: 'Class Sections', to: '/master-data/class-sections' },
      { label: 'Students', to: '/master-data/students' },
      { label: 'Parents', to: '/master-data/parents' },
      { label: 'Teachers', to: '/master-data/teachers' }
    ]
  },
  {
    label: 'Settings',
    icon: 'pi pi-cog',
    items: [
      { label: 'Users', to: '/settings/users', roles: ['SuperAdmin', 'Admin'] },
      { label: 'Audit Log', to: '/audit-log', roles: ['SuperAdmin', 'Admin'] },
      { label: 'My Profile', to: '/settings/profile' }
    ]
  }
])
</script>
```

### BookSelector.vue

Used in distribution/return forms to select multiple books.

```vue
<template>
  <div class="book-selector">
    <!-- Search -->
    <div class="search-section">
      <IconField>
        <InputIcon class="pi pi-search" />
        <InputText 
          v-model="searchTerm" 
          placeholder="Search by title, code, or ISBN..."
          @input="debounceSearch"
        />
      </IconField>
    </div>
    
    <!-- Search Results -->
    <div v-if="searchResults.length" class="search-results">
      <div 
        v-for="book in searchResults" 
        :key="book.id"
        class="search-result-item"
        @click="addBook(book)"
      >
        <div class="book-info">
          <span class="book-title">{{ book.title }}</span>
          <span class="book-code">{{ book.code }}</span>
        </div>
        <StockBadge :available="book.available" />
      </div>
    </div>
    
    <!-- Selected Books -->
    <div class="selected-books">
      <DataTable :value="selectedBooks" class="compact">
        <Column field="title" header="Book" />
        <Column field="code" header="Code" />
        <Column header="Available">
          <template #body="{ data }">
            <StockBadge :available="data.available" />
          </template>
        </Column>
        <Column header="Quantity">
          <template #body="{ data, index }">
            <InputNumber 
              v-model="data.quantity" 
              :min="1" 
              :max="data.available"
              class="w-20"
              @update:modelValue="validateQuantity(index)"
            />
          </template>
        </Column>
        <Column header="">
          <template #body="{ index }">
            <Button 
              icon="pi pi-times" 
              severity="danger" 
              text 
              @click="removeBook(index)"
            />
          </template>
        </Column>
      </DataTable>
    </div>
  </div>
</template>

<script setup lang="ts">
interface SelectedBook {
  id: number
  title: string
  code: string
  available: number
  quantity: number
}

const props = defineProps<{
  modelValue: SelectedBook[]
}>()

const emit = defineEmits<{
  'update:modelValue': [books: SelectedBook[]]
}>()

const selectedBooks = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const searchTerm = ref('')
const searchResults = ref<Book[]>([])

const { debounce } = useDebounceFn(async () => {
  if (searchTerm.value.length < 2) {
    searchResults.value = []
    return
  }
  
  const { data } = await useApi().get<Book[]>('/books/search', {
    q: searchTerm.value,
    limit: 10
  })
  
  // Filter out already selected
  searchResults.value = data.filter(
    book => !selectedBooks.value.some(s => s.id === book.id)
  )
}, 300)

function addBook(book: Book) {
  selectedBooks.value = [
    ...selectedBooks.value,
    {
      id: book.id,
      title: book.title,
      code: book.code,
      available: book.available,
      quantity: 1
    }
  ]
  searchTerm.value = ''
  searchResults.value = []
}

function removeBook(index: number) {
  selectedBooks.value = selectedBooks.value.filter((_, i) => i !== index)
}
</script>
```

### StockBadge.vue

```vue
<template>
  <Tag :severity="severity" :value="label" />
</template>

<script setup lang="ts">
const props = defineProps<{
  available: number
  showLabel?: boolean
}>()

const severity = computed(() => {
  if (props.available === 0) return 'danger'
  if (props.available < 10) return 'warning'
  return 'success'
})

const label = computed(() => {
  if (props.showLabel) {
    return `${props.available} available`
  }
  return props.available.toString()
})
</script>
```

---

## Composables

| Composable | Purpose |
|------------|---------|
| `useApi` | Authenticated API client with GET/POST/PUT/DELETE methods |
| `useAuth` | Login state, token management, role checks |
| `usePagination` | Page/pageSize state, handlers for DataTable pagination |
| `useToast` | PrimeVue toast notifications wrapper |
| `useConfirm` | Confirmation dialog wrapper |
| `useAcademicYear` | Active year context, year switching |
| `usePrint` | Open print dialogs, format for printing |

---

## Type Definitions

### types/entities.ts

```typescript
export interface Book {
  id: number
  isbn: string | null
  code: string
  title: string
  author: string | null
  edition: string | null
  publisher: string | null
  publishedYear: number | null
  subjectId: number
  subjectName: string
  grade: string | null
  totalStock: number
  distributed: number
  withTeachers: number
  damaged: number
  lost: number
  available: number
}

export interface Student {
  id: number
  fullName: string
  indexNo: string
  nationalId: string | null
  classSectionId: number
  className: string
}

export interface Parent {
  id: number
  fullName: string
  nationalId: string
  phone: string | null
  relationship: string | null
}

export interface DistributionSlip {
  id: number
  referenceNo: string
  academicYearId: number
  term: Term
  student: Student
  parent: Parent
  issuedBy: Staff
  issuedAt: string
  notes: string | null
  items: DistributionSlipItem[]
}

export interface DistributionSlipItem {
  id: number
  bookId: number
  bookTitle: string
  bookCode: string
  quantity: number
}

// ... more types
```

### types/enums.ts

```typescript
export enum Term {
  Term1 = 1,
  Term2 = 2,
  Both = 3
}

export enum BookCondition {
  Good = 1,
  Fair = 2,
  Poor = 3,
  Damaged = 4,
  Lost = 5
}

export enum StockMovementType {
  StockEntry = 1,
  Distribution = 2,
  Return = 3,
  TeacherIssue = 4,
  TeacherReturn = 5,
  MarkDamaged = 6,
  MarkLost = 7,
  Adjustment = 8,
  WriteOff = 9
}

export enum TeacherIssueStatus {
  Active = 1,
  PartiallyReturned = 2,
  FullyReturned = 3,
  Overdue = 4
}
```

---

## Page Example: Distribution Create

**Path:** `/pages/distribution/create.vue`

**Structure:**
- Card: Student selection with `StudentLookup` component
- Card: Parent selection with `ParentLookup` component  
- Card: Term dropdown (Term 1 / Term 2 / Both)
- Card: Book selection with `BookSelector` component
- Card: Notes textarea
- Actions: Cancel / Create Distribution buttons

**Form state:**
```typescript
const form = reactive({
  student: null as Student | null,
  parent: null as Parent | null,
  term: Term.Term1,
  items: [] as SelectedBook[],
  notes: ''
})
```

**Validation:** Requires student, parent, and at least one book with valid quantity.

**Submit flow:**
1. POST to `/distributions` with form data
2. On success: show toast, navigate to detail page
3. On error: handled by `useApi` (shows toast)

---

## Styling Guidelines

### Tailwind Configuration

```typescript
// tailwind.config.ts
export default {
  content: [
    './components/**/*.{vue,js,ts}',
    './layouts/**/*.vue',
    './pages/**/*.vue',
    './plugins/**/*.{js,ts}',
    './app.vue'
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#f0fdf4',
          100: '#dcfce7',
          500: '#22c55e',
          600: '#16a34a',
          700: '#15803d'
        }
      }
    }
  },
  plugins: []
}
```

### PrimeVue Theme Customization

```css
/* assets/css/primevue-theme.css */
:root {
  --primary-color: #16a34a;
  --primary-color-text: #ffffff;
  --surface-ground: #f8fafc;
  --surface-card: #ffffff;
  --surface-border: #e2e8f0;
}

/* Custom component overrides */
.p-datatable .p-datatable-header {
  background: var(--surface-card);
  border: none;
}

.p-card {
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}
```
