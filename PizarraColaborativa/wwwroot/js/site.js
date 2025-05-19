let conexion = new signalR.HubConnectionBuilder()
    .withUrl("/dibujohub").build();

let canvas = document.getElementById("area");
let papel = canvas.getContext('2d');

function dibujar(color1, corX, corY, corXFinal, corYFinal, tamanioLinea, enviar = true) {
        papel.beginPath();
        papel.strokeStyle = color1;
        papel.lineWidth = tamanioLinea
        papel.moveTo(corX, corY);
        papel.lineTo(corXFinal, corYFinal);
        papel.stroke();
        papel.closePath();


        if (enviar && conexion.state === signalR.HubConnectionState.Connected) {
            conexion.invoke("SendDibujo", color, corX, corY, corXFinal, corYFinal, tamanioInicial)
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

        conexion.invoke("SendLimpiar")
            .catch(err => console.error(err.toString()));
    
}

// Escuchar evento de limpieza que viene del servidor... no anda xD
conexion.on("ReceiveLimpiar", function () {
    papel.clearRect(0, 0, canvas.width, canvas.height);
});

conexion.on("ReceivePosition", function (colorHub, xInicial, yInicial, xFinal, yFinal, tamanioInicialHub) {
    console.log("Dibujo recibido:", colorHub, xInicial, yInicial, xFinal, yFinal, tamanioInicialHub);
    dibujar(colorHub, xInicial, yInicial, xFinal, yFinal, tamanioInicialHub, false);

});



// empieza la conexión
conexion.start().then(() => {
    console.log("conectado correctamente");
}).catch(function (err) {
    return console.error("Error al iniciar:", err.toString());
});






