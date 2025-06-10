document.addEventListener("DOMContentLoaded", function () {
    // --- SIDEBAR ---
    const toggleBtn = document.getElementById('toggleSidebar');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('main-content');
    const menuIcon = document.getElementById('menuIcon');

    const sidebarState = localStorage.getItem('sidebarState');
    if (sidebarState === 'open') {
        sidebar.classList.add('active');
        mainContent.classList.add('shifted');
        menuIcon.textContent = 'menu_open';
    }

    toggleBtn.addEventListener('click', () => {
        const isActive = sidebar.classList.toggle('active');
        mainContent.classList.toggle('shifted');
        localStorage.setItem('sidebarState', isActive ? 'open' : 'closed');
        menuIcon.textContent = isActive ? 'menu_open' : 'menu';
    });

    // --- FLOATING CHAT ---
    const selector = document.getElementById("chatSelector");
    const input = document.getElementById("chatInput");
    const sendBtn = document.getElementById("sendMsg");
    const writingMsg = document.getElementById("writing-msg");
    const chatToggleBtn = document.getElementById("chatToggleBtn");
    const chatPanel = document.getElementById("floatingChatPanel");
    const chatIcon = document.getElementById("chatToggleIcon");

    const chatMessagesContainers = document.querySelectorAll(".chat-messages");

    function mostrarChatSeleccionado() {
        chatMessagesContainers.forEach(c => c.classList.add("d-none"));
        const selectedChat = document.getElementById(`chat-${selector.value}`);
        if (selectedChat) selectedChat.classList.remove("d-none");
        writingMsg.innerText = "";
    }

    mostrarChatSeleccionado();
    selector.addEventListener("change", mostrarChatSeleccionado);

    sendBtn.addEventListener("click", () => {
        const mensaje = input.value.trim();
        if (mensaje === "") return;

        const selected = selector.value;
        window.enviarMensaje(selected, mensaje);
        input.value = "";
    });

    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault(); 
            sendBtn.click();
        } else {
            window.usuarioEscribiendo(selector.value);
        }
    });

    chatToggleBtn.addEventListener("click", () => {
        chatPanel.classList.toggle("show");
        chatIcon.textContent = chatPanel.classList.contains("show") ? "close" : "chat";
    });

    // --- SIGNALR ---
    if (!window.chatData) return;

    const { userId, userName, pizarras } = window.chatData;
    const writingTimers = {};

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chathub?userId=${encodeURIComponent(userId)}`)
        .build();

    connection.on("RecibirMensaje", (userIdRemitente, userNameRemitente, mensaje, pizarraId) => {
        const chat = document.getElementById(`chat-${pizarraId}`);
        if (chat) {
            const nuevoMsg = document.createElement("div");
            nuevoMsg.innerHTML = `<b>${userNameRemitente}:</b> ${mensaje}`;
            chat.appendChild(nuevoMsg);
            chat.scrollTop = chat.scrollHeight;
        }
    });

    connection.on("UsuarioEscribiendo", (userNamewriting, pizarraId) => {
        if (selector.value === pizarraId) {
            writingMsg.innerText = `${userNamewriting} está escribiendo...`;
        }
    });

    connection.on("UsuarioDejoDeEscribir", (userIdEscribio, pizarraId) => {
        if (selector.value === pizarraId) {
            writingMsg.innerText = "";
        }
    });

    window.enviarMensaje = async function (pizarraId, mensaje) {
        if (!mensaje) return;
        await connection.invoke("EnviarMensaje", pizarraId, userId, mensaje);
        await connection.invoke("UsuarioDejoDeEscribir", pizarraId, userId);
    };

    window.usuarioEscribiendo = function (pizarraId) {
        connection.invoke("UsuarioEscribiendo", pizarraId, userId);
        if (writingTimers[pizarraId]) {
            clearTimeout(writingTimers[pizarraId]);
        }
        writingTimers[pizarraId] = setTimeout(() => {
            connection.invoke("UsuarioDejoDeEscribir", pizarraId, userId);
        }, 1500);
    };

    connection.start()
        .then(() => console.log("Conexión SignalR iniciada"))
        .catch(err => console.error("Error de conexión SignalR:", err));
});