import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
// Default matches API/Properties/launchSettings.json (http://localhost:1071).
// Override: create Frontend/dashboard/.env.development with e.g. VITE_DEV_API_PROXY=http://localhost:5555
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiProxyTarget = env.VITE_DEV_API_PROXY?.trim() || 'http://localhost:1071'

  return {
    plugins: [react()],
    server: {
      proxy: {
        '/api': {
          target: apiProxyTarget,
          changeOrigin: true,
        },
      },
    },
  }
})
