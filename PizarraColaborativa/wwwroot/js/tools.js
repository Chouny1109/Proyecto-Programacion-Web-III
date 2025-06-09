let undo = document.querySelector("#undo");
let redo = document.querySelector("#redo");
undo.addEventListener("click", deshacerLinea);
redo.addEventListener("click", ponerLinea);

function deshacerLinea() {
    if (linesDB.length) {
        let undoLine = linesDB.pop();
        redoLinesDB.push(undoLine);

      
        papel.clearRect(0, 0, canvas.width, canvas.height);

        drawLinesFromDB();
    }
}
function ponerLinea() {
    if (redoLinesDB.length) {

        let currentLineWidth = papel.lineWidth;
        let currentStrokeStyle = papel.strokeStyle;

        let redoLine = redoLinesDB.pop();
        for (let i = 0; i < redoLine.length; i++) {
            let pointObject = redoLine[i];
            if (pointObject.type == "md") {
                papel.lineWidth = pointObject.lineWidth;
                papel.strokeStyle = pointObject.strokeStyle;
                papel.beginPath();
                papel.moveTo(pointObject.x, pointObject.y);
            } else {
                papel.lineTo(pointObject.x, pointObject.y);
                papel.stroke();
            }
        }
        linesDB.push(redoLine);

        papel.lineWidth = currentLineWidth;
        papel.strokeStyle = currentStrokeStyle;

        console.log("Tamanio en redo:" + papel.lineWidth)
    }
}
function drawLinesFromDB() {
    let currentLineWidth = papel.lineWidth;
    let currentStrokeStyle = papel.strokeStyle;

    for (let i = 0; i < linesDB.length; i++) {
        let line = linesDB[i];
        for (let i = 0; i < line.length; i++) {
            let pointObject = line[i];
            if (pointObject.type == "md") {
                papel.lineWidth = pointObject.lineWidth;
                papel.strokeStyle = pointObject.strokeStyle;
                papel.beginPath();
                papel.moveTo(pointObject.x, pointObject.y);
            } else {
                papel.lineTo(pointObject.x, pointObject.y);
                papel.stroke();
            }
        }
    }

    papel.lineWidth = currentLineWidth;
    papel.strokeStyle = currentStrokeStyle;

    console.log("Tamanio en undo:" + papel.lineWidth)
}