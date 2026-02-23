# BooksPortal Frontend

Nuxt 4 SSR frontend for the Books Portal textbook management system.

## Tech Stack

| Category | Technology |
|----------|------------|
| Framework | Nuxt 4.3, Vue 3.5 |
| Language | TypeScript |
| UI Components | PrimeVue 4.5 |
| State Management | Pinia 3 + Pinia Colada (server-state caching) |
| Form Validation | Regle + Zod |
| Charts | nuxt-charts |
| Icons | @nuxt/icon |
| Date Handling | dayjs-nuxt |
| Images | @nuxt/image |
| CSRF | nuxt-csurf |
| Linting | @nuxt/eslint |

## Prerequisites

- Node.js 20+
- Bun (package manager)

## Getting Started

```bash
# Install dependencies
bun install

# Start dev server
bun run dev
```

The dev server starts at `http://localhost:3000`.

## Project Structure

```
BooksPortalFrontEnd/
  app/
    assets/             # CSS, fonts
    components/         # Vue components
    composables/        # Shared composables
    layouts/            # Page layouts
    middleware/          # Route guards (auth)
    pages/              # File-based routing
    plugins/            # PrimeVue, auth plugins
    stores/             # Pinia stores
    utils/              # Helper functions
  server/               # Nuxt BFF routes and server utilities
  public/               # Static assets
  nuxt.config.ts        # Nuxt configuration
```

## Configuration

- External backend base URL: `runtimeConfig.public.apiBase` in `nuxt.config.ts`
- Frontend app calls internal BFF routes under `/api/bff`
- BFF routes proxy to backend with server-managed auth cookies and refresh flow
- Session cookie/skew settings are runtime-configurable under `runtimeConfig.auth.session`:
  - `accessCookieMaxAgeSeconds`
  - `refreshCookieMaxAgeSeconds`
  - `expiryCookieMaxAgeSeconds`
  - `expirySkewSeconds`
  - `cookieSecure`
- Runtime config can be overridden by env vars when deployed (for example: `NUXT_PUBLIC_API_BASE`, `NUXT_AUTH_SESSION_EXPIRY_SKEW_SECONDS`).

## Route Surface Model (Planned)

- Operational area (all authenticated users): `/`, `/distribution/*`, `/returns/*`, `/teacher-issues/*`
- Admin area (`Admin` and `SuperAdmin` only): `/admin/*`
- Planned IA change: move master-data, books management, reports, settings, and audit screens under `/admin`

## UX Conventions (Planned)

- Prefer modal forms for create/edit actions.
- Keep full-page forms only for high-complexity workflows.
- Lookup flows should support inline create actions for missing:
  - students
  - parents
  - teachers

## API Reference

See [documentation/api-reference.md](../documentation/api-reference.md) for the backend API this frontend consumes.

## Status

- Module 10 Phase 1: complete
- Module 10 Phase 2: complete
- Module 10 Phase 2.5 (BFF + CSRF alignment): complete
- Module 10 Phase 2.6 (routing/layout split): complete
- Module 10 Phase 2.7 (backend contract alignment): complete
- Module 10 Phase 3 (master data): complete
- Module 10 Phase 4 (books): next to start
