import { describe, expect, it } from 'vitest'
import { defineComponent, h } from 'vue'
import { mount } from '@vue/test-utils'
import IconActionButton from '~/components/common/IconActionButton.vue'

const ButtonStub = defineComponent({
  name: 'Button',
  inheritAttrs: false,
  props: {
    disabled: { type: Boolean, default: false },
  },
  emits: ['click'],
  setup(props, { attrs, emit }) {
    return () =>
      h('button', {
        ...attrs,
        disabled: props.disabled,
        onClick: (event: MouseEvent) => emit('click', event),
      })
  },
})

describe('IconActionButton', () => {
  it('forwards tooltip/icon/severity and emits click', async () => {
    const wrapper = mount(IconActionButton, {
      props: {
        icon: 'pi pi-pencil',
        tooltip: 'Edit row',
        severity: 'warn',
      },
      global: {
        components: {
          Button: ButtonStub,
        },
      },
    })

    const button = wrapper.get('button')
    expect(button.attributes('aria-label')).toBe('Edit row')
    expect(button.attributes('title')).toBe('Edit row')
    await button.trigger('click')
    const emitted = wrapper.emitted('click')
    expect(emitted).toBeTruthy()
    expect(emitted).toHaveLength(1)
    expect(emitted?.[0]?.[0]).toBeInstanceOf(MouseEvent)
  })

  it('is disabled by default flag and does not emit click when disabled', async () => {
    const wrapper = mount(IconActionButton, {
      props: {
        icon: 'pi pi-trash',
        tooltip: 'Delete row',
        disabled: true,
      },
      global: {
        components: {
          Button: ButtonStub,
        },
      },
    })

    const button = wrapper.get('button')
    expect(button.attributes('disabled')).toBeDefined()

    await button.trigger('click')
    expect(wrapper.emitted('click')).toBeFalsy()
  })
})
