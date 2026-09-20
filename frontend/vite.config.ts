import { reactRouter } from "@react-router/dev/vite";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig } from "vite";

export default defineConfig({
  plugins: [tailwindcss(), reactRouter()],
  resolve: {
    tsconfigPaths: true,
  },
  server: {
    // Same-origin /api in development, mirroring the Caddy proxy in the composed stack.
    proxy: {
      "/api": "http://localhost:5260",
    },
  },
});
