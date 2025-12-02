using System.Linq;
using caso_de_uso_6_ejercer_turno.Models.Domain;
using caso_de_uso_6_ejercer_turno.Models.Events;

namespace caso_de_uso_6_ejercer_turno.Services
{
    public class TurnManager
    {
        private readonly GameState _game = new();
        private readonly IEventBus _bus;

        public TurnManager(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        public void AsignarPlayerNumberALocal(string username, int playerNumber, string color)
        {
            if (_game.Jugadores == null || _game.Jugadores.Count == 0) return;

            // Normalizamos el nombre recibido
            string usuarioNorm = (username ?? "").Trim().ToUpperInvariant();

            // Intentar buscar jugador por nombre (sin acentos / case-insensitive)
            var jugador = _game.Jugadores
                .FirstOrDefault(j => !string.IsNullOrWhiteSpace(j.Nombre) &&
                                     j.Nombre.Trim().ToUpperInvariant().Contains(usuarioNorm));

            if (jugador != null)
            {
                // Si existe, asignamos IdJugador y color
                jugador.IdJugador = playerNumber.ToString();
                jugador.ColorFichas = color ?? jugador.ColorFichas;
                return;
            }

            // Si no encontramos por nombre, intentamos asignar por índice playerNumber-1
            int idx = playerNumber - 1;
            if (idx >= 0 && idx < _game.Jugadores.Count)
            {
                var j = _game.Jugadores[idx];
                j.IdJugador = playerNumber.ToString();
                j.ColorFichas = color ?? j.ColorFichas;
                // opcional: poner su nombre al username recibido
                j.Nombre = string.IsNullOrWhiteSpace(username) ? j.Nombre : username;
                return;
            }

            // Si no cabe, como fallback creamos/añadimos un nuevo jugador
            var nuevo = new Player
            {
                IdJugador = playerNumber.ToString(),
                Nombre = username ?? $"Jugador{playerNumber}",
                ColorFichas = color ?? "sin_color",
                PosicionesFichas = new List<int> { -1, -1, -1, -1 }
            };

            _game.Jugadores.Add(nuevo);
        }


        public GameState GetGameState() => _game;

        public Player GetJugadorActual()
        {
            if (_game.Jugadores == null || _game.Jugadores.Count == 0) return null;
            return _game.Jugadores[_game.IndiceJugadorActual];
        }

        public void IniciarTurno()
        {
            var jugador = GetJugadorActual();
            if (jugador == null) return;
            jugador.Estado = "jugando";
            _bus.Publish(new TurnoIniciadoEvent { IdJugador = jugador.IdJugador });
        }

        public void FinalizarTurno()
        {
            var jugador = GetJugadorActual();
            if (jugador != null) jugador.Estado = "esperando";
            _game.IndiceJugadorActual = (_game.IndiceJugadorActual + 1) % System.Math.Max(1, _game.Jugadores.Count);
            var prox = GetJugadorActual();
            _bus.Publish(new TurnoFinalizadoEvent { IdJugador = jugador?.IdJugador ?? "" });
            _bus.Publish(new TurnoIniciadoEvent { IdJugador = prox?.IdJugador });
        }

        public void ProcesarDadoLanzado(string idJugador, int valor)
        {
            _bus.Publish(new DadoLanzadoEvent { IdJugador = idJugador, Valor = valor });
        }

        public void ProcesarMovimiento(string idJugador, int indiceFicha, int desde, int hasta)
        {
            // Actualizar posición de la ficha en el estado del juego
            var jugador = _game.Jugadores.FirstOrDefault(j => j.IdJugador == idJugador);
            if (jugador != null && jugador.PosicionesFichas != null && indiceFicha >= 0 && indiceFicha < jugador.PosicionesFichas.Count)
            {
                jugador.PosicionesFichas[indiceFicha] = hasta;
            }

            // Publicar evento para que el orquestador pueda reaccionar (ej. finalizar turno)
            _bus.Publish(new FichaMovidaEvent { IdJugador = idJugador, IndiceFicha = indiceFicha, Desde = desde, Hasta = hasta });
        }
    }
}