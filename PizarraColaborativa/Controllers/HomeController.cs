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

        public IActionResult Index(int? idFiltrarPorRol, string busqueda)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idUsuario = _userManager.GetUserId(User);

                var pizarras = _service.ObtenerPizarrasFiltradas(idUsuario, idFiltrarPorRol, busqueda);

                ViewBag.Busqueda = busqueda;
                ViewBag.FiltroRol = idFiltrarPorRol;

                if (pizarras.Count == 0)
                {   
                    ViewBag.Mensaje = "No se han encontrado pizarras disponibles.";
                }

                return View(pizarras);
            }

            return RedirectToAction("Login", "Cuenta");
        }

        [HttpPost]
        public async Task<IActionResult> EliminarPizarra(Guid id)
        {
            var idUsuario = _userManager.GetUserId(User);

            var esAdmin = await _service.EsAdminDeLaPizarra(idUsuario, id);
            if (!esAdmin) return Forbid();

            await _service.EliminarPizarra(id);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}