let photoDiv = document.querySelector("#photo");
let photoUploadInput = document.querySelector("#photo-upload");

function addSticky(imageElement, idImg) {
    let stickyDiv = document.createElement("div");
    stickyDiv.classList.add("sticky");
    stickyDiv.dataset.idImagen = idImg;
    stickyDiv.innerHTML = `<div class="sticky-header">
        <div class="minimize"></div>
        <div class="close"></div>
    </div>`;

    let minimize = stickyDiv.querySelector(".minimize");
    let close = stickyDiv.querySelector(".close");
    let stickyHeader = stickyDiv.querySelector(".sticky-header");
    let stickyContent;

    if (imageElement) {
        let stickyImageDiv = document.createElement("div");
        stickyImageDiv.classList.add("sticky-image-div");
        stickyDiv.append(stickyImageDiv);
        stickyImageDiv.append(imageElement);
        stickyContent = stickyImageDiv;
    } else {
        let stickyContentDiv = document.createElement("div");
        stickyContentDiv.classList.add("sticky-content");
        stickyContentDiv.setAttribute("contenteditable", "true");
        stickyContentDiv.setAttribute("spellcheck", "false");
        stickyDiv.append(stickyContentDiv);
        stickyContent = stickyContentDiv;
    }

    minimize.addEventListener("click", () => {
        stickyContent.style.display = stickyContent.style.display === "none" ? "block" : "none";
    });

    close.addEventListener("click", () => {
        conexion.invoke("CerrarImagen", pizarraId, idImg)
            .catch(err => console.error(err.toString()));
        stickyDiv.remove();
    });
      
    let isDragging = false;
    let offsetX = 0;
    let offsetY = 0;

    stickyHeader.addEventListener("mousedown", (e) => {
        isDragging = true;

  
        const rect = stickyDiv.getBoundingClientRect();
        offsetX = e.clientX - rect.left;
        offsetY = e.clientY - rect.top;

        document.body.style.userSelect = "none";
    });

    document.addEventListener("mousemove", (e) => {
        if (!isDragging) return;

        const canvas = document.querySelector(".canvas");
        const canvasRect = canvas.getBoundingClientRect();
        const stickyRect = stickyDiv.getBoundingClientRect();

     
        let newLeft = e.clientX - offsetX;
        let newTop = e.clientY - offsetY;

        if (newLeft < canvasRect.left) {
            newLeft = canvasRect.left;
        }
        if (newLeft + stickyRect.width > canvasRect.right) {
            newLeft = canvasRect.right - stickyRect.width;
        }

        if (newTop < canvasRect.top) {
            newTop = canvasRect.top;
        }
        if (newTop + stickyRect.height > canvasRect.bottom) {
            newTop = canvasRect.bottom - stickyRect.height;
        }


        stickyDiv.style.left = (newLeft - canvasRect.left) + "px";
        stickyDiv.style.top = (newTop - canvasRect.top) + "px";

       
    });


    document.addEventListener("mouseup", () => {

        isDragging = false;
        document.body.style.userSelect = "";

        const left = parseInt(stickyDiv.style.left);
        const top = parseInt(stickyDiv.style.top);



        conexion.invoke("MoverImagen", pizarraId, idImg, left, top)
            .catch(err => console.error(err.toString()));
   
    });

    document.querySelector(".canvas").append(stickyDiv);
}
