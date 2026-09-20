import type { Config } from "@react-router/dev/config";

export default {
  // Server-side render by default; false builds a static bundle for Caddy to serve.
  ssr: false,
} satisfies Config;
