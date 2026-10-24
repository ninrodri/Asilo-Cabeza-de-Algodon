using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly AsiloDbContext _context;

        public UsuariosController(AsiloDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // LISTADO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(string? buscar)
        {
            var consulta = _context.Usuarios
                .Include(u => u.Rol)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                consulta = consulta.Where(u =>
                    u.NombreUsuario.Contains(buscar) ||
                    u.Correo.Contains(buscar) ||
                    (u.Rol != null &&
                     u.Rol.Nombre.Contains(buscar)) ||
                    (u.Especialidad != null &&
                     u.Especialidad.Contains(buscar))
                );
            }

            ViewBag.Buscar = buscar;

            var usuarios = await consulta
                .OrderByDescending(u => u.Estado)
                .ThenBy(u => u.NombreUsuario)
                .ToListAsync();

            ViewBag.TotalUsuarios = usuarios.Count;

            return View(usuarios);
        }

        // =========================================================
        // REGISTRAR - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            await CargarRoles();

            return View(new Usuario
            {
                Estado = true
            });
        }


        // =========================================================
        // REGISTRAR - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Usuario usuario)
        {
            var nombreExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.NombreUsuario == usuario.NombreUsuario);

            if (nombreExiste)
            {
                ModelState.AddModelError(
                    nameof(usuario.NombreUsuario),
                    "Ya existe un usuario con este nombre."
                );
            }


            var correoExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.Correo == usuario.Correo);

            if (correoExiste)
            {
                ModelState.AddModelError(
                    nameof(usuario.Correo),
                    "Ya existe un usuario con este correo."
                );
            }


            var rol = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.Id == usuario.RolId);

            if (rol == null)
            {
                ModelState.AddModelError(
                    nameof(usuario.RolId),
                    "Debe seleccionar un rol válido."
                );
            }


            if (rol != null &&
                rol.Nombre == "Médico especialista" &&
                string.IsNullOrWhiteSpace(usuario.Especialidad))
            {
                ModelState.AddModelError(
                    nameof(usuario.Especialidad),
                    "Debe seleccionar una especialidad."
                );
            }


            if (rol != null &&
                rol.Nombre != "Médico especialista")
            {
                usuario.Especialidad = null;
            }


            if (!ModelState.IsValid)
            {
                await CargarRoles();

                return View(usuario);
            }


            usuario.Estado = true;

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();


            TempData["Exito"] =
                "El usuario fue registrado correctamente.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // EDITAR - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u =>
                    u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }


            /*
             * Importante:
             * No enviamos la contraseña actual al formulario.
             *
             * Si se deja vacío:
             * se conserva la contraseña existente.
             */
            usuario.PasswordHash = "";


            await CargarRoles();

            return View(usuario);
        }


        // =========================================================
        // EDITAR - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Usuario usuario)
        {
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Id == usuario.Id);

            if (usuarioExistente == null)
            {
                return NotFound();
            }


            /*
             * PasswordHash tiene [Required] en el modelo.
             *
             * Durante la edición permitimos dejarlo vacío
             * para conservar la contraseña anterior.
             */
            ModelState.Remove(
                nameof(usuario.PasswordHash)
            );


            // -----------------------------------------------------
            // VALIDAR NOMBRE DE USUARIO
            // -----------------------------------------------------

            var nombreExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.NombreUsuario == usuario.NombreUsuario &&
                    u.Id != usuario.Id);

            if (nombreExiste)
            {
                ModelState.AddModelError(
                    nameof(usuario.NombreUsuario),
                    "Ya existe otro usuario con este nombre."
                );
            }


            // -----------------------------------------------------
            // VALIDAR CORREO
            // -----------------------------------------------------

            var correoExiste = await _context.Usuarios
                .AnyAsync(u =>
                    u.Correo == usuario.Correo &&
                    u.Id != usuario.Id);

            if (correoExiste)
            {
                ModelState.AddModelError(
                    nameof(usuario.Correo),
                    "Ya existe otro usuario con este correo."
                );
            }


            // -----------------------------------------------------
            // VALIDAR ROL
            // -----------------------------------------------------

            var rol = await _context.Roles
                .FirstOrDefaultAsync(r =>
                    r.Id == usuario.RolId);

            if (rol == null)
            {
                ModelState.AddModelError(
                    nameof(usuario.RolId),
                    "Debe seleccionar un rol válido."
                );
            }


            // -----------------------------------------------------
            // VALIDAR ESPECIALIDAD
            // -----------------------------------------------------

            if (rol != null &&
                rol.Nombre == "Médico especialista" &&
                string.IsNullOrWhiteSpace(usuario.Especialidad))
            {
                ModelState.AddModelError(
                    nameof(usuario.Especialidad),
                    "Debe seleccionar una especialidad."
                );
            }


            if (!ModelState.IsValid)
            {
                await CargarRoles();

                return View(usuario);
            }


            // -----------------------------------------------------
            // ACTUALIZAR DATOS
            // -----------------------------------------------------

            usuarioExistente.NombreUsuario =
                usuario.NombreUsuario;

            usuarioExistente.Correo =
                usuario.Correo;

            usuarioExistente.RolId =
                usuario.RolId;


            if (rol!.Nombre == "Médico especialista")
            {
                usuarioExistente.Especialidad =
                    usuario.Especialidad;
            }
            else
            {
                usuarioExistente.Especialidad =
                    null;
            }


            /*
             * Si se escribió una nueva contraseña,
             * la actualizamos.
             *
             * Si quedó vacía,
             * conservamos la anterior.
             */
            if (!string.IsNullOrWhiteSpace(
                usuario.PasswordHash))
            {
                usuarioExistente.PasswordHash =
                    usuario.PasswordHash;
            }


            await _context.SaveChangesAsync();


            TempData["Exito"] =
                "El usuario fue actualizado correctamente.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // DESACTIVAR
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }


            // Evitar que el administrador se desactive a sí mismo
            var idActualTexto =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );

            if (int.TryParse(
                    idActualTexto,
                    out int idActual) &&
                usuario.Id == idActual)
            {
                TempData["Advertencia"] =
                    "No puede desactivar su propia cuenta mientras tiene la sesión iniciada.";

                return RedirectToAction(nameof(Index));
            }


            usuario.Estado = false;

            await _context.SaveChangesAsync();


            TempData["Exito"] =
                "El usuario fue desactivado. Su historial se conserva.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // REACTIVAR
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }


            usuario.Estado = true;

            await _context.SaveChangesAsync();


            TempData["Exito"] =
                "El usuario fue reactivado correctamente.";


            return RedirectToAction(nameof(Index));
        }


        // =========================================================
        // CARGAR ROLES
        // =========================================================

        private async Task CargarRoles()
        {
            ViewBag.Roles = await _context.Roles
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }
    }
}