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
        // GET: Cuenta/Login
        // ============================
        public IActionResult Login()
        {
            return View();
        }

        // ============================
        // POST: Cuenta/Login
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["SuccessMessage"] = "¡Bienvenido al juego!";
                return RedirectToAction("Index", "Game");
            }

            TempData["ErrorMessage"] = "Usuario o contraseña incorrectos";
            return View(model);
        }

        // ============================
        // GET: Cuenta/Registrar
        // ============================
        public IActionResult Registrar()
        {
            return View();
        }

        // ============================
        // POST: Cuenta/Registrar
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                await _cuentaService.RegistrarUsuarioAsync(model);

                TempData["SuccessMessage"] = "¡Registro exitoso!";
                return RedirectToAction("Login");
            }

            return View(model);
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
