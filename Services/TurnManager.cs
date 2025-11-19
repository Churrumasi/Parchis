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
            _bus = bus;
            // Inicializar con fichas en posiciones visibles para testing
            _game.Jugadores.Add(new Player { 
                Nombre = "Ana", 
                ColorFichas = "rojo",
                PosicionesFichas = new List<int> { 38, 39, 40, -1 }
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
    }
}