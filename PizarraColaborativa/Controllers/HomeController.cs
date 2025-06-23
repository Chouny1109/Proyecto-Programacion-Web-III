using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizarraColaborativa.Models;
using Services;

namespace PizarraColaborativa.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class HomeController(IPizarraService service, INotificacionService notifService,
        UserManager<IdentityUser> userManager) : Controller
    {
        private readonly IPizarraService _service = service;
        private readonly INotificacionService _notifService = notifService;
        private readonly UserManager<IdentityUser> _userManager = userManager;

        public async Task<IActionResult> Index(string filtroRol, string busqueda)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                ViewData["UserName"] = _userManager.GetUserName(User);

                var pizarrasFiltradas = await _service.ObtenerPizarrasFiltradasAsync(_userManager.GetUserId(User), filtroRol, busqueda);
                ViewData["FiltroRol"] = filtroRol;
                ViewData["Busqueda"] = busqueda;

                var contadorNotificaciones = await _notifService.ContarNotificacionesNoVistasAsync(_userManager.GetUserId(User));
                ViewData["ContadorNotificaciones"] = contadorNotificaciones;

                var rolAdminId = await _service.ObtenerRolIdAsync("Admin");
                ViewData["RolAdminId"] = rolAdminId;
                var rolEscrituraId = await _service.ObtenerRolIdAsync("Escritura");
                ViewData["RolEscrituraId"] = rolEscrituraId;
                var rolLecturaId = await _service.ObtenerRolIdAsync("Lectura");
                ViewData["RolLecturaId"] = rolLecturaId;

                if (pizarrasFiltradas.Count == 0)
                    TempData["Mensaje"] = "No se han encontrado pizarras disponibles.";

                return View(pizarrasFiltradas);
            }

            return RedirectToAction("Login", "Cuenta");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
