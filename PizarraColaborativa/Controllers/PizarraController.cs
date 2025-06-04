using DTO;
using Entidades.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services;

namespace PizarraColaborativa.Controllers
{
    [Authorize(Roles ="Usuario")]
    public class PizarraController : Controller
    {
        private readonly IPizarraService _service;
        private readonly UserManager<IdentityUser> _userManager;


        public PizarraController(IPizarraService service, UserManager<IdentityUser> userManager)
        {
            _service= service;
            _userManager= userManager;
        }

        [HttpGet]
        public IActionResult CrearPizarra()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear()
        {
            var creadorId = _userManager.GetUserId(User);
            var idPizarra = Guid.NewGuid();
            var pizarra = new Pizarra
            {
                Id = idPizarra,
                CreadorId = creadorId
            };
            var usuarioPizarra = new PizarraUsuario()
            {
                PizarraId = idPizarra,
                UsuarioId = creadorId,
                Rol = RolEnPizarra.Admin
            };
            await _service.CrearPizarra(pizarra);
            await _service.AgregarUsuarioALaPizarra(usuarioPizarra);
            return RedirectToAction("Dibujar", new { id = pizarra.Id });
        }

        public async Task<IActionResult> Dibujar(Guid id)
        {
            var pizarra = await _service.ObtenerPizarra(id);
            if (pizarra == null)
            {
                return NotFound("La pizarra no existe");
            }

            var idUsuario = _userManager.GetUserId(User);
            var existe = await _service.ExisteUsuarioEnPizarra(idUsuario, pizarra.Id);

            if (!existe)
            {
                TempData["Error"] = "No tiene permiso para acceder a esta pizarra.";
                return RedirectToAction("Index");
            }

            var esAdmin = await _service.EsAdminDeLaPizarra(idUsuario, pizarra.Id);
            ViewData["EsAdmin"] = esAdmin;
            ViewData["PizarraId"] = id;

            return View(); 
        }

    }
}
