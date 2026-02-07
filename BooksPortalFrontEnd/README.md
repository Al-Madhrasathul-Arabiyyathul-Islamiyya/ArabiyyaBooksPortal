# BooksPortal Frontend

Nuxt 4 single-page application for the Books Portal textbook management system.

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
  public/               # Static assets
  server/               # Nitro server routes (if needed)
  nuxt.config.ts        # Nuxt configuration
```

## Configuration

The backend API base URL is configured in `nuxt.config.ts` via the `runtimeConfig` or a custom `$fetch` wrapper.

## API Reference

See [documentation/api-reference.md](../documentation/api-reference.md) for the backend API this frontend consumes.

## Status

- Project scaffolded with all dependencies installed
- Modules registered but not yet configured
- Frontend implementation (Module 10) has not started
