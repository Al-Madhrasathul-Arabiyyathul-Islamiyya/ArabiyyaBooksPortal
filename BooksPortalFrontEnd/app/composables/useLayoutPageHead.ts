type LayoutArea = 'client' | 'admin'

function toTitleCase(value: string): string {
  return value
    .replace(/[-_]+/g, ' ')
    .replace(/\s+/g, ' ')
    .trim()
    .replace(/\b\w/g, char => char.toUpperCase())
}

function getFallbackPageTitle(path: string): string {
  if (path === '/') return 'Operations'
  if (path === '/admin') return 'Admin Dashboard'

  const segment = path.split('/').filter(Boolean).at(-1)
  if (!segment) return 'Home'
  if (/^\d+$/.test(segment)) return 'Details'

  return toTitleCase(segment)
}

export function useLayoutPageHead(area: LayoutArea) {
  const route = useRoute()
  const { public: { appTitle } } = useRuntimeConfig()

  const pageTitle = computed(() => {
    const explicit = route.meta?.title
    if (typeof explicit === 'string' && explicit.trim().length > 0) {
      return explicit.trim()
    }

    const breadcrumb = route.meta?.breadcrumb
    if (breadcrumb && typeof breadcrumb === 'object') {
      const labels = Object.values(breadcrumb)
        .filter((value): value is string => typeof value === 'string' && value.trim().length > 0)
      if (labels.length > 0) {
        return labels[labels.length - 1]!.trim()
      }
    }

    return getFallbackPageTitle(route.path)
  })

  const fullTitle = computed(() =>
    area === 'admin'
      ? `${pageTitle.value} - Admin - ${appTitle}`
      : `${pageTitle.value} - ${appTitle}`,
  )

  useHead(() => ({
    title: fullTitle.value,
  }))

  useSeoMeta({
    title: fullTitle,
    ogTitle: fullTitle,
    twitterTitle: fullTitle,
    description: () => `${pageTitle.value} page for ${appTitle}.`,
    ogDescription: () => `${pageTitle.value} page for ${appTitle}.`,
    twitterDescription: () => `${pageTitle.value} page for ${appTitle}.`,
  })
}
