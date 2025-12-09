using System;

namespace caso_de_uso_6_ejercer_turno.Models.Events
{
    public class LobbyClosedEvent : IEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Type => nameof(LobbyClosedEvent);
        public string IdPartida { get; set; }
    }
}
