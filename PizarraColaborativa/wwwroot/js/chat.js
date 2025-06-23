document.addEventListener("DOMContentLoaded", () => {
    const input = document.getElementById("chatInput");
    const sendBtn = document.getElementById("sendMsg");
    const chatWriting = document.getElementById("chatWriting");
    const chatToggleBtn = document.getElementById("chatToggleBtn");
    const chatPanel = document.getElementById("floatingChatPanel");
    const chatIcon = document.getElementById("chatToggleIcon");
    const chatBadge = document.getElementById("chatBadge");
    
    const pizarraId = chatToggleBtn.dataset.pizarraId;
    let contadorNoVistos = chatToggleBtn.dataset.contadorMensajes ? parseInt(chatToggleBtn.dataset.contadorMensajes) : 0;

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
    let writingTimeout = null;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(`/chathub?pizarraId=${encodeURIComponent(pizarraId)}`)
        .build();

    connection.on("HistorialMensajes", (pizarraIdChat, mensajes) => {
        if (pizarraIdChat !== pizarraId) return;
        chat.innerHTML = "";
        mensajes.forEach(msg => {
            const nuevoMsg = document.createElement("div");
            nuevoMsg.innerHTML = `<b>${msg.nombreUsuario}:</b> ${msg.descripcion}`;
            chat.appendChild(nuevoMsg);
        });
        chat.scrollTop = chat.scrollHeight;
    });

    connection.on("RecibirMensaje", (userNameChat, mensaje, pizarraIdChat) => {
        if (pizarraIdChat !== pizarraId) return;

        const nuevoMsg = document.createElement("div");
        nuevoMsg.innerHTML = `<b>${userNameChat}:</b> ${mensaje}`;
        chat.appendChild(nuevoMsg);
        chat.scrollTop = chat.scrollHeight;

        if (!chatPanel.classList.contains("show")) {
            contadorNoVistos++;
            actualizarBadge();
        } else {
            connection.invoke("MarcarTodosComoVistos", pizarraId);
        }
    });

    connection.on("UsuarioEscribiendo", (userName, pizarraIdChat) => {
        if (pizarraIdChat === pizarraId) {
            chatWriting.innerText = `${userName} está escribiendo...`;
        }
    });

    connection.on("UsuarioDejoDeEscribir", (pizarraIdChat) => {
        if (pizarraIdChat === pizarraId) {
            chatWriting.innerText = "";
        }
    });

    connection.on("ActualizarContadorMensajes", (pizarraIdChat, cantidad) => {
        if (pizarraIdChat !== boardId) return;
        chatUnseenCount = cantidad;
        actualizarBadge();
    });

    async function enviarMensaje(mensaje) {
        if (!mensaje) return;
        await connection.invoke("EnviarMensaje", pizarraId, mensaje);
        await connection.invoke("UsuarioDejoDeEscribir", pizarraId);
    };

    function  usuarioEscribiendo() {
        connection.invoke("UsuarioEscribiendo", pizarraId);
        clearTimeout(writingTimeout);
        writingTimeout = setTimeout(() => {
            connection.invoke("UsuarioDejoDeEscribir", pizarraId);
        }, 1500);
    };

    sendBtn.addEventListener("click", () => {
        const mensaje = input.value.trim();
        if (mensaje === "") return;
        enviarMensaje(mensaje);
        input.value = "";
    });

    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            sendBtn.click();
        } else {
            usuarioEscribiendo();
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
