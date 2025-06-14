using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizarraColaborativa.Models;
using Services;

namespace PizarraColaborativa.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class HomeController(
        ILogger<HomeController> logger,
        IPizarraService service,
        UserManager<IdentityUser> userManager) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly IPizarraService _service = service;
        private readonly UserManager<IdentityUser> _userManager = userManager;

        public async Task<IActionResult> Index(string? filtroRol, string busqueda)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);

                var pizarrasFiltradas = await _service.ObtenerPizarrasFiltradasAsync(userId, filtroRol, busqueda);

                ViewData["UserName"] = _userManager.GetUserName(User);
                ViewData["Busqueda"] = busqueda;
                ViewData["FiltroRol"] = filtroRol;

                if (pizarrasFiltradas.Count == 0)
                    TempData["Mensaje"] = "No se han encontrado pizarras disponibles.";

                return View(pizarrasFiltradas);
            }

            return RedirectToAction("Login", "Cuenta");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPizarra(Guid pizarraId)
        {
            var userId = _userManager.GetUserId(User);

            if (!await _service.EsAdminDeLaPizarraAsync(userId, pizarraId)) return Forbid();

            await _service.EliminarPizarraAsync(pizarraId);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}