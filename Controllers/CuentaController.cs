using Microsoft.AspNetCore.Mvc;
using caso_de_uso_6_ejercer_turno.Models;
using System.Threading.Tasks;

namespace caso_de_uso_6_ejercer_turno.Controllers
{
    public class CuentaController : Controller
    {
        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Aquí iría la lógica de autenticación con tu sistema
                // Ejemplo: validar contra base de datos, Identity, etc.
                
                // Simulación de login exitoso
                // if (await _authService.ValidateUser(model.Username, model.Password))
                // {
                //     return RedirectToAction("Index", "Game");
                // }
                
                TempData["SuccessMessage"] = "¡Bienvenido al juego!";
                return RedirectToAction("Index", "Game");
            }

            TempData["ErrorMessage"] = "Usuario o contraseña incorrectos";
            return View(model);
        }

        // GET: Account/Register
        public IActionResult Registrar()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Aquí iría la lógica de registro
                // Ejemplo: crear usuario en base de datos
                
                // if (await _authService.CreateUser(model))
                // {
                //     TempData["SuccessMessage"] = "¡Registro exitoso! Ya puedes jugar";
                //     return RedirectToAction("Login");
                // }
                
                TempData["SuccessMessage"] = "¡Registro exitoso!";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // GET: Account/Logout
        public IActionResult Logout()
        {
            // Aquí iría la lógica de cierre de sesión
            // await HttpContext.SignOutAsync();
            
            TempData["InfoMessage"] = "Has cerrado sesión correctamente";
            return RedirectToAction("Login");
        }

        // GET: Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }
    }
}