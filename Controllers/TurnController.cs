using Microsoft.AspNetCore.Mvc;
using caso_de_uso_6_ejercer_turno.Services;
using caso_de_uso_6_ejercer_turno.Models.Domain;

namespace caso_de_uso_6_ejercer_turno.Controllers
{
    public class TurnController : Controller
    {
        private readonly TurnManager _turnManager;
        private readonly IEventBus _bus;
        private readonly LobbyService _lobbyService;

        public TurnController(TurnManager turnManager, IEventBus bus, LobbyService lobbyService)
        {
            _turnManager = turnManager;
            _bus = bus;
            _lobbyService = lobbyService;
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
        public IActionResult SalaEspera(string lobby)
        {
            var username = HttpContext.Session.GetString("username");

            if (string.IsNullOrWhiteSpace(username))
            {
                ViewBag.Error = "Sala no encontrada";
                ViewBag.LobbyId = null;
                ViewBag.Username = null;
                return View();
            }

            if (string.IsNullOrEmpty(lobby))
            {
                // Crear un nuevo lobby y redirigir al mismo con el id generado
                var id = _lobbyService.CreateLobby(username);
                return RedirectToAction("SalaEspera", new { lobby = id });
            }
            else
            {
                // Validar existencia del lobby solicitado
                if (!_lobbyService.TryGetLobby(lobby, out var state))
                {
                    TempData["ErrorMessage"] = "El identificador de sala no coincide o la sala no existe. Permaneces en la pantalla de selección.";
                    return RedirectToAction("GameLobby");
                }

                // If lobby exists, set owner name if owner is not set and this user created it earlier
                if (state.OwnerName == null && state.Jugadores.Count == 0)
                {
                    state.OwnerName = username;
                }

                ViewBag.LobbyId = state.IdPartida;
                ViewBag.Username = username;
                return View();
            }
        }

        // Nueva acción para mostrar la pantalla intermedia de selección (crear/unirse)
        public IActionResult GameLobby()
        {
            var username = HttpContext.Session.GetString("username") ?? "Invitado";
            ViewBag.Username = username;
            return View();
        }

        // ---------------- LOBBY API ----------------
        [HttpGet]
        public IActionResult LobbyPlayers(string lobby)
        {
            if (string.IsNullOrEmpty(lobby)) return BadRequest();
            var players = _lobbyService.GetLobbyPlayers(lobby).Select(p => new {
                id = p.IdJugador,
                name = p.Nombre,
                color = p.ColorFichas
            });
            return Json(players);
        }

        [HttpPost]
        public IActionResult LobbyAdd([FromForm] string lobby, [FromForm] string name, [FromForm] string color)
        {
            if (string.IsNullOrEmpty(lobby)) return BadRequest();
            var p = _lobbyService.AddPlayer(lobby, name, color);
            if (p == null) return BadRequest(new { ok = false });
            return Json(new { ok = true, id = p.IdJugador });
        }

        [HttpPost]
        public IActionResult LobbyRemove([FromForm] string lobby, [FromForm] string id)
        {
            if (string.IsNullOrEmpty(lobby)) return BadRequest();
            var ok = _lobbyService.RemovePlayer(lobby, id);
            return Json(new { ok });
        }

        [HttpPost]
        public IActionResult LobbyStart([FromForm] string lobby)
        {
            if (string.IsNullOrEmpty(lobby)) return BadRequest();
            if (!_lobbyService.CanStart(lobby)) return BadRequest(new { error = "No hay suficientes jugadores" });

            var gs = _lobbyService.StartGame(lobby);
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
