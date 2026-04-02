import { createApp } from 'vue'
import ChatApp from './ChatApp.vue'

// Mount het Vue component op het element met id="chat-app"
// De hub URL en username worden via data-attributen meegegeven vanuit Razor
const el = document.getElementById('chat-app')
if (el) {
  createApp(ChatApp, {
    hubUrl: el.dataset.hubUrl || '/chatHub',
    username: el.dataset.username || 'Anoniem'
  }).mount(el)
}
