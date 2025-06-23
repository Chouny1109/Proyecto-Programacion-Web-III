const tray = document.getElementById("tray");
const trayToggleBtn = document.getElementById("trayToggleBtn");
const trayToggleIcon = document.getElementById("trayToggleIcon");
const clearTrayBtn = document.getElementById("clearTrayBtn");
const trayList = document.getElementById("trayList");
const trayBadge = document.getElementById("trayBadge");

let contadorNoVistos = trayToggleBtn.dataset.contadorNotificaciones ? parseInt(trayToggleBtn.dataset.contadorNotificaciones) : 0;

function actualizarBadge() {
    if (contadorNoVistos > 0) {
        trayBadge.textContent = contadorNoVistos;
        trayBadge.classList.remove("d-none");
    } else {
        trayBadge.classList.add("d-none");
    }
}
actualizarBadge();

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificacionHub")
    .build();

function mostrarMensajeVacio() {
    if (trayList.children.length === 0) {
        trayList.innerHTML = `<div class="text-center text-muted py-4">Sin notificaciones</div>`;
    }
}

connection.on("HistorialNotificaciones", (historial) => {
    trayList.innerHTML = "";

    historial.forEach(notificacion => {
        agregarNotificacionAlDOM(notificacion);
    });
    if (historial.length === 0) mostrarMensajeVacio();

    contadorNoVistos = historial.filter(n => !n.FueVista).length;
    actualizarBadge();
    tray.scrollTop = tray.scrollHeight;
});

connection.on("RecibirNotificacion", (notificacion) => {
    agregarNotificacionAlDOM(notificacion);
    if (!tray.classList.contains("show")) {
        contadorNoVistos++;
        actualizarBadge();
    }
    tray.scrollTop = tray.scrollHeight;
});

connection.on("ActualizarContadorNotificaciones", (cantidad) => {
    contadorNoVistos = cantidad;
    actualizarBadge();
});

connection.on("NotificacionEliminada", (id) => {
    const elem = document.querySelector(`[data-notificacion-id="${id}"]`);
    if (elem) elem.remove();

    mostrarMensajeVacio();
});

connection.on("BandejaVaciada", () => {
    trayList.innerHTML = "";
    mostrarMensajeVacio();
});

async function eliminarNotificacion(id) {
    await connection.invoke("EliminarNotificacion", id);
}

async function responderInvitacion(id, aceptada) {
    const elem = document.querySelector(`[data-notificacion-id="${id}"]`);
    const remitenteId = elem?.dataset.remitenteId;
    const destinatarioId = elem?.dataset.destinatarioId;
    const pizarraId = elem?.dataset.pizarraId;
    const rol = parseInt(elem?.dataset.rol);

    await connection.invoke("ResponderInvitacion", id, remitenteId, destinatarioId, pizarraId, rol, aceptada);
    if (aceptada && pizarraId)
        window.location.href = `/Pizarra/Dibujar/${pizarraId}`;
}

function agregarNotificacionAlDOM(notificacion) {
    if (trayList.children.length === 1 && trayList.firstElementChild && trayList.firstElementChild.classList.contains("text-muted")) {
        trayList.innerHTML = "";
    }

    const item = document.createElement("div");
    item.className = "notification-item d-flex align-items-start justify-content-between mb-2 p-2 rounded bg-white shadow-sm";
    item.style.border = "1px solid #eee";
    item.dataset.notificacionId = notificacion.id;
    if (notificacion.remitenteId) item.dataset.remitenteId = notificacion.remitenteId;
    if (notificacion.destinatarioId) item.dataset.destinatarioId = notificacion.destinatarioId;
    if (notificacion.pizarraId) item.dataset.pizarraId = notificacion.pizarraId;
    if (notificacion.pizarraNombre) item.dataset.pizarraNombre = notificacion.pizarraNombre;
    if (notificacion.rol) item.dataset.rol = notificacion.rol;

    const contentDiv = document.createElement("div");
    contentDiv.className = "flex-grow-1 pe-2";
    contentDiv.innerHTML = `
        <div class="fw-bold">${notificacion.titulo || ""}</div>
        <div class="small text-muted">${notificacion.fechaCreacion ? new Date(notificacion.fechaCreacion).toLocaleString() : ""}</div>
        ${notificacion.remitenteNombre ? `<div><small class="text-muted">De: ${notificacion.remitenteNombre}</small></div>` : ""}
        ${notificacion.pizarraNombre ? `<div><small class="text-muted">Pizarra: ${notificacion.pizarraNombre}</small></div>` : ""}
        <div>${notificacion.descripcion || ""}</div>
    `;

    const botonesDiv = document.createElement("div");
    botonesDiv.className = "d-flex flex-column align-items-end gap-1 ms-2";

    const esInvitacion = notificacion.titulo && notificacion.titulo.trim().toLowerCase() === "nueva invitación";
    if (esInvitacion) {
        const btnAceptar = document.createElement("button");
        btnAceptar.className = "btn btn-sm btn-success mb-1";
        btnAceptar.innerHTML = `<span class="material-symbols-outlined">check</span>`;
        btnAceptar.title = "Aceptar invitación";
        btnAceptar.addEventListener("click", () => responderInvitacion(notificacion.id, true));
        botonesDiv.appendChild(btnAceptar);

        const btnRechazar = document.createElement("button");
        btnRechazar.className = "btn btn-sm btn-danger mb-1";
        btnRechazar.innerHTML = `<span class="material-symbols-outlined">close</span>`;
        btnRechazar.title = "Rechazar invitación";
        btnRechazar.addEventListener("click", () => responderInvitacion(notificacion.id, false));
        botonesDiv.appendChild(btnRechazar);
    }

    const btnEliminar = document.createElement("button");
    btnEliminar.className = "btn btn-sm btn-outline-secondary";
    btnEliminar.innerHTML = `<span class="material-symbols-outlined">delete</span>`;
    btnEliminar.title = "Eliminar notificación";
    btnEliminar.addEventListener("click", () => eliminarNotificacion(notificacion.id));
    botonesDiv.appendChild(btnEliminar);

    item.appendChild(contentDiv);
    item.appendChild(botonesDiv);

    trayList.prepend(item);
}

trayToggleBtn.addEventListener("click", () => {
    const isOpen = tray.classList.toggle("show");
    trayToggleIcon.textContent = isOpen ? "close" : "notifications";
    if (isOpen) {
        contadorNoVistos = 0;
        actualizarBadge();
        connection.invoke("MarcarTodasComoVistas");
    }
});

clearTrayBtn.addEventListener('click', async () => {
    await connection.invoke("VaciarBandeja");
});

document.addEventListener("DOMContentLoaded", () => {
    let validacionPendiente = false;
    let formActual = null;
    let usuarioInputActual = null;
    let errorMsgActual = null;

    document.querySelectorAll("form[data-eliminar-pizarra]").forEach(form => {
        form.addEventListener("submit", async function (e) {
            e.preventDefault();
            const pizarraId = form.querySelector("input[name='pizarraId']").value;
            const remitenteId = form.dataset.remitenteId;
            const destinatariosIds = JSON.parse(form.dataset.destinatariosIds);
            const pizarraNombre = form.dataset.pizarranombre;

            await connection.invoke("EnviarPizarraEliminada", remitenteId, destinatariosIds, pizarraId, pizarraNombre);

            form.submit();
        });
    });

    document.querySelectorAll("form[data-compartir-pizarra]").forEach(form => {
        const usuarioInput = form.querySelector("input[name='usuario']");
        let errorMsg = form.querySelector(".usuario-error-msg");
        if (!errorMsg) {
            errorMsg = document.createElement("div");
            errorMsg.className = "usuario-error-msg text-danger mt-1";
            usuarioInput.parentNode.appendChild(errorMsg);
        }

        form.addEventListener("submit", async function (e) {
            e.preventDefault();
            errorMsg.textContent = "";

            const usuario = usuarioInput.value.trim();
            const pizarraId = form.querySelector("input[name='pizarraId']").value;

            validacionPendiente = true;
            formActual = form;
            usuarioInputActual = usuarioInput;
            errorMsgActual = errorMsg;

            await connection.invoke("ValidarUsuario", usuario, pizarraId);
        });
    });

    connection.on("ValidacionUsuarioInvitacion", async function (resultado) {
        if (!validacionPendiente) return;
        validacionPendiente = false;

        if (!resultado.existe) {
            errorMsgActual.textContent = "El usuario no existe.";
            usuarioInputActual.focus();
            return;
        }
        if (resultado.relacionado) {
            errorMsgActual.textContent = "El usuario ya está relacionado con la pizarra.";
            usuarioInputActual.focus();
            return;
        }

        const remitenteId = formActual.dataset.remitenteId;
        const pizarraNombre = formActual.dataset.pizarranombre;
        const pizarraId = formActual.querySelector("input[name='pizarraId']").value;
        const usuario = usuarioInputActual.value.trim();
        const rol = parseInt(formActual.querySelector("input[name='rol']:checked").value);

        try {
            await connection.invoke("EnviarInvitacion", remitenteId, usuario, pizarraId, pizarraNombre, rol);
        } catch (err) {
            errorMsgActual.textContent = "Error enviando invitación.";
            return;
        }

        const modal = bootstrap.Modal.getInstance(formActual.closest(".modal"));
        modal.hide();
    });
});

connection.start().catch(err => console.error(err.toString()));
