using Microsoft.AspNetCore.SignalR;
using caso_de_uso_6_ejercer_turno.Services;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Concurrent;

namespace caso_de_uso_6_ejercer_turno.Hubs
{
    public class LobbyHub : Hub
    {
        private readonly LobbyService _lobbyService;
        private static readonly ConcurrentDictionary<string, (string LobbyId, string PlayerId)> _connectionToPlayer = new(); // connectionId -> (lobbyId, playerId)

        public LobbyHub(LobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            if (_connectionToPlayer.TryRemove(Context.ConnectionId, out var info))
            {
                // remove player from lobby when disconnecting
                _lobbyService.RemovePlayer(info.LobbyId, info.PlayerId);
                await BroadcastLobbyUpdate(info.LobbyId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Join requires lobby id and username
        public async Task Join(string lobbyId, string name)
        {
            var color = null as string;
            var player = _lobbyService.AddPlayer(lobbyId, name, color);
            if (player == null)
            {
                await Clients.Caller.SendAsync("ActionDenied", "Lobby no encontrado");
                return;
            }

            _connectionToPlayer[Context.ConnectionId] = (lobbyId, player.IdJugador);

            await Groups.AddToGroupAsync(Context.ConnectionId, lobbyId);

            // Send to caller their assigned id so client can identify itself
            await Clients.Caller.SendAsync("Joined", new { id = player.IdJugador, lobbyId = lobbyId });

            await BroadcastLobbyUpdate(lobbyId);
        }

        public async Task Leave()
        {
            if (_connectionToPlayer.TryRemove(Context.ConnectionId, out var info))
            {
                _lobbyService.RemovePlayer(info.LobbyId, info.PlayerId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, info.LobbyId);
                await BroadcastLobbyUpdate(info.LobbyId);
            }
        }

        public async Task Exit()
        {
            if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var info))
            {
                await Clients.Caller.SendAsync("ActionDenied", "No estás registrado en la sala");
                return;
            }

            var players = _lobbyService.GetLobbyPlayers(info.LobbyId).ToList();
            var caller = players.FirstOrDefault(p => p.IdJugador == info.PlayerId);
            if (caller == null) return;

            if (caller.IsHost)
            {
                // notify all clients that lobby closed
                await Clients.Group(info.LobbyId).SendAsync("LobbyClosed");
                // kick everyone
                var conns = _connectionToPlayer.Where(kv => kv.Value.LobbyId == info.LobbyId).Select(kv => kv.Key).ToList();
                foreach (var c in conns)
                {
                    await Clients.Client(c).SendAsync("YouWereKicked");
                    _connectionToPlayer.TryRemove(c, out _);
                }
                // clear server lobby
                var pids = players.Select(p => p.IdJugador).ToList();
                foreach (var pid in pids)
                {
                    _lobbyService.RemovePlayer(info.LobbyId, pid);
                }
                // publish event
                // LobbyService does not have a Close method currently; leaving players cleared
            }
            else
            {
                // normal guest exit
                _lobbyService.RemovePlayer(info.LobbyId, info.PlayerId);
                _connectionToPlayer.TryRemove(Context.ConnectionId, out _);
                await Clients.Caller.SendAsync("ExitOk");
                await BroadcastLobbyUpdate(info.LobbyId);
            }
        }

        public async Task RemovePlayer(string id)
        {
            if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var info))
            {
                await Clients.Caller.SendAsync("ActionDenied", "No estás registrado en la sala");
                return;
            }

            var players = _lobbyService.GetLobbyPlayers(info.LobbyId).ToList();
            var caller = players.FirstOrDefault(p => p.IdJugador == info.PlayerId);
            if (caller == null || !caller.IsHost)
            {
                await Clients.Caller.SendAsync("ActionDenied", "Solo el anfitrión puede expulsar jugadores");
                return;
            }

            var ok = _lobbyService.RemovePlayer(info.LobbyId, id);
            if (!ok)
            {
                await Clients.Caller.SendAsync("ActionDenied", "No se pudo expulsar al jugador (quizá es el anfitrión)");
                return;
            }

            // notify removed player specifically (if connected)
            var targetConn = _connectionToPlayer.FirstOrDefault(kv => kv.Value.PlayerId == id).Key;
            if (!string.IsNullOrEmpty(targetConn))
            {
                await Clients.Client(targetConn).SendAsync("YouWereKicked");
            }

            await BroadcastLobbyUpdate(info.LobbyId);
        }

        public async Task SetReady(bool ready)
        {
            if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var info))
            {
                await Clients.Caller.SendAsync("ActionDenied", "No estás registrado en la sala");
                return;
            }
            _lobbyService.SetPlayerReady(info.LobbyId, info.PlayerId, ready);
            await BroadcastLobbyUpdate(info.LobbyId);
        }

        public async Task StartGame()
        {
            if (!_connectionToPlayer.TryGetValue(Context.ConnectionId, out var info))
            {
                await Clients.Caller.SendAsync("ActionDenied", "No estás registrado en la sala");
                return;
            }
            var players = _lobbyService.GetLobbyPlayers(info.LobbyId).ToList();
            var caller = players.FirstOrDefault(p => p.IdJugador == info.PlayerId);
            if (caller == null || !caller.IsHost)
            {
                await Clients.Caller.SendAsync("ActionDenied", "Solo el anfitrión puede iniciar la partida");
                return;
            }

            if (!_lobbyService.CanStart(info.LobbyId))
            {
                var notReady = _lobbyService.GetNotReadyPlayers(info.LobbyId).Select(p => p.Nombre).ToArray();
                await Clients.Caller.SendAsync("StartFailed", notReady.Length == 0 ? "No hay suficientes jugadores" : ("Los siguientes jugadores no están listos: " + string.Join(", ", notReady)));
                return;
            }

            var gs = _lobbyService.StartGame(info.LobbyId);
            await Clients.Group(info.LobbyId).SendAsync("GameStarted", gs);
        }

        private async Task BroadcastLobbyUpdate(string lobbyId)
        {
            var players = _lobbyService.GetLobbyPlayers(lobbyId).Select(p => new { id = p.IdJugador, name = p.Nombre, color = p.ColorFichas, isHost = p.IsHost, isReady = p.IsReady });
            var payload = new {
                lobbyId = lobbyId,
                players = players
            };
            await Clients.Group(lobbyId).SendAsync("LobbyUpdated", payload);
        }
    }
}
