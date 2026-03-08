# Build Stage
FROM node:20-alpine AS builder
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci
COPY . .
ENV NODE_OPTIONS="--max-old-space-size=4096"
RUN npm run build

# Run Stage
FROM node:20-alpine AS runner
WORKDIR /app
COPY --from=builder /app/.output ./.output
CMD ["node", ".output/server/index.mjs"]