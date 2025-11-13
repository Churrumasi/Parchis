using caso_de_uso_6_ejercer_turno.Models.Events;
using System;

namespace caso_de_uso_6_ejercer_turno.Services
{
    public interface IEventBus
    {
        void Publish(IEvent evt);
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent;
    }
}
