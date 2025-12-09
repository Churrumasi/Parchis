using System;

namespace caso_de_uso_6_ejercer_turno.Models.Events
{
    public class LobbyStartedEvent : IEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
        public string Type => nameof(LobbyStartedEvent);
        public string IdPartida { get; set; }
    }
}
