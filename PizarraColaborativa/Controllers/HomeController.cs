using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
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

        public HomeController(ILogger<HomeController> logger, IPizarraService service)
        {
            _logger = logger;
            _service = service;
        }

        public IActionResult Index()
        {
            var idUsuario = User.Identity?.Name;
            List<PizarraResumenDTO> pizarrasID = _service.ObtenerPizarrasDelUsuario(idUsuario);
            return View(pizarrasID);
        }

      
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
