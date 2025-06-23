using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace PizarraColaborativa.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class PizarraController(IPizarraService service, IPizarraUsuarioService puService,
        IMensajeService msgService, INotificacionService notifService,
        UserManager<IdentityUser> userManager) : Controller
    {
        private readonly IPizarraService _service = service;
        private readonly IPizarraUsuarioService _puService = puService;
        private readonly INotificacionService _notifService = notifService;
        private readonly IMensajeService _msgService = msgService;
        private readonly UserManager<IdentityUser> _userManager = userManager;

        [HttpPost]
        public async Task<IActionResult> CrearPizarra(string nombre)
        {
            var pizarraId = await _service.CrearPizarraAsync(_userManager.GetUserId(User), nombre);

            return RedirectToAction("Dibujar", new { id = pizarraId });
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPizarra(Guid pizarraId)
        {
            await _service.EliminarPizarraAsync(pizarraId);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Dibujar(Guid id)
        {
            var userId = _userManager.GetUserId(User);
            ViewData["UserName"] = _userManager.GetUserName(User);

            var pizarra = await _service.ObtenerPizarra(id);
            if (pizarra == null) return NotFound("La pizarra no existe");

            var pertenece = await _puService.ExisteRelacionPizarraUsuarioAsync(userId, id);
            if (!pertenece)
            {
                TempData["Error"] = "No tiene permiso para acceder a esta pizarra.";
                return RedirectToAction("Index");
            }

            var rolUsuario = await _service.ObtenerRolDeUsuarioEnPizarraAsync(userId, id);
            ViewData["RolUsuario"] = rolUsuario;

            var esAdmin = await _service.EsAdminDeLaPizarraAsync(userId, id);
            ViewData["EsAdmin"] = esAdmin;

            ViewData["PizarraId"] = id;
            ViewData["PizarraNombre"] = pizarra.NombrePizarra;

            var contadorNotificaciones = await _notifService.ContarNotificacionesNoVistasAsync(userId);
            ViewData["ContadorNotificaciones"] = contadorNotificaciones;

            var contadorMensajes = await _msgService.CantidadMensajesNoVistosAsync(userId, id);
            ViewData["ContadorMensajes"] = contadorMensajes;

            return View();
        }
    }
}
