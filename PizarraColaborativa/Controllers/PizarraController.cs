using Entidades.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;

namespace PizarraColaborativa.Controllers
{
    public class PizarraController : Controller
    {
        private readonly IPizarraService _service;

        public PizarraController(IPizarraService service)
        {
            _service= service;
        }

        [HttpGet]
        public IActionResult CrearPizarra()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear()
        {
            var pizarra = new Pizarra
            {
                Id = Guid.NewGuid(),
                CreadorId = User.Identity?.Name ?? "Invitado"
            };

            await _service.CrearPizarra(pizarra);
            return RedirectToAction("Dibujar", new { id = pizarra.Id });
        }
        public async Task<IActionResult> Dibujar(Guid id)
        {
            var pizarra = await _service.ObtenerPizarra(id);
            if (pizarra == null)
            {
                return NotFound("La pizarra no existe");
            }

            ViewData["PizarraId"] = id;
            return View(); 
        }

    }
}
