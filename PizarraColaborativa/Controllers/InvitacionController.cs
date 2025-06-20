using DTO;
using Entidades.EF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services;

namespace PizarraColaborativa.Controllers
{
    public class InvitacionController : Controller
    {
        private readonly IInvitacionService _invitacionService;
        private readonly IPizarraService _pizarraService;

        private readonly UserManager<IdentityUser> _userManager;

        public InvitacionController(UserManager<IdentityUser> userManager,
             IInvitacionService invitacionService, IPizarraService pizarraService)
        {
            _invitacionService = invitacionService;
            _pizarraService = pizarraService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> GenerarInvitacion(Guid pizarraId, int rolId)
        {
            var usuario = await _userManager.GetUserAsync(User);
    
            var esAdmin = await _pizarraService.EsAdminDeLaPizarraAsync(usuario.Id, pizarraId);
            if (!esAdmin) return Forbid();

            try
            {
                var codigo = await _invitacionService.CrearInvitacionAsync(pizarraId, usuario.Id, rolId);

                var url = Url.Action("VerInvitacion", "Invitacion", new { codigo }, Request.Scheme);
                return Content(url);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> VerInvitacion(string codigo)
        {
            var invitacion = await _invitacionService.ObtenerInvitacionPorCodigo(codigo);

            if (invitacion == null || (invitacion.FechaExpiracion.HasValue && invitacion.FechaExpiracion < DateTime.UtcNow))
            {
                return View("InvitacionExpirada");
            }

            var perteneceAPizarra = await _pizarraService.ExisteUsuarioEnPizarra(_userManager.GetUserId(User), invitacion.PizarraId);
            if (perteneceAPizarra)
            {
                return RedirectToAction("Dibujar", "Pizarra", new { id = invitacion.PizarraId });
            }
            else
            {
                var pizarra = await _pizarraService.ObtenerPizarra(invitacion.PizarraId);

                ViewBag.NombrePizarra = pizarra?.NombrePizarra ?? "Pizarra Sin Nombre";
                ViewBag.Rol = invitacion.Rol.Nombre;

                return View("AceptarInvitacion", invitacion);
            } 
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AceptarInvitacion(string codigo)
        {
            var invitacion = await _invitacionService.ObtenerInvitacionPorCodigo(codigo);

            if (invitacion == null || (invitacion.FechaExpiracion.HasValue && invitacion.FechaExpiracion < DateTime.UtcNow))
            {
                return NotFound("Invitación no válida o expirada.");
            }
            var usuarioInvitado= await _userManager.GetUserAsync(User);

            var yaExiste = await _pizarraService.ExisteUsuarioEnPizarra(usuarioInvitado.Id,invitacion.PizarraId);

            if (!yaExiste)
            {
                var pizarraUsuario = new PizarraUsuario
                {
                    PizarraId = invitacion.PizarraId,
                    UsuarioId = usuarioInvitado.Id,
                    RolId= invitacion.RolId
                    
                };
               await _pizarraService.AgregarOActualizarUsuarioEnPizarraAsync(pizarraUsuario);
            }
            return RedirectToAction("Dibujar", "Pizarra", new { id = invitacion.PizarraId });

        }
    }
}
