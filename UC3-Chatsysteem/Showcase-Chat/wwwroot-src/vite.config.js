import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  build: {
    // Output naar wwwroot/js zodat ASP.NET Core het als static file kan serveren
    outDir: '../wwwroot/js',
    emptyOutDir: false,
    rollupOptions: {
      input: 'src/main.js',
      output: {
        entryFileNames: 'chat-app.js',
        // Geen chunksplitting nodig voor deze kleine app
        inlineDynamicImports: true
      }
    }
  }
})
