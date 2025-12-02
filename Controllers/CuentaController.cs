using caso_de_uso_6_ejercer_turno.Models;
using caso_de_uso_6_ejercer_turno.Services;
using Microsoft.AspNetCore.Mvc;

namespace caso_de_uso_6_ejercer_turno.Controllers
{
    public class CuentaController : Controller
    {
        private readonly CuentaService _cuentaService;
        private readonly TurnManager _turnManager;
        public CuentaController(CuentaService cuentaService, TurnManager turnManager)
        {
            _cuentaService = cuentaService;
            _turnManager = turnManager;
        }


        // ============================
        // GET: Cuenta/Login
        // ============================
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // ============================
        // POST: Cuenta/Login
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            [FromServices] SocketGameService socketService)
        {
            string username = string.IsNullOrWhiteSpace(model.Username)
                ? "UsuarioPrueba"
                : model.Username;

            // 1. Conectar al servidor
            bool conectado = await socketService.ConectarAsync("127.0.0.1", 9000);

            if (!conectado)
            {
                TempData["ErrorMessage"] = "No se pudo conectar al servidor de Parchís.";
                return View(model);
            }

            // 2. Enviar usuario al servidor
            await socketService.EnviarAsync(new
            {
                type = "register",
                username = username
            });

            // 3. Recibir asignación
            string respuesta = await socketService.RecibirAsync();

            var asignacion = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerAssignResponse>(respuesta);

            if (asignacion == null)
            {
                TempData["ErrorMessage"] = "Error al recibir la respuesta del servidor.";
                return View(model);
            }

            // 4. Guardar en sesión
            HttpContext.Session.SetString("Username", username);
            HttpContext.Session.SetInt32("PlayerNumber", asignacion.player);
            HttpContext.Session.SetString("PlayerColor", asignacion.color);

            // 5. SINCRONIZAR con el TurnManager local
            // Asignamos el playerNumber y color al jugador local que corresponda
            _turnManager.AsignarPlayerNumberALocal(username, asignacion.player, asignacion.color);

            TempData["SuccessMessage"] = $"Bienvenido {username}! Eres el jugador {asignacion.player} ({asignacion.color}).";

            return RedirectToAction("EsTuTurno", "Turn");
        }

        public class PlayerAssignResponse
        {
            public string type { get; set; }
            public int player { get; set; }
            public string color { get; set; }
        }


        // ============================
        // GET: Cuenta/Registrar
        // ============================
        public IActionResult Registrar()
        {
            return View(new RegisterViewModel());
        }

        // ============================
        // POST: Cuenta/Registrar
        // ============================
       [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Registrar(RegisterViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    bool registrado = await _cuentaService.RegistrarUsuarioAsync(model);

    if (!registrado)
    {
        TempData["ErrorMessage"] = "El nombre de usuario ya está registrado.";
        return View(model);
    }

    TempData["SuccessMessage"] = "¡Registro exitoso! Ahora inicia sesión.";
    return RedirectToAction("Login");
}

        // ============================
        // GET: Cuenta/Logout
        // ============================
        public IActionResult Logout()
        {
            TempData["InfoMessage"] = "Has cerrado sesión correctamente";
            return RedirectToAction("Login");
        }

        // ============================
        // GET: Cuenta/ForgotPassword
        // ============================
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // ============================
        // POST: Cuenta/ForgotPassword
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ContraseñaOlvidadaVM model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Si el correo existe, recibirás un email con instrucciones";
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View(model);
        }

        // ============================
        // GET: Cuenta/ForgotPasswordConfirmation
        // ============================
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // ============================
        // GET: Cuenta/ResetPassword
        // ============================
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        // ============================
        // POST: Cuenta/ResetPassword
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "Tu contraseña ha sido restablecida correctamente";
                return RedirectToAction("Login");
            }

            return View(model);
        }
    }
}
