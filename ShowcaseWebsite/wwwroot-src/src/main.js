import { createApp } from 'vue'
import ChatApp from './ChatApp.vue'

// Zoek het mount-element en lees de data-attributen uit
const el = document.getElementById('chat-app')
if (el) {
  const app = createApp(ChatApp, {
    username: el.dataset.username,
    hubUrl: el.dataset.hubUrl
  })
  app.mount(el)
}
