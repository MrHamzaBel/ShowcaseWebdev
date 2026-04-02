<template>
  <div class="chat-wrapper">
    <!-- Berichtenlijst -->
    <div class="chat-messages list-group mb-3" ref="messagesContainer">
      <div v-if="messages.length === 0" class="text-muted p-3">
        Nog geen berichten. Wees de eerste!
      </div>
      <div
        v-for="(msg, i) in messages"
        :key="i"
        class="list-group-item"
        :class="{ 'list-group-item-primary': msg.sender === username }"
      >
        <div class="d-flex justify-content-between">
          <strong>{{ msg.sender }}</strong>
          <small class="text-muted">{{ msg.time }}</small>
        </div>
        <p class="mb-0">{{ msg.content }}</p>
      </div>
    </div>

    <!-- Foutmelding -->
    <div v-if="error" class="alert alert-danger py-2">{{ error }}</div>

    <!-- Verbindingsstatus -->
    <div class="mb-2">
      <span :class="connected ? 'badge bg-success' : 'badge bg-danger'">
        {{ connected ? 'Verbonden' : 'Niet verbonden' }}
      </span>
    </div>

    <!-- Invoerveld -->
    <form @submit.prevent="sendMessage" class="input-group">
      <input
        v-model="newMessage"
        class="form-control"
        placeholder="Typ een bericht..."
        maxlength="1000"
        :disabled="!connected"
        autocomplete="off"
      />
      <button class="btn btn-primary" type="submit" :disabled="!connected || !newMessage.trim()">
        Verzenden
      </button>
    </form>
  </div>
</template>

<script>
import * as signalR from '@microsoft/signalr'

export default {
  name: 'ChatApp',

  props: {
    // URL van de SignalR hub, wordt meegegeven via data-attribuut
    hubUrl: { type: String, default: '/chatHub' },
    username: { type: String, default: 'Anoniem' }
  },

  data() {
    return {
      messages: [],
      newMessage: '',
      connected: false,
      error: null,
      connection: null
    }
  },

  async mounted() {
    await this.connectToHub()
  },

  beforeUnmount() {
    // Verbinding sluiten als het component wordt verwijderd
    if (this.connection) {
      this.connection.stop()
    }
  },

  methods: {
    async connectToHub() {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.hubUrl)
        .withAutomaticReconnect()  // Automatisch opnieuw verbinden bij onderbreking
        .configureLogging(signalR.LogLevel.Warning)
        .build()

      // Handler voor inkomende berichten
      this.connection.on('ReceiveMessage', (sender, content, time) => {
        this.messages.push({ sender, content, time })
        this.$nextTick(() => this.scrollToBottom())
      })

      // Handler voor foutmeldingen van de server
      this.connection.on('ReceiveError', (message) => {
        this.error = message
        setTimeout(() => { this.error = null }, 4000)
      })

      this.connection.onreconnecting(() => { this.connected = false })
      this.connection.onreconnected(() => { this.connected = true })
      this.connection.onclose(() => { this.connected = false })

      try {
        await this.connection.start()
        this.connected = true
      } catch (err) {
        this.error = 'Kon niet verbinden met de chatserver. Probeer de pagina te vernieuwen.'
        console.error('SignalR verbindingsfout:', err)
      }
    },

    async sendMessage() {
      const content = this.newMessage.trim()
      if (!content || !this.connected) return

      try {
        // Roep de server-methode aan; het bericht wordt opgeslagen én uitgezonden via de hub
        await this.connection.invoke('SendMessage', content)
        this.newMessage = ''
      } catch (err) {
        this.error = 'Bericht kon niet worden verzonden.'
        console.error('SendMessage fout:', err)
      }
    },

    scrollToBottom() {
      const container = this.$refs.messagesContainer
      if (container) {
        container.scrollTop = container.scrollHeight
      }
    }
  }
}
</script>

<style scoped>
.chat-wrapper {
  max-width: 700px;
}

.chat-messages {
  max-height: 450px;
  overflow-y: auto;
  border: 1px solid #dee2e6;
  border-radius: 0.375rem;
}
</style>
