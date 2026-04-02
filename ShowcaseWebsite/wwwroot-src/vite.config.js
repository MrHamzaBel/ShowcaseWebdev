import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  build: {
    // Output direct naar wwwroot/js van het MVC project
    outDir: '../ShowcaseWebsite/wwwroot/js',
    emptyOutDir: false,
    rollupOptions: {
      input: './src/main.js',
      output: {
        entryFileNames: 'chat-app.js',
        // Geen content-hash in bestandsnaam zodat de View altijd dezelfde URL gebruikt
        chunkFileNames: '[name].js',
        assetFileNames: '[name].[ext]'
      }
    }
  }
})
