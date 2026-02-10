# Books Portal - Frontend Structure

## Overview

Nuxt 4 SSR frontend for Books Portal.

Current status:
- Module 10 Phase 1: complete
- Module 10 Phase 2: complete
- Module 10 Phase 2.5: complete
- Module 10 Phase 2.6: complete (routing/layout split)
- Module 10 Phase 2.7: complete (backend contract alignment)
- Module 10 Phase 3: complete (master data screens)
- Module 10 Phase 4: next to start (books)

Primary implementation plan: `plans/frontend-plan.md`

## Technology Stack (Current)

| Technology | Version | Purpose |
|------------|---------|---------|
| Nuxt | 4.3.x | SSR framework |
| Vue | 3.5.x | UI framework |
| TypeScript | 5.x | Type safety |
| PrimeVue | 4.5.x | UI components (Aura preset) |
| Tailwind CSS | 4.1.x | Utility CSS via `@tailwindcss/vite` |
| tailwindcss-primeui | 0.6.x | PrimeVue/Tailwind layering |
| Pinia | 3.x | Client state |
| Pinia Colada | 0.3.x | Server-state querying |
| Regle + Zod | 1.x + 4.x | Validation |
| dayjs-nuxt | 2.1.x | Date/time formatting |

## Key Decisions (Implemented)

- SSR enabled (not SPA-only).
- Auth now uses a Nuxt BFF model (`/api/bff`) instead of direct client-to-backend API calls.
- JWT/refresh tokens are managed in server-side HttpOnly cookies (`bp_access_token`, `bp_refresh_token`, `bp_token_expiry`).
- Tailwind v4 uses `@tailwindcss/vite` (not `@nuxtjs/tailwindcss`).
- `tailwindcss-primeui` is enabled to avoid PrimeVue style regressions.
- App composables are named `useAppToast` / `useAppConfirm` to avoid PrimeVue auto-import collisions.
- `nuxt-csurf` is configured and used for state-changing requests.

## UI Surface Split (Planned for Phase 3+)

- `User` role should see only operational flows:
  - `/`
  - `/distribution/*`
  - `/returns/*`
  - `/teacher-issues/*`
- `Admin` and `SuperAdmin` should see operational flows plus admin area under `/admin/*`.
- Master data, books catalog/stock management, reports, settings, and audit screens should live under `/admin/*`.
- Sidebar should prioritize operations and show a single `Admin` entry at the bottom for Admin/SuperAdmin.

Current implementation progress:
- Operations-focused `default` layout is in place.
- Dedicated `admin` layout is added.
- `/admin` and `/admin/audit-log` route shells are added with Admin/SuperAdmin guards.
- Audit log navigation is modeled as top-level admin entry, not under settings.

## Form UX Convention (Planned)

- Prefer modal create/edit flows across admin and operational screens for faster data entry.
- Reserve full-page forms for genuinely complex workflows.
- Lookup components should support inline creation when no result is found:
  - `Student`
  - `Parent`
  - `Teacher`

## Project Structure (As Implemented)

```text
BooksPortalFrontEnd/
  app/
    app.vue
    assets/
      css/
        main.css
    components/
      common/
        AppBreadcrumb.vue
        EmptyState.vue
      forms/
        FormField.vue
        SearchInput.vue
    composables/
      useAcademicYear.ts
      useApi.ts
      useAppConfirm.ts
      useAppToast.ts
      useAuth.ts
      usePagination.ts
      usePrint.ts
    layouts/
      auth.vue
      default.vue
    middleware/
      auth.global.ts
      role.ts
    pages/
      index.vue
      login.vue
    stores/
      app.ts
      auth.ts
      lookups.ts
    types/
      api.ts
      entities.ts
      enums.ts
      forms.ts
    utils/
      constants.ts
      formatters.ts
  server/
    api/
      bff/
        [...path].ts
        auth/
          login.post.ts
          logout.post.ts
          me.get.ts
          refresh.post.ts
    utils/
      auth-session.ts
      backend-api.ts
  public/
    favicon.ico
    logo.png
    logo.svg
    robots.txt
    fonts/
      Faruma.ttf
  bun.lock
  eslint.config.mjs
  nuxt.config.ts
  package.json
  README.md
  tsconfig.json
```

## Configuration Highlights

### Nuxt (`nuxt.config.ts`)
- `@primevue/nuxt-module` with Aura preset
- `@pinia/nuxt` + `@pinia/colada-nuxt`
- `@regle/nuxt`
- `@nuxtjs/color-mode`
- `nuxt-csurf`
- `dayjs-nuxt`
- `vite.plugins: [tailwindcss()]`
- `runtimeConfig.public.apiBase = http://localhost:5071/api`

### Styling (`app/assets/css/main.css`)
- `@import "tailwindcss"`
- `@import "tailwindcss-primeui"`
- Dark variant: `@custom-variant dark (&:where(.dark, .dark *))`
- Faruma font-face registered from `/public/fonts/Faruma.ttf`

### Auth and Routing
- BFF auth/session handling in `server/api/bff/*` + `server/utils/*`
- Global auth guard in `app/middleware/auth.global.ts` (profile-based initialization)
- Role guard in `app/middleware/role.ts`
- API client in `app/composables/useApi.ts` targeting `/api/bff`

## Notes About Planned vs Current Structure

- Older proposed component/page trees are intentionally not repeated here.
- For future pages/components by phase, use `plans/frontend-plan.md` as the active roadmap.
- For latest implementation status and decisions, use basic-memory:
  - `progress/module-10-progress`
  - `architecture/frontend-setup`
  - `decisions/design-decisions`
