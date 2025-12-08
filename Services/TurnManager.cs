using System.Linq;
using System.Collections.Generic;
using caso_de_uso_6_ejercer_turno.Models.Domain;
using caso_de_uso_6_ejercer_turno.Models.Events;

namespace caso_de_uso_6_ejercer_turno.Services
{
    public class TurnManager
    {
        private readonly GameState _game = new();
        private readonly IEventBus _bus;
        private static readonly List<string> ALL_COLORS = new() { "rojo", "azul", "amarillo", "verde" };

        public TurnManager(IEventBus bus)
        {
            _bus = bus;
            // Inicializar con fichas en posiciones visibles para testing
            _game.Jugadores.Add(new Player {
                Nombre = "Ana",
                ColorFichas = "rojo",
                PosicionesFichas = new List<int> { 38, 39, 40, -1 },
                IsHost = true
            });
            _game.Jugadores.Add(new Player {
                Nombre = "Luis",
                ColorFichas = "azul",
                PosicionesFichas = new List<int> { 12, 13, 14, -1 }
            });
            _game.Jugadores.Add(new Player {
                Nombre = "María",
                ColorFichas = "amarillo",
                PosicionesFichas = new List<int> { 4, 5, 6, -1 }
            });
            _game.Jugadores.Add(new Player {
                Nombre = "José",
                ColorFichas = "verde",
                PosicionesFichas = new List<int> { 55, 56, 57, -1 }
            });
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

        // LOBBY / INICIO DE PARTIDA
        public IEnumerable<Player> GetLobbyPlayers() => _game.Jugadores;

        public bool CanStart(int minPlayers = 2, int maxPlayers = 4)
        {
            var count = _game.Jugadores.Count;
            return count >= minPlayers && count <= maxPlayers;
        }

        public Player AddPlayer(string nombre, string color)
        {
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Jugador";

            // Determine color: if provided and unused accept, otherwise pick an unused color
            var used = _game.Jugadores.Select(j => j.ColorFichas).Where(c => !string.IsNullOrEmpty(c)).ToHashSet();
            string chosenColor = color; 
            if (string.IsNullOrWhiteSpace(chosenColor) || used.Contains(chosenColor))
            {
                chosenColor = ALL_COLORS.FirstOrDefault(c => !used.Contains(c));
            }
            // if still null, fallback to a random color
            if (string.IsNullOrWhiteSpace(chosenColor))
            {
                var rnd = new System.Random();
                chosenColor = ALL_COLORS[rnd.Next(ALL_COLORS.Count)];
            }

            var player = new Player
            {
                Nombre = nombre,
                ColorFichas = chosenColor,
                // si no hay anfitrión, el primer jugador añadido será el anfitrión
                IsHost = !_game.Jugadores.Any(j => j.IsHost)
            };
            _game.Jugadores.Add(player);
            return player;
        }

        public bool RemovePlayer(string idJugador)
        {
            var p = _game.Jugadores.FirstOrDefault(j => j.IdJugador == idJugador);
            if (p == null) return false;
            // No permitir eliminar al anfitrión
            if (p.IsHost) return false;
            _game.Jugadores.Remove(p);
            // Adjust index if needed
            if (_game.IndiceJugadorActual >= _game.Jugadores.Count)
            {
                _game.IndiceJugadorActual = System.Math.Max(0, _game.Jugadores.Count - 1);
            }
            return true;
        }

        public GameState StartGameAndShuffleOrder()
        {
            // Shuffle players randomly
            var rnd = new System.Random();
            _game.Jugadores = _game.Jugadores.OrderBy(_ => rnd.Next()).ToList();
            _game.IndiceJugadorActual = 0;
            _game.Estado = "en curso";
            // publish event that first turn started
            var first = GetJugadorActual();
            if (first != null)
            {
                first.Estado = "jugando";
                _bus.Publish(new TurnoIniciadoEvent { IdJugador = first.IdJugador });
            }
            return _game;
        }

    }
}