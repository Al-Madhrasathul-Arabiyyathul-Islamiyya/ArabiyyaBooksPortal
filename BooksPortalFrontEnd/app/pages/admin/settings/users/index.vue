<template>
  <div class="flex flex-col gap-4">
    <div class="flex flex-wrap items-end justify-between gap-3">
      <div>
        <h1 class="text-2xl font-semibold text-surface-900 dark:text-surface-0">
          Users
        </h1>
        <p class="text-sm text-surface-600 dark:text-surface-400">
          Manage user accounts, roles, and activation status.
        </p>
      </div>
      <Button
        label="New User"
        icon="pi pi-plus"
        @click="openCreateDialog"
      />
    </div>

    <Card>
      <template #content>
        <DataTable
          :value="users"
          :loading="isLoading"
          data-key="id"
          responsive-layout="scroll"
        >
          <Column
            field="userName"
            header="Username"
            style="min-width: 10rem;"
          />
          <Column
            field="email"
            header="Email"
            style="min-width: 14rem;"
          />
          <Column
            field="fullName"
            header="Full Name"
            style="min-width: 12rem;"
          />
          <Column
            header="Roles"
            style="min-width: 16rem;"
          >
            <template #body="{ data }">
              <div class="flex flex-wrap gap-1">
                <Tag
                  v-for="role in data.roles"
                  :key="`${data.id}-${role}`"
                  :value="role"
                  :severity="roleSeverity(role)"
                />
              </div>
            </template>
          </Column>
          <Column
            header="Status"
            style="min-width: 8rem;"
          >
            <template #body="{ data }">
              <Tag
                :value="data.isActive ? 'Active' : 'Inactive'"
                :severity="data.isActive ? 'success' : 'warn'"
              />
            </template>
          </Column>
          <Column
            header="Created"
            style="min-width: 11rem;"
          >
            <template #body="{ data }">
              {{ formatDateTime(data.createdAt) }}
            </template>
          </Column>
          <Column
            header="Actions"
            :exportable="false"
            style="width: 12rem;"
          >
            <template #body="{ data }">
              <div class="flex items-center gap-2">
                <CommonIconActionButton
                  icon="pi pi-pencil"
                  tooltip="Edit user"
                  @click="openEditDialog(data)"
                />
                <CommonIconActionButton
                  :icon="data.isActive ? 'pi pi-user-minus' : 'pi pi-user-plus'"
                  :severity="data.isActive ? 'warn' : 'success'"
                  :tooltip="data.isActive ? 'Deactivate user' : 'Activate user'"
                  @click="toggleActive(data)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <Dialog
      v-model:visible="isDialogVisible"
      modal
      :header="isEditing ? 'Edit User' : 'Create User'"
      :style="{ width: '44rem' }"
    >
      <form
        class="grid gap-4 md:grid-cols-2"
        @submit.prevent="submitUser"
      >
        <FormsFormField
          v-if="!isEditing"
          label="Username"
          required
          field-id="userName"
          :error="errors.userName"
        >
          <InputText
            id="userName"
            v-model.trim="form.userName"
            fluid
            :invalid="!!errors.userName"
            @blur="touchField('userName')"
          />
        </FormsFormField>

        <FormsFormField
          label="Email"
          required
          field-id="email"
          :error="errors.email"
        >
          <InputText
            id="email"
            v-model.trim="form.email"
            fluid
            :invalid="!!errors.email"
            @blur="touchField('email')"
          />
        </FormsFormField>

        <FormsFormField
          label="Full Name"
          required
          field-id="fullName"
          :error="errors.fullName"
        >
          <InputText
            id="fullName"
            v-model.trim="form.fullName"
            fluid
            :invalid="!!errors.fullName"
            @blur="touchField('fullName')"
          />
        </FormsFormField>

        <FormsFormField
          v-if="!isEditing"
          label="Password"
          required
          field-id="password"
          :error="errors.password"
        >
          <Password
            id="password"
            v-model="form.password"
            toggle-mask
            :feedback="false"
            fluid
            :invalid="!!errors.password"
            @blur="touchField('password')"
          />
        </FormsFormField>

        <FormsFormField
          label="National ID"
          field-id="nationalId"
          :error="errors.nationalId"
        >
          <InputText
            id="nationalId"
            v-model.trim="form.nationalId"
            fluid
            :invalid="!!errors.nationalId"
            @blur="touchField('nationalId')"
          />
        </FormsFormField>

        <FormsFormField
          label="Designation"
          field-id="designation"
          :error="errors.designation"
        >
          <InputText
            id="designation"
            v-model.trim="form.designation"
            fluid
            :invalid="!!errors.designation"
            @blur="touchField('designation')"
          />
        </FormsFormField>

        <FormsFormField
          label="Roles"
          required
          field-id="roles"
          :error="errors.roles"
          class="md:col-span-2"
        >
          <MultiSelect
            id="roles"
            v-model="form.roles"
            :options="roleOptions"
            option-label="label"
            option-value="value"
            display="chip"
            placeholder="Select one or more roles"
            fluid
            :invalid="!!errors.roles"
            @blur="touchField('roles')"
          />
        </FormsFormField>

        <FormsFormField
          v-if="isEditing"
          label="Status"
          field-id="isActive"
          class="md:col-span-2"
        >
          <div class="flex items-center gap-2">
            <ToggleSwitch
              id="isActive"
              v-model="form.isActive"
            />
            <span class="text-sm text-surface-600 dark:text-surface-400">
              {{ form.isActive ? 'Active' : 'Inactive' }}
            </span>
          </div>
        </FormsFormField>

        <Message
          v-if="formError"
          severity="error"
          :closable="false"
          class="md:col-span-2"
        >
          {{ formError }}
        </Message>

        <div class="flex justify-end gap-2 pt-2 md:col-span-2">
          <Button
            type="button"
            label="Cancel"
            severity="secondary"
            text
            @click="closeDialog"
          />
          <Button
            type="submit"
            :label="isEditing ? 'Save Changes' : 'Create User'"
            :loading="isSubmitting"
          />
        </div>
      </form>
    </Dialog>
  </div>
</template>

<script setup lang="ts">
import { z } from 'zod/v4'
import type { User } from '~/types/entities'
import { CreateUserRequestSchema, UpdateUserRequestSchema } from '~/types/forms'
import { API, ROLES } from '~/utils/constants'
import { getFriendlyErrorMessage } from '~/utils/validation/backend-errors'

definePageMeta({
  layout: 'admin',
  middleware: ['admin'],
  breadcrumb: {
    admin: 'Admin',
    settings: 'Settings',
    users: 'Users',
  },
})

type UserFormState = {
  userName: string
  email: string
  password: string
  fullName: string
  nationalId: string
  designation: string
  roles: string[]
  isActive: boolean
}

const api = useApi()
const { showError, showSuccess } = useAppToast()
const { confirmAction } = useAppConfirm()

const users = ref<User[]>([])
const isLoading = ref(false)
const isSubmitting = ref(false)
const isDialogVisible = ref(false)
const isEditing = ref(false)
const selectedUserId = ref<number | null>(null)

const roleOptions = [
  { label: ROLES.superAdmin, value: ROLES.superAdmin },
  { label: ROLES.admin, value: ROLES.admin },
  { label: ROLES.user, value: ROLES.user },
]

const UserFormSchema = z.object({
  userName: z.string().trim().min(1, 'Username is required'),
  email: z.email('Enter a valid email address'),
  password: z.string(),
  fullName: z.string().trim().min(1, 'Full name is required'),
  nationalId: z.string().trim(),
  designation: z.string().trim(),
  roles: z.array(z.string()).min(1, 'At least one role is required'),
  isActive: z.boolean(),
}).superRefine((values, ctx) => {
  if (!isEditing.value) {
    if (!values.userName.trim()) {
      ctx.addIssue({
        code: 'custom',
        message: 'Username is required',
        path: ['userName'],
      })
    }
    if (values.password.length < 6) {
      ctx.addIssue({
        code: 'custom',
        message: 'Password must be at least 6 characters',
        path: ['password'],
      })
    }
  }
})

const {
  state: form,
  errors,
  globalError: formError,
  touchField,
  validateWithSchema,
  setGlobalError,
  applyBackendErrors,
  resetForm: resetValidationForm,
} = useAppValidation<UserFormState>(
  {
    userName: '',
    email: '',
    password: '',
    fullName: '',
    nationalId: '',
    designation: '',
    roles: [ROLES.user],
    isActive: true,
  },
  UserFormSchema as z.ZodType<UserFormState>,
)

function resetForm() {
  resetValidationForm()
  isEditing.value = false
  selectedUserId.value = null
}

function toNullable(value: string) {
  const trimmed = value.trim()
  return trimmed.length > 0 ? trimmed : null
}

function roleSeverity(role: string) {
  if (role === ROLES.superAdmin) return 'danger'
  if (role === ROLES.admin) return 'warn'
  return 'info'
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString()
}

async function loadUsers() {
  isLoading.value = true
  try {
    const response = await api.get<User[]>(API.users.base)
    if (response.success) {
      users.value = response.data
      return
    }
    showError(response.message ?? 'Failed to load users')
  }
  catch (error: unknown) {
    showError(getFriendlyErrorMessage(error, 'Failed to load users'))
  }
  finally {
    isLoading.value = false
  }
}

function openCreateDialog() {
  resetForm()
  form.userName = ''
  form.email = ''
  form.password = ''
  form.fullName = ''
  form.nationalId = ''
  form.designation = ''
  form.roles = [ROLES.user]
  form.isActive = true
  isDialogVisible.value = true
}

function openEditDialog(user: User) {
  resetForm()
  isEditing.value = true
  selectedUserId.value = user.id
  form.userName = user.userName
  form.email = user.email
  form.password = ''
  form.fullName = user.fullName
  form.nationalId = user.nationalId ?? ''
  form.designation = user.designation ?? ''
  form.roles = [...user.roles]
  form.isActive = user.isActive
  isDialogVisible.value = true
}

function closeDialog() {
  isDialogVisible.value = false
  resetForm()
}

async function submitUser() {
  const parsed = await validateWithSchema(form)
  if (!parsed.success) return

  isSubmitting.value = true
  try {
    if (isEditing.value && selectedUserId.value) {
      const updatePayload = {
        email: parsed.data.email,
        fullName: parsed.data.fullName,
        nationalId: toNullable(parsed.data.nationalId),
        designation: toNullable(parsed.data.designation),
        isActive: parsed.data.isActive,
      }
      const updateCheck = UpdateUserRequestSchema.safeParse(updatePayload)
      if (!updateCheck.success) {
        setGlobalError('Invalid user update payload.')
        return
      }

      const updateResponse = await api.put<User>(API.users.byId(selectedUserId.value), updateCheck.data)
      if (!updateResponse.success) {
        setGlobalError(updateResponse.message ?? 'Failed to update user')
        return
      }

      const rolesResponse = await api.put<boolean>(API.users.roles(selectedUserId.value), parsed.data.roles)
      if (!rolesResponse.success) {
        setGlobalError(rolesResponse.message ?? 'Failed to update roles')
        return
      }

      showSuccess('User updated successfully')
      closeDialog()
      await loadUsers()
      return
    }

    const createPayload = {
      userName: parsed.data.userName,
      email: parsed.data.email,
      password: parsed.data.password,
      fullName: parsed.data.fullName,
      nationalId: toNullable(parsed.data.nationalId),
      designation: toNullable(parsed.data.designation),
      roles: parsed.data.roles,
    }
    const createCheck = CreateUserRequestSchema.safeParse(createPayload)
    if (!createCheck.success) {
      setGlobalError('Invalid user create payload.')
      return
    }

    const createResponse = await api.post<User>(API.users.base, createCheck.data)
    if (createResponse.success) {
      showSuccess('User created successfully')
      closeDialog()
      await loadUsers()
      return
    }

    setGlobalError(createResponse.message ?? 'Failed to create user')
  }
  catch (error: unknown) {
    applyBackendErrors(error)
  }
  finally {
    isSubmitting.value = false
  }
}

function toggleActive(user: User) {
  confirmAction(
    `${user.isActive ? 'Deactivate' : 'Activate'} "${user.userName}"?`,
    async () => {
      try {
        const response = await api.post<boolean>(API.users.toggleActive(user.id))
        if (response.success) {
          showSuccess(response.message ?? 'User status updated')
          await loadUsers()
          return
        }
        showError(response.message ?? 'Failed to update status')
      }
      catch (error: unknown) {
        showError(getFriendlyErrorMessage(error, 'Failed to update status'))
      }
    },
    'Confirm Status Change',
    user.isActive ? 'Deactivate' : 'Activate',
    'Cancel',
  )
}

onMounted(async () => {
  await loadUsers()
})
</script>
