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

        // Nueva acción para mostrar la sala de espera entre login y tablero
        public IActionResult SalaEspera()
        {
            return View();
        }

        // ---------------- LOBBY API ----------------
        [HttpGet]
        public IActionResult LobbyPlayers()
        {
            var players = _turnManager.GetLobbyPlayers().Select(p => new {
                id = p.IdJugador,
                name = p.Nombre,
                color = p.ColorFichas
            });
            return Json(players);
        }

        [HttpPost]
        public IActionResult LobbyAdd([FromForm] string name, [FromForm] string color)
        {
            var p = _turnManager.AddPlayer(name, color);
            return Json(new { ok = true, id = p.IdJugador });
        }

        [HttpPost]
        public IActionResult LobbyRemove([FromForm] string id)
        {
            var ok = _turnManager.RemovePlayer(id);
            return Json(new { ok });
        }

        [HttpPost]
        public IActionResult LobbyStart()
        {
            if (!_turnManager.CanStart()) return BadRequest(new { error = "No hay suficientes jugadores" });

            var gs = _turnManager.StartGameAndShuffleOrder();
            return Json(new { ok = true, game = gs });
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

            await socket.EnviarAsync(new
            {
                type = "roll",
                player = jugador.IdJugador
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

            await socket.EnviarAsync(new
            {
                type = "move",
                player = jugador.IdJugador,
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
