using System;

namespace caso_de_uso_6_ejercer_turno.Models.Events
{
    public interface IEvent
    {
        DateTime Timestamp { get; }
        string Type { get; }
    }
}
