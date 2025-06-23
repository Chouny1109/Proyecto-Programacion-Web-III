let pizarraId = document.getElementById("info-pizarra").dataset.pizarraid;
let isPenDown = false;
let conexion = new signalR.HubConnectionBuilder()
    .withUrl("/dibujohub?pizarraid=" + encodeURIComponent(pizarraId))
    .build();

let username = window.userName;
let canvas = document.getElementById("area");
let papel = canvas.getContext('2d');


canvas.addEventListener("mousemove", function (e) {
    const rect = canvas.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const y = e.clientY - rect.top;

    conexion.invoke("ActualizarCursor", pizarraId, userName, x, y);
});

const cursors = {};

const cursorTimers = {}; 

conexion.on("MostrarCursor", function (username, x, y) {
   
    if (!cursors[username]) {
        const div = document.createElement("div");
        div.className = "cursor-remoto";
        const color = generarColorDesdeTexto(username);

        div.innerHTML = `
        <div class="cursor-punto" style="background-color: ${color};"></div>
        <div class="cursor-nombre" style="background-color: ${color};">${username}</div>`;

        document.getElementById("cursors").appendChild(div);
        cursors[username] = div;
    }


    // Mover cursor a la nueva posición
    const el = cursors[username];
    el.style.left = `${x}px`;
    el.style.top = `${y}px`;
    el.style.display = "block";

    // Si ya había un timeout, se resetea
    if (cursorTimers[username]) clearTimeout(cursorTimers[username]);

    // Ocultar después de 5 segundos de inactividad
    cursorTimers[username] = setTimeout(() => {
        el.style.display = "none";
    }, 5000);
});
function mostrarListaUsuarios() {
    document.getElementById("panelUsuarios").style.display = "block";
    conexion.invoke("ObtenerUsuariosDePizarra", pizarraId);
}

function cerrarPanelUsuarios() {
    document.getElementById("panelUsuarios").style.display = "none";
}

conexion.on("ListaUsuariosPizarra", function (usuarios) {
    const contenedor = document.getElementById("lista-usuarios");
    contenedor.innerHTML = "";

    usuarios.forEach(u => {
        const div = document.createElement("div");
        div.classList.add("d-flex", "justify-content-between", "align-items-center", "mb-1");

        const nombre = document.createElement("span");
        nombre.textContent = u.userName;

        const btn = document.createElement("button");
        btn.textContent = "✖";
        btn.className = "btn btn-sm btn-outline-danger";
        btn.title = "Expulsar usuario";
        btn.onclick = () => {
            if (confirm(`¿Expulsar a ${u.userName}?`)) {
                conexion.invoke("ExpulsarUsuarioDePizarra", u.userId, pizarraId);
            }
        };

        div.appendChild(nombre);
        div.appendChild(btn);
        contenedor.appendChild(div);
    });
});

conexion.on("UsuarioExpulsado", function (mensaje) {
    console.log(" Usuario fue expulsado:", mensaje); 
    Swal.fire({
        icon: 'error',
        title: 'Expulsado',
        text: mensaje,
        confirmButtonText: 'Aceptar'
    }).then(() => {
        window.location.href = "/Home/Index";
    });
});

conexion.on("UsuariosConectados", function (usuarios) {
    const contenedor = document.getElementById("usuarios-conectados");
    contenedor.innerHTML = "";

    usuarios.forEach(u => {
        const wrapper = document.createElement("div");
        wrapper.className = "usuario-wrapper";

        const burbuja = document.createElement("div");
        burbuja.className = "usuario-burbuja";
        burbuja.style.backgroundColor = generarColorDesdeTexto(u.userId);
        burbuja.textContent = obtenerIniciales(u.userName);

        const nombre = document.createElement("div");
        nombre.className = "usuario-nombre";
        nombre.textContent = u.userName;

        wrapper.appendChild(burbuja);
        wrapper.appendChild(nombre);
        contenedor.appendChild(wrapper);
    });
});

function obtenerIniciales(nombre) {
    const partes = nombre.trim().split(" ");
    if (partes.length === 1) return partes[0].substring(0, 2).toUpperCase();
    return (partes[0][0] + partes[1][0]).toUpperCase();
}

function generarColorDesdeTexto(texto) {
    let hash = 0;
    for (let i = 0; i < texto.length; i++) {
        hash = texto.charCodeAt(i) + ((hash << 5) - hash);
    }
    const hue = hash % 360;
    return `hsl(${hue}, 70%, 60%)`;
}


//actualizar nombre pizarra 
function cambiarNombre() {
    const nuevoNombre = document.getElementById("nombrePizarraInput").value;
    conexion.invoke("CambiarNombrePizarra", pizarraId, nuevoNombre);
}

conexion.on("NombrePizarraCambiado", function (nuevoNombre) {
    document.getElementById("nombrePizarraTexto").innerText = nuevoNombre;
});


//Cargar trazos guardados
conexion.on("CargarTrazos", function (trazos) {
    papel.clearRect(0, 0, canvas.width, canvas.height); 

    trazos.forEach(t => dibujarTrazo(t));
});

conexion.on("ActualizarPosicionImagen", (idImg, posX, posY) => {
    // Buscar el sticky div con ese id
    const stickyDiv = document.querySelector(`div.sticky[data-id-imagen='${idImg}']`);
    if (!stickyDiv) {

        console.log("entro al if");

        return;
    }

    stickyDiv.style.left = posX + "px";
    stickyDiv.style.top = posY + "px";

    console.log(posX + " x " + posY);

});

conexion.on("SacarImagen", (idImg) => {
    // Buscar el sticky div con ese id
    const stickyDiv = document.querySelector(`div.sticky[data-id-imagen='${idImg}']`);
    if (!stickyDiv) {

        console.log("entro al if");

        return;
    }
    stickyDiv.remove();

});



conexion.on("RecibirImagen", function (base64, x, y, idImg) {
    let img = new Image();
    img.src = base64;
    addSticky(img, idImg);
});






// Dibuja los trazos traidos.
function dibujarTrazo(trazo) {
    const xInicio = trazo.xinicio ?? 0;
    const yInicio = trazo.yinicio ?? 0;
    const xFin = trazo.xfin ?? 0;
    const yFin = trazo.yfin ?? 0;
    const color = trazo.color ?? '#000000';
    const grosor = trazo.grosor ?? 1;

    papel.beginPath();
    papel.strokeStyle = color;
    papel.lineWidth = grosor;
    papel.moveTo(xInicio, yInicio);
    papel.lineTo(xFin, yFin);
    papel.stroke();
}
//Cargar textos guardados.
conexion.on("CargarTextos", function (textos) {
    console.log("Textos recibidos:", textos); 
    for (const texto of textos) {
        crearTextoEditable(
            texto.contenido,
            texto.posX,
            texto.posY,
            texto.color,
            texto.tamano,
            texto.id,
            false
        );
    }
});


conexion.on("DibujarTrazoCompleto", function (segmentos) {
    for (const s of segmentos) {
        dibujar(s.color, s.xinicio, s.yinicio, s.xfin, s.yfin, s.grosor);
    }
});


let presionMouse = false;
let x = 0;
let y = 0;
let trazoActual = [];      
let grupoTrazoId = null;

function dibujar(color1, corX, corY, corXFinal, corYFinal, tamanioLinea) {
        papel.beginPath();
        papel.strokeStyle = color1;
        papel.lineWidth = tamanioLinea
        papel.moveTo(corX, corY);
        papel.lineTo(corXFinal, corYFinal);
        papel.stroke();
        papel.closePath();

    }

canvas.addEventListener("mousedown", presionaMouse)
function presionaMouse(event) {
    presionMouse = true;
    x = event.offsetX;
    y = event.offsetY;
    trazoActual = [];
    grupoTrazoId = crypto.randomUUID();
}

 canvas.addEventListener("mousemove", dibujarConMouse)
    function dibujarConMouse(event) {
         const nuevoX = event.offsetX;
         const nuevoY = event.offsetY;

    if (!presionMouse || (nuevoX === x && nuevoY === y)) return;

        if (modoGoma) {
            dibujar(colorFondo, x, y, nuevoX, nuevoY, 12);         
            if (conexion.state === signalR.HubConnectionState.Connected) {
                const centroX = (x + nuevoX) / 2;
                const centroY = (y + nuevoY) / 2;
                const radioGoma = 10;

                conexion.invoke("BorrarTrazosEnRango", pizarraId, centroX, centroY, radioGoma)
                    .catch(err => console.error("Error al borrar con goma:", err));
            }
    } else {
        const segmento = {
            xinicio: x,
            yinicio: y,
            xfin: nuevoX,
            yfin: nuevoY,
            color: color,
            grosor: tamanioInicial
        };
        dibujar(segmento.color, segmento.xinicio, segmento.yinicio, segmento.xfin, segmento.yfin, segmento.grosor);
        trazoActual.push(segmento);
    }

    x = nuevoX;
    y = nuevoY;
}

   

    canvas.addEventListener("mouseup", soltoMouse)
    function soltoMouse(event) {
        presionMouse = false;
        x = event.offsetX;
        y = event.offsetY;

        if (trazoActual.length > 0 && conexion.state === signalR.HubConnectionState.Connected) {
        conexion.invoke("EnviarTrazoCompleto", pizarraId, trazoActual, grupoTrazoId)
            .catch(err => console.error("Error al enviar trazo completo:", err));
    }
    trazoActual = [];


    }

    let tamanioLapiz = document.getElementById("tamanio_lapiz")
    let tamanioInicial = 2



    document.addEventListener("change", tamanioLapizz);

    function tamanioLapizz() {
        tamanioInicial = parseInt(tamanioLapiz.value);
}

document.addEventListener("change", colorLinea);

let color = 'black';
function colorLinea() {

    color = document.getElementById("color_lapiz").value

}


document.addEventListener("change",fondoArea)

let colorFondo = 'white'

function fondoArea() {

    colorFondo = document.getElementById("color_fondo").value

    canvas.style.backgroundColor = colorFondo;


    if (conexion.state === signalR.HubConnectionState.Connected) {
        conexion.invoke("CambiarColorFondo", pizarraId, colorFondo)
            .catch(err => console.error(err.toString()));

    }
}

conexion.on("ColorFondoCambiado", function (colorFondo) {
    canvas.style.backgroundColor = colorFondo;
    document.getElementById("color_fondo").value = colorFondo;
});


let limpiarLineas = document.getElementById("limpiar");

limpiarLineas.addEventListener("click",limpiarTodo)
function limpiarTodo() {
    if (conexion.state === signalR.HubConnectionState.Connected) {
        conexion.invoke("SendLimpiar", pizarraId)

            .catch(err => console.error(err.toString()));
    }
    limpiarTextos(); // también limpia localmente
}
function limpiarTextos() {
    for (const id in textos) {
        const div = textos[id].div;
        div.remove(); // elimina el div del DOM
    }
    textos = {}; // limpia el registro local
}

conexion.on("ReceiveLimpiar", function () {
    papel.clearRect(0, 0, canvas.width, canvas.height);
    limpiarTextos(); // 
    colorFondo = 'white'; 

    canvas.style.backgroundColor = colorFondo;
});

//insertar Texto

let textos = {}; // diccionario de textos por id
let textoSeleccionadoId = null;
let textoInicialX = 0;
let textoInicialY = 0;

// Función para seleccionar texto
function seleccionarTexto(id) {
    textoSeleccionadoId = id;

    // Quitar resaltado de todos
    for (const t of Object.values(textos)) {
        t.div.style.outline = "none";
    }

    const texto = textos[id];
    if (!texto) return;

    texto.div.style.outline = "2px dashed gray";

    // Actualizar inputs
    document.getElementById("colorTexto").value = texto.color;
    document.getElementById("tamanioTexto").value = texto.tamano;
}

// Crear texto editable
function crearTextoEditable(texto, x, y, color = 'black', tamano = 20, id = null, enviar = true) {
    if (!id) id = crypto.randomUUID();

    // Si ya existe, solo actualizarlo
    if (textos[id] && textos[id].div) {
        let div = textos[id].div;
        div.textContent = texto;
        div.style.left = x + "px";
        div.style.top = y + "px";
        div.style.color = color;
        div.style.fontSize = tamano + "px";
        textos[id] = { div, x, y, texto, color, tamano };
        return;
    }

    let div = document.createElement("div");
    div.contentEditable = true;
    div.textContent = texto;
    div.style.position = "absolute";
    div.style.left = x + "px";
    div.style.top = y + "px";
    div.style.color = color;
    div.style.fontSize = tamano + "px";
    div.style.cursor = "move";
    div.style.userSelect = "text";
    div.id = id;
    div.style.pointerEvents = "auto";
    div.style.zIndex = "10";

    document.getElementById("contenedorTextos").appendChild(div);

    textos[id] = { div, x, y, texto, color, tamano };

    if (enviar && conexion.state === signalR.HubConnectionState.Connected) {
        conexion.invoke("CrearOEditarTexto", pizarraId, {
            id: id,
            contenido: texto,
            x: x,
            y: y,
            color: color,
            tamano: tamano
        }).catch(e => console.error(e));
    }

    // Manejo de selección
    div.addEventListener("mousedown", (e) => {
        isDragging = true;
        offsetX = e.clientX - div.offsetLeft;
        offsetY = e.clientY - div.offsetTop;
        div.style.userSelect = "none";

        seleccionarTexto(id);
        textoInicialX = parseInt(div.style.left);
        textoInicialY = parseInt(div.style.top);
        e.stopPropagation();
    });

    let isDragging = false;
    let offsetX = 0;
    let offsetY = 0;

    window.addEventListener("mousemove", (e) => {
        if (!isDragging) return;
        let nx = e.clientX - offsetX;
        let ny = e.clientY - offsetY;
        div.style.left = nx + "px";
        div.style.top = ny + "px";
        textos[id].x = nx;
        textos[id].y = ny;
    });

    window.addEventListener("mouseup", (e) => {
        if (isDragging) {
            isDragging = false;
            div.style.userSelect = "text";

            if (textoSeleccionadoId) {
                const texto = textos[textoSeleccionadoId];
                const xFinal = texto.x;
                const yFinal = texto.y;

                if (conexion.state === signalR.HubConnectionState.Connected) {
                    conexion.invoke("MoverTexto", pizarraId, textoSeleccionadoId, textoInicialX, textoInicialY, xFinal, yFinal)
                        .catch(e => console.error(e));
                }
            }
        }
    });

    // Guardar contenido original al hacer foco
    let contenidoOriginal = texto;
    div.addEventListener("focus", () => {
        contenidoOriginal = div.textContent.trim();
    });

    // Manejar cuando se deja de editar
    div.addEventListener("blur", () => {
        const nuevoContenido = div.textContent.trim();

        if (nuevoContenido === "") {
            div.remove();
            delete textos[id];
            if (conexion.state === signalR.HubConnectionState.Connected) {
                conexion.invoke("EliminarTexto", pizarraId, id)
                    .catch(e => console.error("Error al eliminar texto vacío:", e));
            }
            return;
        }

        if (nuevoContenido !== contenidoOriginal) {
            textos[id].texto = nuevoContenido;

            if (conexion.state === signalR.HubConnectionState.Connected) {
                conexion.invoke("CrearOEditarTexto", pizarraId, {
                    id: id,
                    contenido: nuevoContenido,
                    x: textos[id].x,
                    y: textos[id].y,
                    color: textos[id].color,
                    tamano: textos[id].tamano
                }).catch(e => console.error(e));
            }
        }
    });
}


// Aplicar color automáticamente al cambiar el input
document.getElementById("colorTexto").addEventListener("input", () => {
    if (!textoSeleccionadoId) return;
    const nuevoColor = document.getElementById("colorTexto").value;
    const texto = textos[textoSeleccionadoId];
    texto.color = nuevoColor;
    texto.div.style.color = nuevoColor;

    if (conexion.state === signalR.HubConnectionState.Connected) {
        conexion.invoke("CrearOEditarTexto", pizarraId, {
            id: textoSeleccionadoId,
            contenido: texto.div.textContent,
            x: texto.x,
            y: texto.y,
            color: nuevoColor,
            tamano: texto.tamano
        }).catch(e => console.error(e));
    }
});

// Aplicar tamaño automáticamente al cambiar el input
document.getElementById("tamanioTexto").addEventListener("input", () => {
    if (!textoSeleccionadoId) return;
    const nuevoTamano = parseInt(document.getElementById("tamanioTexto").value);
    const texto = textos[textoSeleccionadoId];
    texto.tamano = nuevoTamano;
    texto.div.style.fontSize = `${nuevoTamano}px`;

    if (conexion.state === signalR.HubConnectionState.Connected) {
        conexion.invoke("CrearOEditarTexto", pizarraId, {
            id: textoSeleccionadoId,
            contenido: texto.div.textContent,
            x: texto.x,
            y: texto.y,
            color: texto.color,
            tamano: nuevoTamano
        }).catch(e => console.error(e));
    }
});

// Deseleccionar texto al hacer clic fuera
document.addEventListener("mousedown", function (e) {
    if (
        !e.target.closest("div[contenteditable='true']") &&
        !e.target.closest("#colorTexto") &&
        !e.target.closest("#tamanioTexto")
    ) {
        for (const t of Object.values(textos)) {
            t.div.style.outline = "none";
        }
        textoSeleccionadoId = null;
    }
});



// Recibir textos actualizados desde el servidor
conexion.on("TextoActualizado", (texto) => {
    console.log("Texto recibido:", texto);
    if (textos[texto.id]) {
        // Actualizar texto existente
        let div = textos[texto.id].div;
        div.textContent = texto.contenido;
        div.style.left = texto.x + "px";
        div.style.top = texto.y + "px";
        div.style.color = texto.color;
        div.style.fontSize = texto.tamano + "px";

        textos[texto.id] = { div, x: texto.x, y: texto.y, texto: texto.contenido, color: texto.color, tamano: texto.tamano };
    } else {
        // Crear nuevo texto recibido
        crearTextoEditable(texto.contenido, texto.x, texto.y, texto.color, texto.tamano, texto.id, false);
    }
});

// Recibir movimiento de texto desde el servidor
conexion.on("TextoMovido", (id, x, y) => {
    if (textos[id]) {
        let div = textos[id].div;
        div.style.left = x + "px";
        div.style.top = y + "px";
        textos[id].x = x;
        textos[id].y = y;
    }
});


document.getElementById("btnInsertarTexto").addEventListener("click", () => {
    const texto = document.getElementById("textoContenido").value || "Nuevo texto";
    const color = document.getElementById("colorTexto").value;
    const tamano = parseInt(document.getElementById("tamanioTexto").value) || 20;
    crearTextoEditable(texto, 500, 250, color, tamano, null, true);
});

let undo = document.getElementById("undo");
let redo = document.getElementById("redo");
undo.addEventListener("click", deshacerUltimaAccion);
redo.addEventListener("click", rehacerAccion);

function deshacerUltimaAccion() {
    if (conexion.state === signalR.HubConnectionState.Connected) {
        conexion.invoke("DeshacerUltimaAccion", pizarraId)
            .catch(err => console.error("Error al hacer undo:", err.toString()));
    }
}

function rehacerAccion() {

    if (conexion.state === signalR.HubConnectionState.Connected) {
        conexion.invoke("RehacerUltimaAccion", pizarraId)
            .catch(err => console.error("Error al hacer redo:", err.toString()));
    }
}
conexion.on("TextoEliminado", function (id) {
    if (textos[id]) {
        textos[id].div.remove(); // eliminar el div visualmente
        delete textos[id]; // eliminar del diccionario local
    }
});

conexion.on("TrazoRehecho", (trazo) => {
    dibujar(trazo.color, trazo.xinicio, trazo.yinicio, trazo.xfin, trazo.yfin, trazo.grosor, false);
});

conexion.on("TrazoEliminado",
    function (trazoEliminado) {
    // Limpiar todo el canvas
    papel.clearRect(0, 0, canvas.width, canvas.height);

    // Solicitar al servidor todos los trazos actualizados
    conexion.invoke("SolicitarTrazos", pizarraId).catch(console.error);
});


// empieza la conexión
conexion.start().then(() => {
    console.log("conectado correctamente");
}).catch(function (err) {
    return console.error("Error al iniciar:", err.toString());
});

let modoGoma = false;

document.getElementById("btnGoma").addEventListener("click", () => {
    modoGoma = !modoGoma;
    if (modoGoma) {
        canvas.style.cursor = "cell"; // apariencia tipo goma
    } else {
        canvas.style.cursor = "crosshair";
    }
});
document.getElementById("photo-upload").addEventListener("change", function (event) {
const file = event.target.files[0];
if (!file) return;



const reader = new FileReader();
reader.onload = function (e) {
    const base64 = e.target.result;

    redimensionarImagen(base64, 300, 300, function (base64Reducida) {
        const img = new Image();
        const blob = base64ToBlob(base64Reducida);
        const url = URL.createObjectURL(blob);
        img.src = url;

        img.onload = () => {
            const idImg = "img-" + Date.now() + "-" + Math.floor(Math.random() * 10000);

       
            addSticky(img, idImg);

            const posX = 100;
            const posY = 100;

    
            conexion.invoke("EnviarImagen", pizarraId, base64Reducida, posX, posY, idImg)
                .catch(err => console.error("Error al enviar:", err.toString()));
        };
    });
};
reader.readAsDataURL(file);

event.target.value = ""; 
});
function base64ToBlob(base64) {
    const parts = base64.split(';base64,');
    const mime = parts[0].split(':')[1];
    const binary = atob(parts[1]);
    const array = new Uint8Array(binary.length);
    for (let i = 0; i < binary.length; i++) {
        array[i] = binary.charCodeAt(i);
    }
    return new Blob([array], { type: mime });
}

function redimensionarImagen(base64Original, maxAncho, maxAlto, callback) {
    const img = new Image();
    img.onload = () => {
        let ratio = Math.min(maxAncho / img.width, maxAlto / img.height, 1); 
        const ancho = Math.round(img.width * ratio);
        const alto = Math.round(img.height * ratio);

        const canvas = document.createElement("canvas");
        canvas.width = ancho;
        canvas.height = alto;

        const ctx = canvas.getContext("2d");
        ctx.clearRect(0, 0, ancho, alto);

        ctx.imageSmoothingEnabled = true;
        ctx.imageSmoothingQuality = "high";

        ctx.drawImage(img, 0, 0, ancho, alto);


        const base64Reducida = canvas.toDataURL("image/jpeg", 1);

        console.log("Tamaño final:", base64Reducida.length);
        callback(base64Reducida);
    };
    img.src = base64Original;

 

}

function generarColorDesdeTexto(texto) {
    let hash = 0;
    for (let i = 0; i < texto.length; i++) {
        hash = texto.charCodeAt(i) + ((hash << 5) - hash);
    }
    const hue = hash % 360;
    return `hsl(${hue}, 70%, 60%)`;
}
