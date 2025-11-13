using caso_de_uso_6_ejercer_turno.Models.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace caso_de_uso_6_ejercer_turno.Services
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, List<Delegate>> _handlers = new();

        public void Publish(IEvent evt)
        {
            var t = evt.GetType();
            if (_handlers.TryGetValue(t, out var list))
            {
                foreach (var d in list.ToList())
                {
                    try
                    {
                        d.DynamicInvoke(evt);
                    }
                    catch
                    {
                        // ignore handler errors in demo
                    }
                }
            }
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IEvent
        {
            var t = typeof(TEvent);
            _handlers.AddOrUpdate(t,
                (_) => new List<Delegate> { handler },
                (_, existing) =>
                {
                    existing.Add(handler);
                    return existing;
                });
        }
    }
}
