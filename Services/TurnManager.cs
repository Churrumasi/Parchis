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

        /// <summary>
        /// Crea un nuevo TurnManager y genera un identificador corto de sala (IdPartida).
        /// Nota: actualmente el TurnManager mantiene un único estado de partida en memoria.
        /// El IdPartida se genera aquí y se asigna al estado de juego para identificar la
        /// sala en las comunicaciones con clientes (SignalR). Para soportar múltiples lobbies
        /// concurrentes se debe introducir un servicio gestor de lobbies que mantenga
        /// múltiples instancias de GameState por Id.
        /// </summary>
        public TurnManager(IEventBus bus)
        {
            _bus = bus;
            // Inicializar lobby vacío y generar un id corto de sala
            // IdPartida: valor alfanumérico corto generado en el servidor, por ejemplo "a1b2c3d4".
            // Se utiliza para construir links de invitación y para agrupar conexiones SignalR.
            _game.Jugadores = new List<Player>();
            _game.IdPartida = System.Guid.NewGuid().ToString("N").Substring(0, 8);
            _game.Estado = "lobby";
            // OwnerName: si se establece, ese nombre de usuario será considerado el creador/anfitrión
            _game.OwnerName = null;
        }

        /// <summary>
        /// Reserva el nombre de usuario que será el anfitrión de la sala.
        /// Llamar esto antes de AddPlayer si quieres garantizar que un usuario concreto
        /// sea tratado como anfitrión aunque otros se unan antes.
        /// </summary>
        public void SetOwnerName(string ownerName)
        {
            _game.OwnerName = ownerName;
        }

        /// <summary>
        /// Devuelve el identificador de la sala actual asignado por el servidor.
        /// Los clientes deben usar este valor para generar/validar enlaces de invitación
        /// y para unirse a la sala correcta mediante SignalR.
        /// </summary>
        public GameState GetGameState() => _game;

        public string GetLobbyId() => _game.IdPartida;

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
            if (!(count >= minPlayers && count <= maxPlayers)) return false;
            // Only allow starting when ALL non-host players are marked as ready
            // Host does not need to be ready; we require that every invited player has IsReady == true
            return _game.Jugadores.All(p => p.IsHost || p.IsReady);
        }

        /// <summary>
        /// Devuelve la lista de jugadores no anfitrones que aún no están preparados.
        /// Útil para informar por qué no se puede iniciar la partida.
        /// </summary>
        public IEnumerable<Player> GetNotReadyPlayers()
        {
            return _game.Jugadores.Where(p => !p.IsHost && !p.IsReady).ToList();
        }

        /// <summary>
        /// Añade un jugador al lobby actual. Si no existe host en el lobby, el primer jugador
        /// añadido será marcado como anfitrión (IsHost = true).
        /// </summary>
        /// <param name="nombre">Nombre de usuario</param>
        /// <param name="color">Color deseado (opcional)</param>
        /// <returns>Instancia del jugador añadido</returns>
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
                // primer jugador que se añade será anfitrión si no hay host
                // Si se reservó un OwnerName en el turno de creación, el jugador cuyo nombre
                // coincida con OwnerName recibirá IsHost = true; el resto serán invitados.
                IsHost = !_game.Jugadores.Any(j => j.IsHost) && (_game.OwnerName == null || _game.OwnerName == nombre)
             };
            _game.Jugadores.Add(player);

            // publicar evento
            _bus.Publish(new PlayerJoinedEvent { IdJugador = player.IdJugador, Nombre = player.Nombre, IdPartida = _game.IdPartida });

            return player;
        }

        /// <summary>
        /// Marca/desmarca al jugador como preparado en el lobby.
        /// </summary>
        public void SetPlayerReady(string idJugador, bool ready)
        {
            var p = _game.Jugadores.FirstOrDefault(j => j.IdJugador == idJugador);
            if (p != null)
            {
                p.IsReady = ready;
                // publicar evento opcional para EDA
                // _bus.Publish(new PlayerReadyEvent { IdJugador = p.IdJugador, Ready = ready, IdPartida = _game.IdPartida });
            }
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

            _bus.Publish(new PlayerLeftEvent { IdJugador = p.IdJugador, Nombre = p.Nombre, IdPartida = _game.IdPartida });

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

            _bus.Publish(new LobbyStartedEvent { IdPartida = _game.IdPartida });

            return _game;
        }

        /// <summary>
        /// Cierra el lobby actual y publica un evento de cierre.
        /// </summary>
        public void CloseLobby()
        {
            var id = _game.IdPartida;
            _game.Jugadores.Clear();
            _game.Estado = "closed";
            _bus.Publish(new LobbyClosedEvent { IdPartida = id });
        }

    }
}