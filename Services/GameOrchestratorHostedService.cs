using Microsoft.Extensions.Hosting;
using caso_de_uso_6_ejercer_turno.Models.Events;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace caso_de_uso_6_ejercer_turno.Services
{
    public class GameOrchestratorHostedService : IHostedService
    {
        private readonly IEventBus _bus;
        private readonly TurnManager _turnManager;

        public GameOrchestratorHostedService(IEventBus bus, TurnManager turnManager)
        {
            _bus = bus;
            _turnManager = turnManager;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _bus.Subscribe<DadoLanzadoEvent>(HandleDado);
            _bus.Subscribe<FichaMovidaEvent>(HandleFichaMovida);
            _bus.Subscribe<TurnoIniciadoEvent>(t => {
                Console.WriteLine($"[Orquestador] Turno iniciado: {t.IdJugador}");
            });
            return Task.CompletedTask;
        }

        private void HandleDado(DadoLanzadoEvent e)
        {
            Console.WriteLine($"[Orquestador] Dado lanzado por {e.IdJugador}: {e.Valor}");
        }

        private void HandleFichaMovida(FichaMovidaEvent e)
        {
            Console.WriteLine($"[Orquestador] Ficha movida por {e.IdJugador}: ficha {e.IndiceFicha} de {e.Desde} a {e.Hasta}");
            _turnManager.FinalizarTurno();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
