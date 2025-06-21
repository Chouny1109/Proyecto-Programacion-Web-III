using DTO;
using Entidades.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace PizarraColaborativa.Controllers
{
    [Authorize(Roles ="Usuario")]
    public class PizarraController(IPizarraService service, UserManager<IdentityUser> userManager) : Controller
    {
        private readonly IPizarraService _service = service;
        private readonly UserManager<IdentityUser> _userManager = userManager;
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Crear(string nombre)
        {
            var creadorId = _userManager.GetUserId(User);
            var nuevaPizarra = new Pizarra
            {
                Id = Guid.NewGuid(),
                CreadorId = creadorId,
                NombrePizarra = nombre,
                FechaCreacion = DateTime.UtcNow
            };

            await _service.CrearPizarraAsync(nuevaPizarra, creadorId);

            return RedirectToAction("Dibujar", new { id = nuevaPizarra.Id });
        }

        public async Task<IActionResult> Dibujar(Guid id)
        {
            var userId = _userManager.GetUserId(User);

            var pizarra = await _service.ObtenerPizarra(id);
            if (pizarra == null) return NotFound("La pizarra no existe");

            var pertenece = await _service.ExisteUsuarioEnPizarra(userId, id);
            if (!pertenece)
            {
                TempData["Error"] = "No tiene permiso para acceder a esta pizarra.";
                return RedirectToAction("Index");
            }

            var rolUsuario = await _service.ObtenerRolUsuarioEnPizarra(userId, id); 
            ViewData["RolUsuario"] = rolUsuario;

            var esAdmin = await _service.EsAdminDeLaPizarraAsync(userId, id);
            ViewData["EsAdmin"] = esAdmin;
            
            ViewData["PizarraId"] = id;
            ViewData["PizarraNombre"] = pizarra.NombrePizarra;
            ViewData["UserId"] = userId;
            ViewData["UserName"] = _userManager.GetUserName(User);

            var mensajesNoVistos = await _service.ObtenerCantidadMensajesNoVistosAsync(userId, id);
            ViewData["MensajesNoVistos"] = mensajesNoVistos;

            return View(); 
        }
    }
}