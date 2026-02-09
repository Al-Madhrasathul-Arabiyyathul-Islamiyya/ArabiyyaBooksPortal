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

## API Reference

See [documentation/api-reference.md](../documentation/api-reference.md) for the backend API this frontend consumes.

## Status

- Module 10 Phase 1: complete
- Module 10 Phase 2: complete
- Module 10 Phase 2.5 (BFF + CSRF alignment): complete
- Module 10 Phase 3: next to start
