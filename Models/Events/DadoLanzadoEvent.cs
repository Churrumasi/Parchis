using System;

namespace caso_de_uso_6_ejercer_turno.Models.Events
{
    public class DadoLanzadoEvent : IEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Type => nameof(DadoLanzadoEvent);
        public string IdJugador { get; set; }
        public int Valor { get; set; }
    }
}
