  // wwwroot-src/vite.config.js
  import { defineConfig } from 'vite'
  import vue from '@vitejs/plugin-vue'

  export default defineConfig({
    plugins: [vue()],
    build: {
      outDir: '../wwwroot/js',
      rollupOptions: {
        input: 'src/main.js',
        output: {
          entryFileNames: 'chat-app.js',
          chunkFileNames: 'chat-app.js',
          assetFileNames: 'chat-app.[ext]'
        }
      }
    }
  })