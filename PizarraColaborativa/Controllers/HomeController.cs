using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizarraColaborativa.Models;
using Services;

namespace PizarraColaborativa.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class HomeController(ILogger<HomeController> logger, IPizarraService service, UserManager<IdentityUser> userManager) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly IPizarraService _service = service;
        private readonly UserManager<IdentityUser> _userManager = userManager;

        public IActionResult Index(int? filtroRol, string busqueda)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);

                ViewBag.UserId = userId;
                ViewBag.UserName = _userManager.GetUserName(User);
                ViewBag.Pizarras = _service.ObtenerPizarrasChat(userId);

                var pizarrasFiltradas = _service.ObtenerPizarrasFiltradas(userId, filtroRol, busqueda);

                ViewBag.Busqueda = busqueda;
                ViewBag.FiltroRol = filtroRol;

                if (pizarrasFiltradas.Count == 0) {
                    ViewBag.Mensaje = "No se han encontrado pizarras disponibles.";
                }

                return View(pizarrasFiltradas);
            }

            return RedirectToAction("Login", "Cuenta");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPizarra(Guid pizarraId)
        {
            var userID = _userManager.GetUserId(User);

            var esAdmin = await _service.EsAdminDeLaPizarra(userID, pizarraId);
            if (!esAdmin) return Forbid();

            await _service.EliminarPizarra(pizarraId);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}