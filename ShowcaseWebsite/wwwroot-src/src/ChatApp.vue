<template>
  <div>
    <!-- Berichtenlijst -->
    <div id="chat-messages" ref="msgContainer">
      <div v-if="messages.length === 0" class="text-secondary small text-center mt-4">
        Nog geen berichten. Stuur het eerste bericht!
      </div>
      <div
        v-for="(msg, idx) in messages"
        :key="idx"
        class="chat-msg"
        :class="{ 'own-msg': msg.userName === username }"
      >
        <span class="chat-user">{{ msg.userName }}</span>
        <span class="chat-time">{{ msg.sentAt }}</span>
        <!-- textContent binding (geen v-html) – voorkomt XSS -->
        <p class="chat-content">{{ msg.content }}</p>
      </div>
    </div>

    <!-- Invoerveld -->
    <div class="d-flex gap-2">
      <input
        v-model="newMessage"
        @keyup.enter="sendMessage"
        type="text"
        class="form-control bg-dark text-light border-secondary"
        placeholder="Typ een bericht…"
        :disabled="!connected"
        maxlength="1000"
      />
      <button
        @click="sendMessage"
        class="btn btn-primary"
        :disabled="!connected || !newMessage.trim()"
      >
        Stuur
      </button>
    </div>

    <!-- Verbindingsstatus -->
    <p class="small mt-2" :class="connected ? 'text-success' : 'text-warning'">
      {{ connected ? '● Verbonden' : '○ Verbinding maken…' }}
    </p>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick } from 'vue'
import * as signalR from '@microsoft/signalr'

// Props worden doorgegeven vanuit main.js via data-attributen
const props = defineProps({
  username: { type: String, default: '' },
  hubUrl: { type: String, default: '/chathub' }
})

const messages = ref([])
const newMessage = ref('')
const connected = ref(false)
const msgContainer = ref(null)

// Scroll naar onderkant na elk nieuw bericht
async function scrollToBottom () {
  await nextTick()
  if (msgContainer.value) {
    msgContainer.value.scrollTop = msgContainer.value.scrollHeight
  }
}

// Haal bestaande berichten op via REST
async function loadHistory () {
  try {
    const res = await fetch('/api/messages')
    if (res.ok) {
      messages.value = await res.json()
      scrollToBottom()
    }
  } catch {
    // Stil falen – geschiedenis niet kritiek
  }
}

// Verstuur bericht via SignalR Hub
async function sendMessage () {
  const text = newMessage.value.trim()
  if (!text || !connection) return
  try {
    await connection.invoke('SendMessage', text)
    newMessage.value = ''
  } catch (err) {
    console.error('Versturen mislukt:', err)
  }
}

let connection = null

onMounted(async () => {
  await loadHistory()

  // Bouw SignalR verbinding op
  connection = new signalR.HubConnectionBuilder()
    .withUrl(props.hubUrl)
    .withAutomaticReconnect()
    .build()

  // Ontvang berichten van server
  connection.on('ReceiveMessage', (msg) => {
    messages.value.push(msg)
    scrollToBottom()
  })

  connection.onclose(() => { connected.value = false })
  connection.onreconnected(() => { connected.value = true })

  try {
    await connection.start()
    connected.value = true
  } catch (err) {
    console.error('SignalR verbinding mislukt:', err)
  }
})
</script>

<style scoped>
.own-msg .chat-user {
  color: #86efac; /* lichtgroener voor eigen berichten */
}
</style>
