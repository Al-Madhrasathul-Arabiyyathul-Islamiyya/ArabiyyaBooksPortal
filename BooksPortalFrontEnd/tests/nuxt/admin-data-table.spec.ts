import { describe, expect, it } from 'vitest'
import { defineComponent, h } from 'vue'
import { shallowMount } from '@vue/test-utils'
import AdminDataTable from '~/components/common/AdminDataTable.vue'

const DataTableStub = defineComponent({
  name: 'DataTable',
  inheritAttrs: false,
  props: {
    scrollable: { type: [Boolean, String], default: undefined },
    scrollHeight: { type: String, default: undefined },
  },
  setup(props, { attrs, slots }) {
    return () =>
      h('div', {
        class: attrs.class,
        'data-scrollable': String(props.scrollable),
        'data-scroll-height': props.scrollHeight,
      }, slots.default?.())
  },
})

describe('CommonAdminDataTable', () => {
  it('applies default scroll shell attributes and class', () => {
    const wrapper = shallowMount(AdminDataTable, {
      attrs: {
        class: 'my-table',
      },
      global: {
        stubs: {
          DataTable: DataTableStub,
        },
      },
    })

    const table = wrapper.get('.admin-data-table')
    expect(table.classes()).toContain('my-table')
    expect(table.attributes('data-scrollable')).toBe('true')
    expect(table.attributes('data-scroll-height')).toBe('flex')
  })

  it('preserves explicit scroll overrides from caller', () => {
    const wrapper = shallowMount(AdminDataTable, {
      attrs: {
        scrollable: false,
        scrollHeight: '20rem',
      },
      global: {
        stubs: {
          DataTable: DataTableStub,
        },
      },
    })

    const table = wrapper.get('.admin-data-table')
    expect(table.attributes('data-scrollable')).toBe('false')
    expect(table.attributes('data-scroll-height')).toBe('20rem')
  })
})
