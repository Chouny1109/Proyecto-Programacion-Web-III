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
    const input = document.getElementById("chatInput");
    const sendBtn = document.getElementById("sendMsg");
    const writingMsg = document.getElementById("writing-msg");
    const chatToggleBtn = document.getElementById("chatToggleBtn");
    const chatPanel = document.getElementById("floatingChatPanel");
    const chatIcon = document.getElementById("chatToggleIcon");
    const chatBadge = document.getElementById("chatBadge");

    if (!window.chatData) return;

    const { userId, userName, pizarraId, mensajesNoVistos } = window.chatData;
    let contadorNoVistos = mensajesNoVistos;

    function actualizarBadge() {
        if (contadorNoVistos > 0) {
            chatBadge.textContent = contadorNoVistos;
            chatBadge.classList.remove("d-none");
        } else {
            chatBadge.classList.add("d-none");
        }
    }
    actualizarBadge();

    const chat = document.getElementById(`chat-${pizarraId}`);
    const writingTimers = {};

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chathub?pizarraId=${encodeURIComponent(pizarraId)}`)
        .build();

    connection.on("HistorialMensajes", (pizarraChatId, mensajes) => {
        if (pizarraChatId !== pizarraId) return;
        mensajes.forEach(msg => {
            const nuevoMsg = document.createElement("div");
            nuevoMsg.innerHTML = `<b>${msg.nombreUsuario}:</b> ${msg.descripcion}`;
            chat.appendChild(nuevoMsg);
        });
        chat.scrollTop = chat.scrollHeight;
    });

    connection.on("RecibirMensaje", (userIdChat, userNameChat, mensaje, pizarraChatId) => {
        if (pizarraChatId !== pizarraId) return;

        const nuevoMsg = document.createElement("div");
        nuevoMsg.innerHTML = `<b>${userNameChat}:</b> ${mensaje}`;
        chat.appendChild(nuevoMsg);
        chat.scrollTop = chat.scrollHeight;

        if (!chatPanel.classList.contains("show")) {
            contadorNoVistos++;
            actualizarBadge();
        }
    });

    connection.on("UsuarioEscribiendo", (userName, pizarraChatId) => {
        if (pizarraChatId === pizarraId) {
            writingMsg.innerText = `${userName} está escribiendo...`;
        }
    });

    connection.on("UsuarioDejoDeEscribir", (pizarraChatId) => {
        if (pizarraChatId === pizarraId) {
            writingMsg.innerText = "";
        }
    });

    window.enviarMensaje = async function (mensaje) {
        if (!mensaje) return;
        await connection.invoke("EnviarMensaje", pizarraId, mensaje);
        await connection.invoke("UsuarioDejoDeEscribir", pizarraId);
    };

    window.usuarioEscribiendo = function () {
        connection.invoke("UsuarioEscribiendo", pizarraId);
        clearTimeout(writingTimers[pizarraId]);
        writingTimers[pizarraId] = setTimeout(() => {
            connection.invoke("UsuarioDejoDeEscribir", pizarraId);
        }, 1500);
    };

    sendBtn.addEventListener("click", () => {
        const mensaje = input.value.trim();
        if (mensaje === "") return;
        window.enviarMensaje(mensaje);
        input.value = "";
    });

    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            sendBtn.click();
        } else {
            window.usuarioEscribiendo();
        }
    });

    chatToggleBtn.addEventListener("click", () => {
        const isOpen = chatPanel.classList.toggle("show");
        chatIcon.textContent = isOpen ? "close" : "chat";
        
        if (isOpen) {
            contadorNoVistos = 0;
            actualizarBadge();
            connection.invoke("MarcarTodosComoVistos", pizarraId);
        }
    });

    connection.start()
        .then(() => console.log("Conexión SignalR iniciada"))
        .catch(err => console.error("Error de conexión SignalR:", err));
});