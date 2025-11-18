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

        public IActionResult EsTuTurno()
        {
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
        public IActionResult LanzarDadoAjax()
        {
            var jugador = _turnManager.GetJugadorActual();
            if (jugador == null) return BadRequest("No hay jugador actual.");
            var rnd = new Random();
            var valor = rnd.Next(1, 7);
            _turnManager.ProcesarDadoLanzado(jugador.IdJugador, valor);
            return Json(new { valor });
        }

        [HttpPost]
        public IActionResult MoverFichaAjax(int indiceFicha, int desde, int hasta)
        {
            var jugador = _turnManager.GetJugadorActual();
            if (jugador == null) return BadRequest();
            _turnManager.ProcesarMovimiento(jugador.IdJugador, indiceFicha, desde, hasta);
            return Json(new { ok = true });
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
