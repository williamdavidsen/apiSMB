import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      react: path.resolve(__dirname, './node_modules/react'),
      'react-dom': path.resolve(__dirname, './node_modules/react-dom'),
      'react-router-dom': path.resolve(__dirname, './node_modules/react-router-dom'),
      '@mui/material': path.resolve(__dirname, './node_modules/@mui/material'),
      '@mui/icons-material': path.resolve(__dirname, './node_modules/@mui/icons-material'),
      '@emotion/react': path.resolve(__dirname, './node_modules/@emotion/react'),
      '@emotion/styled': path.resolve(__dirname, './node_modules/@emotion/styled'),
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: ['./vitest.setup.ts'],
    pool: 'threads',
    maxWorkers: 1,
    fileParallelism: false,
    coverage: {
      provider: 'v8',
      all: true,
      reportsDirectory: './coverage',
      reporter: ['text', 'json-summary', 'html'],
      include: ['../../Frontend/dashboard/src/**/*.{ts,tsx}'],
      exclude: [
        '../../Frontend/dashboard/src/**/*.d.ts',
      ],
    },
  },
})
