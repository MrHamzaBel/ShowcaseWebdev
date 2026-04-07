// chat-app.js – Vanilla JS SignalR chat
// Monteert zichzelf in <div id="chat-app">

(function () {
    const root = document.getElementById("chat-app");
    if (!root) return;

    const currentUser = root.dataset.username || "";
    const hubUrl = root.dataset.hubUrl || "/chathub";

    // ── UI opbouwen ──────────────────────────────────────────────────────────
    root.innerHTML = `
        <div id="chat-messages" style="
            height:380px; overflow-y:auto;
            background:#0f0f23; border:1px solid #2a2a4a;
            border-radius:6px; padding:12px; margin-bottom:12px;">
        </div>
        <div class="d-flex gap-2">
            <input id="chat-input" type="text" maxlength="1000"
                placeholder="Typ een bericht..."
                class="form-control bg-dark text-light border-secondary" />
            <button id="chat-send" class="btn btn-primary" style="min-width:90px;">Sturen</button>
        </div>
        <p id="chat-status" class="text-secondary small mt-2 mb-0">Verbinden...</p>
    `;

    const messagesEl = document.getElementById("chat-messages");
    const inputEl = document.getElementById("chat-input");
    const sendBtn = document.getElementById("chat-send");
    const statusEl = document.getElementById("chat-status");

    // ── Bericht renderen ─────────────────────────────────────────────────────
    function appendMessage(userName, content, time) {
        const isOwn = userName === currentUser;
        const div = document.createElement("div");
        div.className = "chat-msg mb-2";
        // Tekst wordt via textContent gezet — nooit innerHTML — XSS-veilig
        const header = document.createElement("div");
        const nameSpan = document.createElement("span");
        nameSpan.className = "chat-user";
        nameSpan.textContent = userName;
        const timeSpan = document.createElement("span");
        timeSpan.className = "chat-time";
        timeSpan.textContent = time;
        header.appendChild(nameSpan);
        header.appendChild(timeSpan);
        const body = document.createElement("p");
        body.className = "chat-content mb-0" + (isOwn ? " text-white" : " text-secondary");
        body.textContent = content;
        div.appendChild(header);
        div.appendChild(body);
        messagesEl.appendChild(div);
        messagesEl.scrollTop = messagesEl.scrollHeight;
    }

    // ── SignalR verbinding ────────────────────────────────────────────────────
    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl)
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveMessage", function (msg) {
        appendMessage(msg.userName, msg.content, msg.sentAt);
    });

    connection.onreconnecting(() => {
        statusEl.textContent = "Opnieuw verbinden...";
        sendBtn.disabled = true;
    });

    connection.onreconnected(() => {
        statusEl.textContent = "Verbonden";
        sendBtn.disabled = false;
    });

    connection.onclose(() => {
        statusEl.textContent = "Verbinding verbroken.";
        sendBtn.disabled = true;
    });

    async function startConnection() {
        try {
            await connection.start();
            statusEl.textContent = "Verbonden als " + currentUser;
            sendBtn.disabled = false;
            inputEl.focus();
        } catch (err) {
            statusEl.textContent = "Kan niet verbinden. Ververs de pagina.";
            console.error("SignalR fout:", err);
        }
    }

    // ── Bericht versturen ────────────────────────────────────────────────────
    async function sendMessage() {
        const content = inputEl.value.trim();
        if (!content || connection.state !== signalR.HubConnectionState.Connected) return;
        inputEl.value = "";
        try {
            await connection.invoke("SendMessage", content);
        } catch (err) {
            console.error("Versturen mislukt:", err);
        }
    }

    sendBtn.addEventListener("click", sendMessage);
    inputEl.addEventListener("keydown", function (e) {
        if (e.key === "Enter") sendMessage();
    });

    sendBtn.disabled = true;
    startConnection();
})();
