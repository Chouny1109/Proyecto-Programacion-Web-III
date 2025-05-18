let canvas = document.getElementById("area");
let papel = canvas.getContext('2d');

    function dibujar(color, corX, corY, corXFinal, corYFinal) {
        papel.beginPath();
        papel.strokeStyle = color;
        papel.lineWidth = tamanioInicial
        papel.moveTo(corX, corY);
        papel.lineTo(corXFinal, corYFinal);
        papel.stroke();
        papel.closePath();

    }


    canvas.addEventListener("mousemove", dibujarConMouse)
    function dibujarConMouse(event) {

        if (presionMouse) {
            dibujar(color, x, y, event.offsetX, event.offsetY);
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
        tamanioInicial = tamanioLapiz.value;
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
    papel.clearRect(0, 0, canvas.width, canvas.height);
}






