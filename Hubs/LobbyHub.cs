using Microsoft.AspNetCore.SignalR;
using caso_de_uso_6_ejercer_turno.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

namespace caso_de_uso_6_ejercer_turno.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly TurnManager _turnManager;
        private static readonly ConcurrentDictionary<string, string> _connectionToPlayer = new(); // connectionId -> playerId

        public LobbyHub(TurnManager turnManager)
        {
            _turnManager = turnManager;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            if (_connectionToPlayer.TryRemove(Context.ConnectionId, out var pid))
            {
                // remove player from lobby when disconnecting
                _turnManager.RemovePlayer(pid);
                await BroadcastLobbyUpdate();
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task Join(string name)
        {
            var color = null as string;
            var player = _turnManager.AddPlayer(name, color);
            _connectionToPlayer[Context.ConnectionId] = player.IdJugador;

            await Groups.AddToGroupAsync(Context.ConnectionId, _turnManager.GetLobbyId());

            // Send to caller their assigned id so client can identify itself
            await Clients.Caller.SendAsync("Joined", new { id = player.IdJugador, lobbyId = _turnManager.GetLobbyId() });

            await BroadcastLobbyUpdate();
        }

        public async Task Leave()
        {
            if (_connectionToPlayer.TryRemove(Context.ConnectionId, out var pid))
            {
                _turnManager.RemovePlayer(pid);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, _turnManager.GetLobbyId());
                await BroadcastLobbyUpdate();
            }
        }

        public async Task Exit()
        {
            // If caller is the host, close lobby and kick everyone
            if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var callerId))
            {
                await Clients.Caller.SendAsync("ActionDenied", "No estás registrado en la sala");
                return;
            }

            var caller = _turnManager.GetLobbyPlayers().FirstOrDefault(p => p.IdJugador == callerId);
            if (caller == null) return;

            if (caller.IsHost)
            {
                // notify all clients that lobby closed
                await Clients.Group(_turnManager.GetLobbyId()).SendAsync("LobbyClosed");
                // kick everyone
                var conns = _connectionToPlayer.Keys.ToList();
                foreach (var c in conns)
                {
                    await Clients.Client(c).SendAsync("YouWereKicked");
                    _connectionToPlayer.TryRemove(c, out _);
                }
                // clear server lobby
                var players = _turnManager.GetLobbyPlayers().Select(p => p.IdJugador).ToList();
                foreach (var pid in players)
                {
                    _turnManager.RemovePlayer(pid);
                }
                // publish event
                _turnManager.CloseLobby();
            }
            else
            {
                // normal guest exit
                _turnManager.RemovePlayer(callerId);
                _connectionToPlayer.TryRemove(Context.ConnectionId, out _);
                await Clients.Caller.SendAsync("ExitOk");
                await BroadcastLobbyUpdate();
            }
        }

        public async Task RemovePlayer(string id)
        {
            // only host can remove
            if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var callerId))
            {
                await Clients.Caller.SendAsync("ActionDenied", "No estás registrado en la sala");
                return;
            }

            var caller = _turnManager.GetLobbyPlayers().FirstOrDefault(p => p.IdJugador == callerId);
            if (caller == null || !caller.IsHost)
            {
                await Clients.Caller.SendAsync("ActionDenied", "Solo el anfitrión puede expulsar jugadores");
                return;
            }

            var ok = _turnManager.RemovePlayer(id);
            if (!ok)
            {
                await Clients.Caller.SendAsync("ActionDenied", "No se pudo expulsar al jugador (quizá es el anfitrión)");
                return;
            }

            // notify removed player specifically (if connected)
            var targetConn = _connectionToPlayer.FirstOrDefault(kv => kv.Value == id).Key;
            if (!string.IsNullOrEmpty(targetConn))
            {
                await Clients.Client(targetConn).SendAsync("YouWereKicked");
            }

            await BroadcastLobbyUpdate();
        }

        public async Task SetReady(bool ready)
        {
            if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var pid))
            {
                await Clients.Caller.SendAsync("ActionDenied", "No estás registrado en la sala");
                return;
            }
            _turnManager.SetPlayerReady(pid, ready);
            await BroadcastLobbyUpdate();
        }

        public async Task StartGame()
        {
            if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var callerId))
            {
                await Clients.Caller.SendAsync("ActionDenied", "No estás registrado en la sala");
                return;
            }
            var caller = _turnManager.GetLobbyPlayers().FirstOrDefault(p => p.IdJugador == callerId);
            if (caller == null || !caller.IsHost)
            {
                await Clients.Caller.SendAsync("ActionDenied", "Solo el anfitrión puede iniciar la partida");
                return;
            }

            if (!_turnManager.CanStart())
            {
                var notReady = _turnManager.GetNotReadyPlayers().Select(p => p.Nombre).ToArray();
                await Clients.Caller.SendAsync("StartFailed", notReady.Length == 0 ? "No hay suficientes jugadores" : ("Los siguientes jugadores no están listos: " + string.Join(", ", notReady)));
                return;
            }

            var gs = _turnManager.StartGameAndShuffleOrder();
            await Clients.Group(_turnManager.GetLobbyId()).SendAsync("GameStarted", gs);
        }

        private async Task BroadcastLobbyUpdate()
        {
            var payload = new {
                lobbyId = _turnManager.GetLobbyId(),
                players = _turnManager.GetLobbyPlayers().Select(p => new { id = p.IdJugador, name = p.Nombre, color = p.ColorFichas, isHost = p.IsHost, isReady = p.IsReady })
            };
            await Clients.Group(_turnManager.GetLobbyId()).SendAsync("LobbyUpdated", payload);
        }
    }
}
