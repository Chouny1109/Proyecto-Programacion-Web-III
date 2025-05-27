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
        public async Task<IActionResult> GenerarInvitacion(Guid pizarraId)
        {
            var usuario = await _userManager.GetUserAsync(User);

            var codigo= Guid.NewGuid().ToString("N");

            var invitacion = new InvitacionPizarra
            {
                PizarraId = pizarraId,
                UsuarioRemitenteId = usuario.Id,
                CodigoInvitacion = codigo,
                FechaInvitacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddDays(1)
            };

            _invitacionService.AgregarInvitacion(invitacion);

            var url = Url.Action("AceptarInvitacion", "Invitacion", 
                new { codigo = codigo }, Request.Scheme);

            return Json(new { link = url });
        }
        [HttpGet]
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
                    Rol= invitacion.Rol
                    
                };
               await _pizarraService.AgregarUsuarioALaPizarra(pizarraUsuario);
            }
            return RedirectToAction("Dibujar", "Pizarra", new { id = invitacion.PizarraId });

        }
    }
}
