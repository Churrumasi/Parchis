using Microsoft.AspNetCore.Mvc;
using caso_de_uso_6_ejercer_turno.Services;
using caso_de_uso_6_ejercer_turno.Models.Domain;

namespace caso_de_uso_6_ejercer_turno.Controllers
{
    public class TurnController : Controller
    {
        private readonly TurnManager _turnManager;
        private readonly IEventBus _bus;

        public TurnController(TurnManager turnManager, IEventBus bus)
        {
            _turnManager = turnManager;
            _bus = bus;
        }

        public async Task<IActionResult> EsTuTurno(
    [FromServices] SocketGameService socket)
        {
            if (!socket.Conectado)
            {
                await socket.ConectarAsync("127.0.0.1", 9000);
            }

            var jugador = _turnManager.GetJugadorActual();
            return View(model: jugador);
        }

        public IActionResult TirarDado()
        {
            var jugador = _turnManager.GetJugadorActual();
            return View(model: jugador);
        }

        public IActionResult Inactividad()
        {
            var jugador = _turnManager.GetJugadorActual();

            return View(model: jugador);
        }

        public IActionResult SeleccionFicha(int valor)
        {
            ViewBag.ValorDado = valor;
            var jugador = _turnManager.GetJugadorActual();
            return View(model: jugador);
        }

        public IActionResult FinTurno()
        {
            var jugador = _turnManager.GetJugadorActual();
            return View(model: jugador);
        }

        [HttpPost]
        public async Task<IActionResult> LanzarDadoAjax(
    [FromServices] SocketGameService socket)
        {
            var jugador = _turnManager.GetJugadorActual();
            if (jugador == null) return BadRequest("No hay jugador actual.");

            // si IdJugador es string que contiene número, lo convertimos a int o enviamos tal cual
            int playerNumber;
            if (!int.TryParse(jugador.IdJugador, out playerNumber))
            {
                // fallback: si no se puede parsear, intenta recuperar desde session
                playerNumber = HttpContext.Session.GetInt32("PlayerNumber") ?? 0;
            }

            await socket.EnviarAsync(new
            {
                type = "roll",
                player = playerNumber
            });

            var respuesta = await socket.RecibirAsync();

            return Json(new { server = respuesta });
        }

        [HttpPost]
        public async Task<IActionResult> MoverFichaAjax(
    int indiceFicha, int desde, int hasta,
    [FromServices] SocketGameService socket)
        {
            var jugador = _turnManager.GetJugadorActual();
            if (jugador == null) return BadRequest();

            int playerNumber;
            if (!int.TryParse(jugador.IdJugador, out playerNumber))
            {
                playerNumber = HttpContext.Session.GetInt32("PlayerNumber") ?? 0;
            }

            await socket.EnviarAsync(new
            {
                type = "move",
                player = playerNumber,
                piece = indiceFicha,
                from = desde,
                to = hasta
            });

            var respuesta = await socket.RecibirAsync();

            return Json(new { server = respuesta });
        }


        [HttpPost]
        public IActionResult PasarTurnoAjax()
        {
            _turnManager.FinalizarTurno();
            return Json(new { ok = true });
        }

        [HttpGet]
        public IActionResult EstadoPartida()
        {
            var gs = _turnManager.GetGameState();
            return Json(gs);
        }
    }
}
