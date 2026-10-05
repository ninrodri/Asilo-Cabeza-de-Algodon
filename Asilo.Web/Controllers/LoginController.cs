using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Asilo.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly AsiloDbContext _context;

        public LoginController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    (u.NombreUsuario == modelo.UsuarioOCorreo ||
                     u.Correo == modelo.UsuarioOCorreo)
                    && u.PasswordHash == modelo.Password
                    && u.Estado);

            if (usuario == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos.";
                return View(modelo);
            }

            var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
    new Claim(ClaimTypes.Email, usuario.Correo),
    new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "")
};

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identidad);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Login");

        }
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}
