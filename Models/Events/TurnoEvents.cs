using System;

namespace caso_de_uso_6_ejercer_turno.Models.Events
{
    public class TurnoIniciadoEvent : IEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Type => nameof(TurnoIniciadoEvent);
        public string IdJugador { get; set; }
    }

    public class TurnoFinalizadoEvent : IEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Type => nameof(TurnoFinalizadoEvent);
        public string IdJugador { get; set; }
    }
}
