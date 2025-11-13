using System;

namespace caso_de_uso_6_ejercer_turno.Models.Events
{
    public class FichaMovidaEvent : IEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Type => nameof(FichaMovidaEvent);
        public string IdJugador { get; set; }
        public int IndiceFicha { get; set; }
        public int Desde { get; set; }
        public int Hasta { get; set; }
    }
}
