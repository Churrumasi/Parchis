using caso_de_uso_6_ejercer_turno.Models;
using caso_de_uso_6_ejercer_turno.Services;
using Microsoft.AspNetCore.Mvc;

namespace caso_de_uso_6_ejercer_turno.Controllers
{
    public class CuentaController : Controller
    {
        private readonly CuentaService _cuentaService;

        public CuentaController(CuentaService cuentaService)
        {
            _cuentaService = cuentaService;
        }

        // ============================
        // GET: Login
        // ============================
        // Recibimos el returnUrl (si alguien intentó entrar a una sala sin permiso)
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // ============================
        // POST: Login
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Completa todos los campos.";
                // Importante: devolvemos el returnUrl a la vista por si falla el modelo
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var usuario = await _cuentaService.LoginAsync(model.Username, model.Password);

            if (usuario == null)
            {
                TempData["ErrorMessage"] = "Usuario o contraseña incorrectos.";
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            // Login exitoso
            TempData["SuccessMessage"] = "Bienvenido " + usuario.NombreUsuario + "!";

            // Guardar nombre en sesion
            HttpContext.Session.SetString("username", usuario.NombreUsuario);

           
            // Redireccion
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // Si no hay returnUrl, flujo normal
            return RedirectToAction("GameLobby", "Turn");
        }

        // ============================
        // GET: Registrar
        // ============================
        public IActionResult Registrar()
        {
            return View(new RegisterViewModel());
        }

        // ============================
        // POST: Registrar
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                TempData["ErrorMessage"] = string.Join(" | ", errores);
                return View(model);
            }

            bool creado = await _cuentaService.RegistrarUsuarioAsync(model);

            if (!creado)
            {
                TempData["ErrorMessage"] = "El nombre de usuario ya está en uso.";
                return View(model);
            }

            TempData["SuccessMessage"] = "Usuario creado con éxito. Ahora puedes iniciar sesión.";

            return RedirectToAction("Login");
        }
    }
}