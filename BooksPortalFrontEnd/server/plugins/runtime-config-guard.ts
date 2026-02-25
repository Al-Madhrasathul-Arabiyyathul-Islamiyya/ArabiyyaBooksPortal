export default defineNitroPlugin(() => {
  if (import.meta.dev) {
    return
  }

  const config = useRuntimeConfig()
  const apiBase = String(config.public?.apiBase ?? '').trim()
  const appTitle = String(config.public?.appTitle ?? '').trim()

  if (!apiBase) {
    throw new Error('Missing required runtime config: NUXT_PUBLIC_API_BASE')
  }

  if (!appTitle) {
    throw new Error('Missing required runtime config: NUXT_PUBLIC_APP_TITLE')
  }

  const session = (config.auth?.session ?? {}) as Record<string, unknown>
  const signingMode = String(session.signingMode ?? 'None').trim().toLowerCase()
  if (signingMode === 'certificate') {
    const privateKeyPath = String(session.signingPrivateKeyPath ?? '').trim()
    const publicCertPath = String(session.signingPublicCertPath ?? '').trim()
    if (!privateKeyPath || !publicCertPath) {
      throw new Error(
        'Certificate signing mode requires NUXT_AUTH_SESSION_SIGNING_PRIVATE_KEY_PATH and NUXT_AUTH_SESSION_SIGNING_PUBLIC_CERT_PATH.',
      )
    }
  }
})
