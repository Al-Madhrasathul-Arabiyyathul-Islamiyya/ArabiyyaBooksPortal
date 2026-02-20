import { describe, expect, it } from 'vitest'
import { defineComponent, h } from 'vue'
import { shallowMount } from '@vue/test-utils'
import BulkImportDialog from '~/components/forms/BulkImportDialog.vue'

const DialogStub = defineComponent({
  name: 'Dialog',
  inheritAttrs: false,
  props: {
    visible: { type: Boolean, default: false },
  },
  emits: ['update:visible'],
  setup(props, { attrs, slots, emit }) {
    return () =>
      h('div', { class: 'dialog-stub', ...attrs }, [
        props.visible ? slots.default?.() : null,
        h('button', {
          type: 'button',
          class: 'dialog-close-stub',
          onClick: () => emit('update:visible', false),
        }),
      ])
  },
})

const ButtonStub = defineComponent({
  name: 'Button',
  inheritAttrs: false,
  props: {
    label: { type: String, default: '' },
    disabled: { type: Boolean, default: false },
  },
  emits: ['click'],
  setup(props, { attrs, emit }) {
    return () =>
      h('button', {
        ...attrs,
        disabled: props.disabled,
        type: 'button',
        onClick: (event: MouseEvent) => emit('click', event),
      }, props.label || 'button')
  },
})

const DataTableStub = defineComponent({
  name: 'DataTable',
  setup(_, { slots }) {
    return () => h('div', { class: 'datatable-stub' }, slots.default?.())
  },
})

const ColumnStub = defineComponent({
  name: 'Column',
  setup() {
    return () => h('div')
  },
})

describe('FormsBulkImportDialog', () => {
  it('emits template / validate / commit / update:visible from dialog flow', async () => {
    const wrapper = shallowMount(BulkImportDialog, {
      props: {
        visible: true,
        entityLabel: 'Books',
        report: {
          entity: 'Book',
          totalRows: 2,
          validRows: 2,
          invalidRows: 0,
          insertedRows: 0,
          failedRows: 0,
          canCommit: true,
          rows: [],
          issues: [],
        },
        errorMessage: '',
        isValidating: false,
        isCommitting: false,
      },
      global: {
        stubs: {
          Dialog: DialogStub,
          Button: ButtonStub,
          Card: { template: '<div><slot /></div>' },
          Message: { template: '<div><slot /></div>' },
          DataTable: DataTableStub,
          Column: ColumnStub,
        },
      },
    })

    const buttons = wrapper.findAllComponents(ButtonStub)
    const findButton = (label: string) => {
      const hit = buttons.find(btn => btn.props('label') === label)
      if (!hit) {
        throw new Error(`Button not found: ${label}`)
      }
      return hit
    }

    await findButton('Download Template').trigger('click')
    expect(wrapper.emitted('template')).toBeTruthy()

    const file = new File(['book,qty\nMath,10'], 'books.xlsx', { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
    const input = wrapper.get('input[type="file"]')
    Object.defineProperty(input.element, 'files', {
      value: [file],
      configurable: true,
    })
    await input.trigger('change')

    await findButton('Validate').trigger('click')
    const validateEvents = wrapper.emitted('validate')
    expect(validateEvents).toBeTruthy()
    expect(validateEvents?.[0]?.[0]).toBeInstanceOf(File)

    await findButton('Commit').trigger('click')
    const commitEvents = wrapper.emitted('commit')
    expect(commitEvents).toBeTruthy()
    expect(commitEvents?.[0]?.[0]).toBeInstanceOf(File)

    await wrapper.get('.dialog-close-stub').trigger('click')
    expect(wrapper.emitted('update:visible')?.[0]).toEqual([false])
  })
})
