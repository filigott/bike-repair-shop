FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM caddy:2-alpine AS runtime
COPY --from=build /app/build/client /srv
COPY Caddyfile /etc/caddy/Caddyfile
