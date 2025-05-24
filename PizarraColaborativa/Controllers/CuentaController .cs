using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PizarraColaborativa.Models;

namespace PizarraColaborativa.Controllers
{
    public class CuentaController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public CuentaController(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(RegistroUsuarioViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Crear rol si no existe
            if (!await _roleManager.RoleExistsAsync(model.Rol))
                await _roleManager.CreateAsync(new IdentityRole(model.Rol));

            var usuario = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var resultado = await _userManager.CreateAsync(usuario, model.Password);
            if (resultado.Succeeded)
            {
                await _userManager.AddToRoleAsync(usuario, model.Rol);
                await _signInManager.SignInAsync(usuario, isPersistent: false);
                return RedirectToAction("Login");
            }

            foreach (var error in resultado.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _signInManager.PasswordSignInAsync(email, password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Login inválido.");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        public IActionResult AccesoDenegado()
        {
            return View("AccesoDenegado");
        }
    }
}
