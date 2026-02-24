FROM oven/bun:1 AS build
WORKDIR /app

COPY BooksPortalFrontEnd/package.json BooksPortalFrontEnd/bun.lock ./
RUN bun install --frozen-lockfile

COPY BooksPortalFrontEnd/ ./
ENV NODE_OPTIONS=--max-old-space-size=2048
RUN bun run build

FROM node:20-alpine AS runtime
WORKDIR /app
COPY --from=build /app/.output ./.output
COPY --from=build /app/node_modules ./node_modules

EXPOSE 3000
ENTRYPOINT ["node", ".output/server/index.mjs"]
