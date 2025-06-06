let pizarraId = document.getElementById("info-pizarra").dataset.pizarraid;

let conexion = new signalR.HubConnectionBuilder()
    .withUrl("/dibujohub?pizarraid=" + encodeURIComponent(pizarraId))
    .build();


let canvas = document.getElementById("area");
let papel = canvas.getContext('2d');


//actualizar nombre pizarra 
function cambiarNombre() {
    const nuevoNombre = document.getElementById("nombrePizarraInput").value;
    conexion.invoke("CambiarNombrePizarra", pizarraId, nuevoNombre);
}

conexion.on("NombrePizarraCambiado", function (nuevoNombre) {
    document.getElementById("tituloPizarra").innerText = nuevoNombre;
});


//Cargar trazos guardados
conexion.on("CargarTrazos", function (trazos) {
    trazos.forEach(t => dibujarTrazo(t));
});

// Dibuja los trazos traidos.
function dibujarTrazo(trazo) {
    const xInicio = trazo.xinicio ?? 0;
    const yInicio = trazo.yinicio ?? 0;
    const xFin = trazo.xfin ?? 0;
    const yFin = trazo.yfin ?? 0;
    const color = trazo.color ?? '#000000';
    const grosor = trazo.grosor ?? 2;

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




function dibujar(color1, corX, corY, corXFinal, corYFinal, tamanioLinea, enviar = true) {
        papel.beginPath();
        papel.strokeStyle = color1;
        papel.lineWidth = tamanioLinea
        papel.moveTo(corX, corY);
        papel.lineTo(corXFinal, corYFinal);
        papel.stroke();
        papel.closePath();


        if (enviar && conexion.state === signalR.HubConnectionState.Connected) {
            conexion.invoke("SendDibujo", pizarraId, color, corX, corY, corXFinal, corYFinal, tamanioInicial)
                .catch(function (err) {
                    return console.error("Error al enviar dibujo:", err.toString());
                });
        } else if (!enviar) {
            // solo dibuja  
        }


    }


    canvas.addEventListener("mousemove", dibujarConMouse)
    function dibujarConMouse(event) {

        if (presionMouse) {
            dibujar(color, x, y, event.offsetX, event.offsetY, tamanioInicial);
        }

        x = event.offsetX;
        y = event.offsetY;
    }


    let presionMouse = false;

    canvas.addEventListener("mousedown", presionaMouse)
    function presionaMouse(event) {
        presionMouse = true;
        x = event.offsetX;
        y = event.offsetY;
    }

    canvas.addEventListener("mouseup", soltoMouse)
    function soltoMouse(event) {
        presionMouse = false;

        x = event.offsetX;
        y = event.offsetY;
    }

    let tamanioLapiz = document.getElementById("tamanio_lapiz")
    let tamanioInicial = 1;


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


function fondoArea() {

    let colorFondo = document.getElementById("color_fondo").value

    canvas.style.backgroundColor = colorFondo;
}

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
});

conexion.on("ReceivePosition", function (colorHub, xInicial, yInicial, xFinal, yFinal, tamanioInicialHub) {
    console.log("Dibujo recibido:", colorHub, xInicial, yInicial, xFinal, yFinal, tamanioInicialHub);
    dibujar(colorHub, xInicial, yInicial, xFinal, yFinal, tamanioInicialHub, false);

});

//insertar Texto

let textos = {}; // guardamos textos por id

function crearTextoEditable(texto, x, y, color = 'black', tamano = 20, id = null, enviar = true) {
    if (!id) id = crypto.randomUUID();


    // Si ya existe el div, solo actualiza sus propiedades
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

    // Crear un div para el texto editable
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

    // Enviar al servidor si corresponde
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

    // Drag para mover el texto
    let isDragging = false;
    let offsetX, offsetY;

    div.addEventListener("mousedown", (e) => {
        isDragging = true;
        offsetX = e.clientX - div.offsetLeft;
        offsetY = e.clientY - div.offsetTop;
        div.style.userSelect = "none";
    });


    window.addEventListener("mousemove", (e) => {
        if (!isDragging) return;
        let nx = e.clientX - offsetX;
        let ny = e.clientY - offsetY;
        div.style.left = nx + "px";
        div.style.top = ny + "px";
        textos[id].x = nx;
        textos[id].y = ny;

        if (conexion.state === signalR.HubConnectionState.Connected) {
            conexion.invoke("MoverTexto", pizarraId, id, nx, ny)
                .catch(e => console.error(e));
        }
    });


    window.addEventListener("mouseup", (e) => {
        if (isDragging) {
            isDragging = false;
            div.style.userSelect = "text";

            // Enviar posición nueva al servidor
            if (conexion.state === signalR.HubConnectionState.Connected) {
                conexion.invoke("MoverTexto", pizarraId, id, textos[id].x, textos[id].y)
                    .catch(e => console.error(e));
            }
        }
    });

    // Escuchar cuando el texto cambia para enviar edición
    div.addEventListener("input", () => {
        textos[id].texto = div.textContent;

        if (conexion.state === signalR.HubConnectionState.Connected) {
            conexion.invoke("CrearOEditarTexto",pizarraId ,{
                id: id,
                contenido: textos[id].texto,
                x: textos[id].x,
                y: textos[id].y,
                color: textos[id].color,
                tamano: textos[id].tamano
            }).catch(e => console.error(e));
        }
    });
}

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
    crearTextoEditable(texto, 100, 100, color, tamano, null, true);
});

// empieza la conexión
conexion.start().then(() => {
    console.log("conectado correctamente");
}).catch(function (err) {
    return console.error("Error al iniciar:", err.toString());
});






