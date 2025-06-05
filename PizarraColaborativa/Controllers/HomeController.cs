using System.Diagnostics;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizarraColaborativa.DTO;
using PizarraColaborativa.Models;
using Services;

namespace PizarraColaborativa.Controllers
{
    [Authorize(Roles = "Usuario")]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPizarraService _service;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IPizarraService service, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _service = service;
            _userManager = userManager; 
        }

        public IActionResult Index(int? idFiltrarPorRol)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idUsuario = _userManager.GetUserId(User);
                var pizarras = _service.ObtenerPizarrasDelUsuario(idUsuario);

               
                if (!pizarras.Any())
                {
                    ViewBag.PizarraMensaje = "No se han encontrado pizarras";
                }

                if (idFiltrarPorRol.HasValue)
                {
                    var rolFiltrado = (RolEnPizarra)idFiltrarPorRol;
                    if(rolFiltrado == RolEnPizarra.Admin)
                    {
                        pizarras = pizarras.Where(p => p.Rol == RolEnPizarra.Admin).ToList();
                    }
                    else
                    {
                        pizarras = pizarras.Where(p => p.Rol != RolEnPizarra.Admin).ToList();
                    }
                  
                }

                return View(pizarras);
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
