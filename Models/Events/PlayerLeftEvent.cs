using System;

namespace caso_de_uso_6_ejercer_turno.Models.Events
{
    public class PlayerLeftEvent : IEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Type => nameof(PlayerLeftEvent);
        public string IdJugador { get; set; }
        public string Nombre { get; set; }
        public string IdPartida { get; set; }
    }
}
