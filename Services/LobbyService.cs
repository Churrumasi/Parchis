using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using caso_de_uso_6_ejercer_turno.Models.Domain;
using caso_de_uso_6_ejercer_turno.Models.Events;

namespace caso_de_uso_6_ejercer_turno.Services
{
    public class LobbyService
    {
        private readonly ConcurrentDictionary<string, GameState> _lobbies = new();
        private readonly IEventBus _bus;
        private static readonly List<string> ALL_COLORS = new() { "rojo", "azul", "amarillo", "verde" };

        public LobbyService(IEventBus bus)
        {
            _bus = bus;
        }

        public string CreateLobby(string ownerName = null)
        {
            var id = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            var state = new GameState
            {
                IdPartida = id,
                OwnerName = ownerName,
                Estado = "lobby",
                Jugadores = new List<Player>()
            };
            _lobbies[id] = state;
            return id;
        }

        public bool TryGetLobby(string id, out GameState state)
        {
            if (string.IsNullOrEmpty(id)) { state = null; return false; }
            return _lobbies.TryGetValue(id, out state);
        }

        public IEnumerable<Player> GetLobbyPlayers(string id)
        {
            if (!_lobbies.TryGetValue(id, out var g)) return Enumerable.Empty<Player>();
            return g.Jugadores;
        }

        public Player AddPlayer(string id, string nombre, string color)
        {
            if (!_lobbies.TryGetValue(id, out var g)) return null;
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Jugador";

            var used = g.Jugadores.Select(j => j.ColorFichas).Where(c => !string.IsNullOrEmpty(c)).ToHashSet();
            string chosenColor = color;
            if (string.IsNullOrWhiteSpace(chosenColor) || used.Contains(chosenColor))
            {
                chosenColor = ALL_COLORS.FirstOrDefault(c => !used.Contains(c));
            }
            if (string.IsNullOrWhiteSpace(chosenColor))
            {
                var rnd = new System.Random();
                chosenColor = ALL_COLORS[rnd.Next(ALL_COLORS.Count)];
            }

            var player = new Player
            {
                Nombre = nombre,
                ColorFichas = chosenColor,
                IsHost = !g.Jugadores.Any(j => j.IsHost) && (g.OwnerName == null || g.OwnerName == nombre)
            };
            g.Jugadores.Add(player);
            _bus.Publish(new PlayerJoinedEvent { IdJugador = player.IdJugador, Nombre = player.Nombre, IdPartida = g.IdPartida });
            return player;
        }

        public bool RemovePlayer(string lobbyId, string playerId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var g)) return false;
            var p = g.Jugadores.FirstOrDefault(j => j.IdJugador == playerId);
            if (p == null) return false;
            if (p.IsHost) return false;
            g.Jugadores.Remove(p);
            if (g.IndiceJugadorActual >= g.Jugadores.Count)
            {
                g.IndiceJugadorActual = System.Math.Max(0, g.Jugadores.Count - 1);
            }
            _bus.Publish(new PlayerLeftEvent { IdJugador = p.IdJugador, Nombre = p.Nombre, IdPartida = g.IdPartida });
            return true;
        }

        public bool CanStart(string lobbyId, int minPlayers = 2, int maxPlayers = 4)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var g)) return false;
            var count = g.Jugadores.Count;
            if (!(count >= minPlayers && count <= maxPlayers)) return false;
            return g.Jugadores.All(p => p.IsHost || p.IsReady);
        }

        public GameState StartGame(string lobbyId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var g)) return null;
            var rnd = new System.Random();
            g.Jugadores = g.Jugadores.OrderBy(_ => rnd.Next()).ToList();
            g.IndiceJugadorActual = 0;
            g.Estado = "en curso";
            var first = g.Jugadores.FirstOrDefault();
            if (first != null)
            {
                first.Estado = "jugando";
                _bus.Publish(new TurnoIniciadoEvent { IdJugador = first.IdJugador });
            }
            _bus.Publish(new LobbyStartedEvent { IdPartida = g.IdPartida });
            return g;
        }

        public GameState GetGameState(string lobbyId)
        {
            _lobbies.TryGetValue(lobbyId, out var g);
            return g;
        }

        public bool SetPlayerReady(string lobbyId, string playerId, bool ready)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var g)) return false;
            var p = g.Jugadores.FirstOrDefault(j => j.IdJugador == playerId);
            if (p == null) return false;
            p.IsReady = ready;
            return true;
        }

        public IEnumerable<Player> GetNotReadyPlayers(string lobbyId)
        {
            if (!_lobbies.TryGetValue(lobbyId, out var g)) return Enumerable.Empty<Player>();
            return g.Jugadores.Where(p => !p.IsHost && !p.IsReady).ToList();
        }
    }
}
